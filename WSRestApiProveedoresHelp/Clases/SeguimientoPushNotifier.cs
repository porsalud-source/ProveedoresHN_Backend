using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using WSRestApiProveedoresHelp.Models;

namespace WSRestApiProveedoresHelp.Clases
{
    internal sealed class SeguimientoPushNotifier
    {
        private const string OneSignalMessagesUrl = "https://onesignal.com/api/v1/notifications";
        private static readonly IDictionary<int, TrackingNotification> TrackingNotifications =
            new Dictionary<int, TrackingNotification>
            {
                {
                    53,
                    new TrackingNotification(
                        "El agente asignado se dirige hacia la ubicación de tu atención #{0}.")
                },
                {
                    52,
                    new TrackingNotification(
                        "El agente ya se encuentra en la ubicación indicada para tu atención #{0}.")
                },
                {
                    51,
                    new TrackingNotification(
                        "Tu agente ya está contigo y está listo para comenzar el servicio de la atención #{0}.")
                },
                {
                    54,
                    new TrackingNotification(
                        "El agente reportó como finalizado el servicio de tu atención #{0}.")
                }
            };

        private readonly helpEntities database;

        internal SeguimientoPushNotifier(helpEntities database)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        internal bool TryNotify(int attentionId, int trackingType, out string detail)
        {
            detail = string.Empty;

            if (!TrackingNotifications.TryGetValue(trackingType, out var notification))
            {
                detail = "El seguimiento no requiere notificacion push.";
                return true;
            }

            var attention = database.help_Atenciones
                .AsNoTracking()
                .FirstOrDefault(item => item.Id == attentionId);

            if (attention == null || !attention.RefClienteId.HasValue)
            {
                detail = "No fue posible resolver el cliente propietario de la atencion.";
                return false;
            }

            var client = ResolveClient(attention.RefClienteId.Value);
            if (client == null)
            {
                detail = "No se encontro el usuario de Asistencias HN asociado a la atencion.";
                return false;
            }

            var appId = ConfigurationManager.AppSettings["OneSignalAsistenciasHNAppId"];
            var apiKey = ConfigurationManager.AppSettings["OneSignalAsistenciasHNRestApiKey"];
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(apiKey))
            {
                detail = "Falta configurar OneSignalAsistenciasHNAppId u OneSignalAsistenciasHNRestApiKey en el servidor.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(client.TokenPush))
            {
                detail = "El cliente no tiene un TokenPush activo registrado.";
                return false;
            }

            var serviceName = ResolveServiceName(attention.RefTipoServicioId);
            var title = string.Format("Atn #{0} - {1}", attentionId, serviceName);
            var message = string.Format(notification.Message, attentionId);
            var subscriptionPayload = new
            {
                app_id = appId,
                // OneSignal conserva el nombre historico player_id para el
                // identificador actual de la suscripcion push.
                include_player_ids = new[] { client.TokenPush.Trim() },
                headings = new { es = title, en = title },
                contents = new { es = message, en = message },
                data = new { IdAtencion = attentionId, TipoSeguimiento = trackingType },
                small_icon = "ic_stat_onesignal_default",
                priority = 10,
                ios_sound = "default"
            };

            return TrySend(apiKey, subscriptionPayload, out detail);
        }

        internal bool TryNotifyConfirmation(int attentionId, int status, out string detail)
        {
            detail = string.Empty;

            var attention = database.help_Atenciones
                .AsNoTracking()
                .FirstOrDefault(item => item.Id == attentionId);

            if (attention == null || !attention.RefClienteId.HasValue)
            {
                detail = "No fue posible resolver el cliente propietario de la atencion.";
                return false;
            }

            var client = ResolveClient(attention.RefClienteId.Value);
            if (client == null)
            {
                detail = "No se encontro el usuario de Asistencias HN asociado a la atencion.";
                return false;
            }

            var appId = ConfigurationManager.AppSettings["OneSignalAsistenciasHNAppId"];
            var apiKey = ConfigurationManager.AppSettings["OneSignalAsistenciasHNRestApiKey"];
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(apiKey))
            {
                detail = "Falta configurar OneSignalAsistenciasHNAppId u OneSignalAsistenciasHNRestApiKey en el servidor.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(client.TokenPush))
            {
                detail = "El cliente no tiene un TokenPush activo registrado.";
                return false;
            }

            var serviceName = ResolveServiceName(attention.RefTipoServicioId);
            var title = string.Format("Atn #{0} - {1}", attentionId, serviceName);
            var isCancellation = IsCancellationConfirmationStatus(status);
            var message = isCancellation
                ? "El agente de servicio cancel\u00f3 la solicitud. Pronto reasignaremos a otro de nuestros expertos. Para cualquier duda puedes comunicarte con nuestros agentes de atenci\u00f3n al cliente al +50422755456."
                : "El agente de servicio acept\u00f3 la solicitud. Pronto recibir\u00e1s actualizaciones del servicio. Para cualquier duda puedes comunicarte con nuestros agentes de atenci\u00f3n al cliente al +50422755456.";

            var subscriptionPayload = new
            {
                app_id = appId,
                include_player_ids = new[] { client.TokenPush.Trim() },
                headings = new { es = title, en = title },
                contents = new { es = message, en = message },
                data = new { IdAtencion = attentionId, EstadoConfirmacion = status },
                small_icon = "ic_stat_onesignal_default",
                priority = 10,
                ios_sound = "default"
            };

            return TrySend(apiKey, subscriptionPayload, out detail);
        }

        private static bool IsCancellationConfirmationStatus(int status)
        {
            return status == 1 || status == 3 || status == 4 || status == 5 || status == 8 || status == 9;
        }

        private ClientPushData ResolveClient(int clientReferenceId)
        {
            const string sql = @"
SELECT TOP (1)
    TokenPush
FROM help_clt.dbo.help_clt_Usuarios
WHERE Id = @ClientReferenceId
   OR RefPersonaId = @ClientReferenceId
ORDER BY CASE WHEN Id = @ClientReferenceId THEN 0 ELSE 1 END, Id DESC";

            return database.Database.SqlQuery<ClientPushData>(
                sql,
                new SqlParameter("@ClientReferenceId", clientReferenceId))
                .FirstOrDefault();
        }

        private string ResolveServiceName(int? serviceId)
        {
            if (!serviceId.HasValue)
            {
                return "SERVICIO";
            }

            const string sql = @"
SELECT TOP (1)
    TipoAtencion
FROM help.dbo.help_Servicios
WHERE Id = @ServiceId";

            var service = database.Database.SqlQuery<ServicePushData>(
                sql,
                new SqlParameter("@ServiceId", serviceId.Value))
                .FirstOrDefault();

            if (service == null || string.IsNullOrWhiteSpace(service.TipoAtencion))
            {
                return "SERVICIO";
            }

            return service.TipoAtencion.Trim();
        }

        private static bool TrySend(string apiKey, object payload, out string detail)
        {
            try
            {
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, OneSignalMessagesUrl))
                {
                    var normalizedApiKey = apiKey.Trim();
                    if (normalizedApiKey.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        normalizedApiKey = normalizedApiKey.Substring(6).Trim();
                    }
                    else if (normalizedApiKey.StartsWith("Key ", StringComparison.OrdinalIgnoreCase))
                    {
                        normalizedApiKey = normalizedApiKey.Substring(4).Trim();
                    }

                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", normalizedApiKey);
                    request.Content = new StringContent(
                        JsonConvert.SerializeObject(payload),
                        Encoding.UTF8,
                        "application/json");

                    var response = client.SendAsync(request).GetAwaiter().GetResult();
                    var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        detail = string.Format(
                            "OneSignal respondio {0}: {1}",
                            (int)response.StatusCode,
                            responseBody);
                        return false;
                    }

                    var result = string.IsNullOrWhiteSpace(responseBody)
                        ? null
                        : JObject.Parse(responseBody);
                    var notificationId = result?["id"]?.ToString();

                    if (string.IsNullOrWhiteSpace(notificationId))
                    {
                        detail = "OneSignal acepto la solicitud, pero no encontro una suscripcion valida.";
                        return false;
                    }

                    detail = "Notificacion enviada. Id OneSignal: " + notificationId;
                    return true;
                }
            }
            catch (Exception exception)
            {
                detail = "Error enviando notificacion OneSignal: " + exception.Message;
                return false;
            }
        }

        private sealed class ClientPushData
        {
            public string TokenPush { get; set; }
        }

        private sealed class ServicePushData
        {
            public string TipoAtencion { get; set; }
        }

        private sealed class TrackingNotification
        {
            internal TrackingNotification(string message)
            {
                Message = message;
            }

            internal string Message { get; }
        }
    }
}
