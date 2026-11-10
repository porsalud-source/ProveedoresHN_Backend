using System;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using WSRestApiProveedoresHelp.Models;

namespace WSRestApiProveedoresHelp.Controllers
{
    public sealed class TrackingPointRequest
    {
        public Guid TrackingPointId { get; set; }
        public Guid SessionId { get; set; }
        public int RefAtencionId { get; set; }
        public int RefUsuarioId { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public long CapturedAt { get; set; }
        public DateTime FechaRegistro { get; set; }
        public double? Accuracy { get; set; }
    }

    public sealed class TrackingPointRow
    {
        public long Sequence { get; set; }
        public Guid TrackingPointId { get; set; }
        public Guid SessionId { get; set; }
        public int AttentionId { get; set; }
        public int AgentId { get; set; }
        public int ProviderId { get; set; }
        public DateTime CapturedAt { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
    }

    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/Proveedor")]
    public sealed class TrackingController : ApiController
    {
        private readonly help_prvEntities db = new help_prvEntities();
        private readonly helpEntities core = new helpEntities();

        private help_prv_F_ObtenerPassword_Result CurrentAgent()
        {
            return db.help_prv_F_ObtenerPassword(User.Identity.Name).FirstOrDefault();
        }

        private bool CanView(help_prv_F_ObtenerPassword_Result agent, int attentionId)
        {
            // Legacy attention assignment stores the agent ID, not the supplier company ID.
            var assignedAgentId = core.help_Atenciones.Where(x => x.Id == attentionId)
                .Select(x => x.RefProveedorId).FirstOrDefault();
            if (!assignedAgentId.HasValue) return false;
            if (assignedAgentId.Value == agent.Id) return true;
            return agent.Admin == 1 && db.help_prv_F_ObtenerAgentesProveedor(agent.RefProveedorId)
                .Any(x => x.Id == assignedAgentId.Value);
        }

        [HttpPost, Route("RegistrarPuntoGps")]
        public IHttpActionResult Register(TrackingPointRequest point)
        {
            double latitude, longitude;
            if (point == null || point.TrackingPointId == Guid.Empty || point.SessionId == Guid.Empty || point.RefAtencionId <= 0 ||
                !double.TryParse(point.Latitud, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude) ||
                !double.TryParse(point.Longitud, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude) ||
                double.IsNaN(latitude) || double.IsInfinity(latitude) || Math.Abs(latitude) > 90 ||
                double.IsNaN(longitude) || double.IsInfinity(longitude) || Math.Abs(longitude) > 180 ||
                (point.Accuracy.HasValue && (double.IsNaN(point.Accuracy.Value) || double.IsInfinity(point.Accuracy.Value) || point.Accuracy < 0)))
                return BadRequest("Punto GPS invalido.");
            DateTime captured;
            try { captured = DateTimeOffset.FromUnixTimeMilliseconds(point.CapturedAt).UtcDateTime; }
            catch (ArgumentOutOfRangeException) { return BadRequest("Fecha GPS invalida."); }
            if (captured.Year < 2000 || captured > DateTime.UtcNow.AddMinutes(1)) return BadRequest("Fecha GPS invalida.");
            var supplied = point.FechaRegistro.Kind == DateTimeKind.Local ? point.FechaRegistro.ToUniversalTime() : DateTime.SpecifyKind(point.FechaRegistro, DateTimeKind.Utc);
            if (Math.Abs((supplied - captured).TotalMilliseconds) > 1) return BadRequest("Las fechas GPS no coinciden.");
            try
            {
                var agent = CurrentAgent();
                if (agent == null || agent.Id != point.RefUsuarioId || !CanView(agent, point.RefAtencionId)) return StatusCode(HttpStatusCode.Forbidden);
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    // The unique point ID and its legacy log are committed in the same transaction.
                    var existing = db.Database.SqlQuery<TrackingPointRow>("SELECT * FROM dbo.help_prv_TrackingPoints WITH (UPDLOCK,HOLDLOCK) WHERE TrackingPointId=@Id", new SqlParameter("@Id", point.TrackingPointId)).SingleOrDefault();
                    if (existing != null)
                    {
                        if (existing.AgentId != agent.Id || existing.ProviderId != agent.RefProveedorId || existing.AttentionId != point.RefAtencionId ||
                            existing.SessionId != point.SessionId || existing.Latitude != latitude || existing.Longitude != longitude ||
                            existing.CapturedAt != captured || existing.Accuracy != point.Accuracy) return StatusCode(HttpStatusCode.Conflict);
                        transaction.Commit();
                        return Ok(new { point.TrackingPointId, Duplicate = true });
                    }
                    db.Database.ExecuteSqlCommand(@"INSERT INTO dbo.help_prv_TrackingPoints
                        (TrackingPointId,SessionId,AttentionId,AgentId,ProviderId,CapturedAt,Latitude,Longitude,Accuracy)
                        VALUES (@Id,@Session,@Attention,@Agent,@Provider,@Captured,@Lat,@Lng,@Accuracy)",
                        new SqlParameter("@Id", point.TrackingPointId), new SqlParameter("@Session", point.SessionId),
                        new SqlParameter("@Attention", point.RefAtencionId), new SqlParameter("@Agent", agent.Id),
                        new SqlParameter("@Provider", agent.RefProveedorId), new SqlParameter("@Captured", SqlDbType.DateTime2) { Value = captured },
                        new SqlParameter("@Lat", latitude), new SqlParameter("@Lng", longitude), new SqlParameter("@Accuracy", SqlDbType.Float) { Value = (object)point.Accuracy ?? DBNull.Value });
                    var answer = new ObjectParameter("Respuesta", typeof(int));
                    var message = new ObjectParameter("mensaje", typeof(string));
                    db.help_prv_P_Log_Insert_GPS(point.Longitud, point.Latitud, point.RefAtencionId, captured, agent.Id, "PRO_MOV", 0, answer, message);
                    if (Convert.ToInt32(answer.Value) != 1) return BadRequest("No fue posible almacenar el punto GPS.");
                    transaction.Commit();
                    return Ok(new { point.TrackingPointId, Duplicate = false });
                }
            }
            catch (Exception error)
            {
                Trace.TraceError("GPS register failed: {0}", error.GetType().Name);
                return InternalServerError();
            }
        }

        [HttpGet, Route("HistorialGpsAtencion")]
        public IHttpActionResult History(int IdAtencion, long after = 0)
        {
            if (IdAtencion <= 0 || after < 0) return BadRequest("Consulta GPS invalida.");
            try
            {
                var agent = CurrentAgent();
                if (agent == null || !CanView(agent, IdAtencion)) return StatusCode(HttpStatusCode.Forbidden);
                // Cursor follows ingestion sequence, so late offline uploads are not skipped.
                var rows = db.Database.SqlQuery<TrackingPointRow>(@"SELECT TOP (1001) * FROM dbo.help_prv_TrackingPoints
                    WHERE ProviderId=@Provider AND AttentionId=@Attention AND [Sequence]>@After AND (@Admin=1 OR AgentId=@Agent)
                    ORDER BY [Sequence]", new SqlParameter("@Provider", agent.RefProveedorId), new SqlParameter("@Attention", IdAtencion),
                    new SqlParameter("@After", after), new SqlParameter("@Admin", agent.Admin == 1 ? 1 : 0), new SqlParameter("@Agent", agent.Id)).ToList();
                var page = rows.Take(1000).ToList();
                var allowedAgents = agent.Admin == 1 ? db.help_prv_F_ObtenerAgentesProveedor(agent.RefProveedorId).Select(x => x.Id).ToList() : new System.Collections.Generic.List<int> { agent.Id };
                // Read committed history; exclude modern copies before paging to avoid temporary parallel traces.
                var legacy = after == 0 ? db.Database.SqlQuery<help_prv_F_Log_GPS_Result>(@"SELECT g.Id,g.Longitud,g.Latitud,g.RefAtencionId,g.FechaRegistro,g.RefUsuarioId,g.Tipo,g.Contador
                    FROM help.dbo.help_Log_GPS g
                    WHERE g.RefAtencionId=@Attention AND g.Tipo='PRO_MOV' AND g.RefUsuarioId IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM dbo.help_prv_TrackingPoints t
                        WHERE t.ProviderId=@Provider AND t.AttentionId=g.RefAtencionId AND t.AgentId=g.RefUsuarioId
                        AND t.CapturedAt BETWEEN DATEADD(millisecond,-5,CAST(g.FechaRegistro AS datetime2)) AND DATEADD(millisecond,5,CAST(g.FechaRegistro AS datetime2))
                        AND t.Latitude=TRY_CONVERT(float,g.Latitud) AND t.Longitude=TRY_CONVERT(float,g.Longitud))",
                    new SqlParameter("@Attention", IdAtencion), new SqlParameter("@Provider", agent.RefProveedorId)).ToList()
                    .Where(x => x.RefUsuarioId.HasValue && allowedAgents.Contains(x.RefUsuarioId.Value))
                    .Select(x => new { x.Id, IdAtencion = x.RefAtencionId, x.RefUsuarioId, x.Latitud, x.Longitud,
                        FechaRegistro = x.FechaRegistro.HasValue ? DateTime.SpecifyKind(x.FechaRegistro.Value, DateTimeKind.Utc).ToString("O") : null }).ToArray() : null;
                return Ok(new {
                    Points = page.Select(x => new { x.TrackingPointId, x.SessionId, RefAtencionId = x.AttentionId, RefUsuarioId = x.AgentId,
                        Latitud = x.Latitude, Longitud = x.Longitude, x.Accuracy, CapturedAt = new DateTimeOffset(DateTime.SpecifyKind(x.CapturedAt, DateTimeKind.Utc)).ToUnixTimeMilliseconds() }).ToArray(),
                    LegacyPoints = legacy, HasMore = rows.Count > 1000,
                    NextCursor = (page.Count == 0 ? after : page.Last().Sequence).ToString(CultureInfo.InvariantCulture)
                });
            }
            catch (Exception error)
            {
                Trace.TraceError("GPS history failed: {0}", error.GetType().Name);
                return InternalServerError();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); core.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
