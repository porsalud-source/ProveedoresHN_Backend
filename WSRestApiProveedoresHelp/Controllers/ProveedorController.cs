using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using WSRestApiProveedoresHelp.Clases;
using WSRestApiProveedoresHelp.Models;
using static WSRestApiProveedoresHelp.Clases.ApiData;

namespace WSRestApiProveedoresHelp.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    [RoutePrefix("api/Proveedor")]
    public class ProveedorController : ApiController
    {
        //13 es Produccion
        //10 es Pruebas

        private help_prvEntities db = new help_prvEntities();
        private helpEntities hdb = new helpEntities();  

        public List<object> Data = new List<object>();

        private static string CalcularSha256Hex(byte[] data)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
            }
        }

        private static string ObtenerBase64Limpio(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                return base64;
            }

            var separadorDataUrl = base64.IndexOf(',');
            if (separadorDataUrl >= 0 && base64.Substring(0, separadorDataUrl).IndexOf("base64", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return base64.Substring(separadorDataUrl + 1);
            }

            return base64;
        }

        private static bool? CompararHashCliente(string hashCliente, string hashServidor)
        {
            if (string.IsNullOrWhiteSpace(hashCliente))
            {
                return null;
            }

            return string.Equals(hashCliente.Trim(), hashServidor, StringComparison.OrdinalIgnoreCase);
        }

        private static void AplicarMetadataFoto(help_Atenciones_Fotografias atencionFoto, FotoSiniestroRequest item, byte[] fotoData, string hashServidor)
        {
            atencionFoto.Latitud = item.Latitud;
            atencionFoto.Longitud = item.Longitud;
            atencionFoto.NombreFirmante = item.NombreFirmante;
            atencionFoto.FechaFirma = item.FechaFirma;
            atencionFoto.Decripcion = item.Descripcion;
            atencionFoto.OrigenId = item.OrigenId;
            atencionFoto.NombreOriginal = item.NombreOriginal;
            atencionFoto.HashSha256Cliente = item.HashSha256Cliente;
            atencionFoto.Extension = item.Extension;
            atencionFoto.TamanoBytes = item.TamanoBytes ?? fotoData.LongLength;
            atencionFoto.AnchoPixels = item.AnchoPixels;
            atencionFoto.AltoPixels = item.AltoPixels;
            atencionFoto.OrientacionExif = item.OrientacionExif;
            atencionFoto.TieneExif = item.TieneExif;
            atencionFoto.FechaCapturaOriginal = item.FechaCapturaOriginal;
            atencionFoto.OffsetCapturaOriginal = item.OffsetCapturaOriginal;
            atencionFoto.FuenteFechaCaptura = item.FuenteFechaCaptura;
            atencionFoto.FechaUltimaModificacion = item.FechaUltimaModificacion;
            atencionFoto.FechaSeleccionDispositivo = item.FechaSeleccionDispositivo;
            atencionFoto.FabricanteDispositivo = item.FabricanteDispositivo;
            atencionFoto.ModeloDispositivo = item.ModeloDispositivo;
            atencionFoto.SoftwareEdicion = item.SoftwareEdicion;
            atencionFoto.AltitudExifMetros = item.AltitudExifMetros;
            atencionFoto.PrecisionGpsExif = item.PrecisionGpsExif;
            atencionFoto.HashSha256Servidor = hashServidor;
            atencionFoto.HashCoincide = CompararHashCliente(item.HashSha256Cliente, hashServidor);
            atencionFoto.AlgoritmoHash = string.IsNullOrWhiteSpace(item.AlgoritmoHash) ? "SHA-256" : item.AlgoritmoHash;
            atencionFoto.MetadataJson = item.MetadataJson;
        }

        [HttpGet]
        [Route("ObtenerMisAtenciones")] // /api/Proveedor/ObtenerMisAtenciones/
        public IHttpActionResult ObtenerMisAtenciones(int IdProveedorAgente) {

            try
            {
                if (IdProveedorAgente == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();

                var misAtenciones = db.help_prv_F_Api_Listar_Mis_Atenciones(IdProveedorAgente).OrderBy(x => x.FechaInicio).ToList();

                if (misAtenciones.Count() > 0) {

                    misAtenciones.ForEach(item => {
                        Data.Add(new
                        {
                            IdAtencion = item.Id,
                            Servicio = item.LbTipoServicio,
                            Categoria = item.LbCategoriaServicio,
                            Proveedor = item.NombreProveedor,
                            Zona = item.Zona,
                            Direccion = item.Direccion,
                            DireccionDestino = item.DireccionDestino,
                            Convenio = item.LbConvenio,
                            Ciudad = item.Ciudad,
                            Colonia = item.Colonia,
                            Cliente = item.Cliente,
                            Poliza = item.Poliza,
                            Plan = item.Plan,
                            UsurioRegistro = item.UsuarioRegistroNombre,
                            Fecha = item.FechaInicio,
                            Estado = item.LbEstado,
                            ColorEstado = item.ColorEstado
                        });
                    });

                    return Ok(Data);
                }
                else {

                    return BadRequest("El codigo que ingreso no tiene un proveedor agente registrado en los expedientes.");
                }


            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        

        [HttpGet]
        [Route("ObtenerMisEventosMes")] // /api/Proveedor/ObtenerMisEventosMes/
        public IHttpActionResult ObtenerMisEventosMes(int mesId)
        {

            try
            {
                if (mesId == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();

                var misEventosMes = db.help_prv_ObtenerEventosMesActual(mesId).ToList();

                if (misEventosMes.Count() > 0)
                {

                    misEventosMes.ForEach(evento => {
                        Data.Add(new
                        {
                         eventId = evento.eventId,
                         eventName = evento.eventName,
                         eventDescription = evento.eventDescription,
                         eventMonthId = evento.eventMonthId,
                         eventMonthName = evento.eventMonthName,
                          eventUrlFolder = evento.eventUrlFolder,
                          eventImage = evento.eventImage,
                          eventSubtitle = evento.eventSubtitle,
                          eventContent = evento.eventContent,
                          eventLocation = evento.eventLocation,
                          eventUser = evento.eventUser,
                          eventContact = evento.eventContact,
                          eventPhone = evento.eventPhone,
                          eventWhatsapp = evento.eventWhatsapp,
                          eventWeb = evento.eventWeb,
                          eventPosition = evento.eventPosition,
                          eventEvento = evento.eventEvento,
                          eventDate = evento.eventDate,
                          eventTime = evento.eventTime,
                          eventTimeEnd = evento.eventTimeEnd
                        });
                    });

                    return Ok(Data);
                }
                else
                {

                    return BadRequest("El codigo que ingreso no tiene un proveedor agente registrado en los expedientes.");
                }


            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }


        [HttpGet]
        [Route("ObtenerLogsEstados")] // /api/Proveedor/ObtenerLogsEstados/
        public IHttpActionResult ObtenerLogsEstados(int IdAtencion)
        {
            try
            {
                if (IdAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("Mensaje", typeof(int));


                //if (cambio == 0)
                //{
                var misLogsEstado = db.help_prv_ObtenerLogsEstados(IdAtencion).ToList();

                if (misLogsEstado.Count() > 0)
                {
                    return Ok(misLogsEstado);
                }
                else
                {
                    return BadRequest("El codigo que ingreso no tiene logs de estado registrados.  ");
                }


            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }


        // ObtenerAgenteEstado
        [HttpGet]
        [Route("ObtenerAgenteEstado")] // /api/Proveedor/ObtenerAgenteEstado/
        public IHttpActionResult ObtenerAgenteEstado(int IdAgente)
        {
            try
            {
                var estado = db.help_prv_F_Obtener_AgenteEstado_Proveedor(IdAgente).FirstOrDefault(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();


                Data.Add(new
                {
                    Id = estado.Id,
                    RefProveedorId = estado.RefProveedorId,
                    NombreAgente = estado.NombreAgente,
                    MovilServicio = estado.MovilServicio,
                    Usuario = estado.Usuario,
                    EstadoId = estado.EstadoId
                });


                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        [HttpGet]
        [Route("ObtenerMisAtencionesActivas")] // /api/Proveedor/ObtenerMisAtencionesActivas/
        public IHttpActionResult ObtenerMisAtencionesActivas(int IdProveedorAgente)
        {

            try
            {
                if (IdProveedorAgente == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();

                var misAtenciones = db.help_prv_F_Api_Listar_Mis_Atenciones_Activas(IdProveedorAgente).OrderBy(x => x.Id).ToList();

                if (misAtenciones.Count() > 0)
                {

                    misAtenciones.ForEach(item => {
                        Data.Add(new
                        {
                            IdAtencion = item.Id,
                            Servicio = item.LbTipoServicio,
                            Categoria = item.LbCategoriaServicio,
                            Proveedor = item.NombreProveedor,
                            Zona = item.Zona,
                            Direccion = item.Direccion,
                            DireccionDestino = item.DireccionDestino,
                            Convenio = item.LbConvenio,
                            Ciudad = item.Ciudad,
                            Colonia = item.Colonia,
                            Cliente = item.Cliente,
                            Poliza = item.Poliza,
                            Plan = item.Plan,
                            UsurioRegistro = item.UsuarioRegistroNombre,
                            Fecha = item.FechaInicio,
                            Estado = item.LbEstado,
                            ColorEstado = item.ColorEstado,
                            LogEstado = item.LbEstado
                        });
                    });

                    return Ok(Data);
                }
                else
                {

                    return BadRequest("El codigo que ingreso no tiene un proveedor agente registrado en los expedientes.");
                }


            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }


        // /api/Proveedor/RecuperarPassword
        [HttpPost]
        [Route("RecuperarPassword")] // /api/Login/RecuperarPassword/
        public IHttpActionResult RecuperarPassword(ApiData.help_passwordDataRequest passwordData)
        {
            try
            {

                if (String.IsNullOrEmpty(passwordData.usuario))
                {
                    return BadRequest("El campo se encuentra vacio. por favor ingrese un campo");
                }
                else
                {
                    List<object> Data = new List<object>();
                    List<object> appversion = new List<object>();

                    ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                    ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                    db.help_prv_P_Api_Insert_Clave_Recuperar_Password(passwordData.usuario, Respuesta, Mensaje);

                    return Ok(Mensaje);
                }
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }


        }

        [HttpPost]
        [Route("Kikiriki")]
        public IHttpActionResult Kikiriki(ApiData.help_claveDataRequest claveData)
        {
            try
            {
                if (String.IsNullOrEmpty(claveData.clave))
                {

                    return BadRequest("El campo se encuentra vacio. por favor ingrese un campo");

                }
                else
                {
                    db.help_prv_F_ValidarClave(claveData.clave, claveData.user);
                    return Ok();
                }

                
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }
        }


            [HttpPost]
        [Route("ValidarClave")]
        public IHttpActionResult ValidarClave(ApiData.help_claveDataRequest claveData)
        {
            try
            {
                if (String.IsNullOrEmpty(claveData.clave))
                {

                    return BadRequest("El campo se encuentra vacio. por favor ingrese un campo");

                }
                else
                {

                    var validar = db.help_prv_F_ProveedorAgentes(claveData.clave, claveData.user).ToList();
                    if (validar.Count() > 0)
                    {
                        return Ok("Clave y usurio coinciden proceda autenticarse.");
                    }
                    else
                    {
                        return BadRequest("Clave no coinciden. Intente de nuevo");
                    }


                }
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        [HttpPost]
        [Route("GetAppVersionProveedores")] // /api/Login/RecuperarPassword/
        public IHttpActionResult GetAppVersionProveedores(string plataforma)
        {
            try
            {

                if (String.IsNullOrEmpty(plataforma))
                {
                    return BadRequest("El campo se encuentra vacio. por favor ingrese un campo");
                }
                else
                {
                    List<object> Data = new List<object>();
                    List<object> appversion = new List<object>();

                    ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                    ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                    var versionApp = db.help_prv_F_GetAppVersion_Proveedores(plataforma).ToList();

                    versionApp.ForEach(item =>
                    {
                        Mensaje.Value = item;
                    });


                    return Ok(Mensaje.Value.ToString());
                }
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }


        }


        [HttpPost]
        [Route("GetAppVersion")] // /api/Proveedor/GetAppVersion/
        public IHttpActionResult GetAppVersion(string plataforma)
        {
            try
            {

                if (String.IsNullOrEmpty(plataforma))
                {
                    return BadRequest("El campo se encuentra vacio. por favor ingrese un campo");
                }
                else
                {
                    List<object> Data = new List<object>();
                    List<object> appversion = new List<object>();

                    ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                    ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                    var versionApp = db.help_prv_F_GetAppVersion(plataforma).ToList();

                    versionApp.ForEach(item =>
                    {
                        Mensaje.Value = item;
                    });


                    return Ok(Mensaje.Value.ToString());
                }
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }


        }

        [HttpGet]
        [Route("ListadoAjustadores")] // /api/Proveedor/ListadoAjustadores/
        public IHttpActionResult ListadoAjustadores(int IdPais, int IdCiudad)
        {
            try
            {
                var listaAjustadores = db.help_prv_F_Api_ListadoProveedores(IdPais, IdCiudad).ToList();
                listaAjustadores.ForEach(item => {
                    Data.Add(new
                    {
                        AgenteNombre = item.NombreAgente,
                        IdProveedor = item.IdProveedor,
                        IdAgente = item.IdAgente,
                        Paid = item.Pais
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        [HttpGet]
        [Route("Usuarios")] // /api/Proveedor/Usuarios/
        public IHttpActionResult Usuarios()
        {
            try
            {
                var Usuarios = db.help_prv_ObtenerUsuarios().ToList();
                Usuarios.ForEach(usuario => {
                    Data.Add(new
                    {
                        userId = usuario.userId,
                        userName = usuario.userName, 
                        userPhoneNumber = usuario.userPhoneNumber, 
                        userEmail = usuario.userEmail, 
                        userPassword = usuario.userPassword, 
                        userTikTok = usuario.userTikTok, 
                        userPicture = usuario.userPicture, 
                        userUsuario = usuario.userUsuario, 
                        userLastName = usuario.userLastName
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        [HttpPost]
        [Route("Usuario")] // /api/Proveedor/Usuario/
        public IHttpActionResult Usuario(ApiData.help_usuarioDataRequest usuarioData)
        {
            try
            {
                var elUsuario = db.help_prv_ObtenerUsuario(usuarioData.usuario, usuarioData.pass, usuarioData.tipo).FirstOrDefault();
                
 
                
                    Data.Add(new
                    {
                        userId = elUsuario.userId,
                        userName = elUsuario.userName,
                        userPhoneNumber = elUsuario.userPhoneNumber,
                        userEmail = elUsuario.userEmail,
                        userPassword = elUsuario.userPassword,
                        userTikTok = elUsuario.userTikTok,
                        userPicture = elUsuario.userPicture,
                        userUsuario = elUsuario.userUsuario,
                        userLastName = elUsuario.userLastName,
                        fechaRegistro = elUsuario.fechaRegistro
                    });
               

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        [HttpPost]
        [Route("RegistrarUsuario")] // /api/Proveedor/RegistrarUsuario/
        public IHttpActionResult RegistrarUsuario(ApiData.help_UsuarioData usuarioData)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                db.help_prv_InsertUser(usuarioData.userName, usuarioData.userPhoneNumber, usuarioData.userEmail, usuarioData.userPassword,
                    usuarioData.userTikTok, usuarioData.userPicture, usuarioData.userUsuario, usuarioData.userLastName);

                return Ok("Usuario registrado con exito.");
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("InsertarLogConfirmacionEvento")] // /api/Proveedor/InsertarLogConfirmacionEvento/
        public IHttpActionResult InsertarLogConfirmacionEvento(ApiData.help_EventLogData eventoData)
        {
            try
            {
                if (eventoData == null ||
                    !eventoData.eventId.HasValue ||
                    !eventoData.refIdAtencion.HasValue ||
                    !eventoData.status.HasValue ||
                    !eventoData.userId.HasValue ||
                    eventoData.eventId.Value <= 0 ||
                    eventoData.refIdAtencion.Value <= 0 ||
                    eventoData.status.Value < 0 ||
                    eventoData.userId.Value <= 0)
                {
                    return BadRequest("Ingrese data valida para registrar la confirmacion del evento.");
                }

                db.help_prv_P_InsertarLogConfirmacionEvento(eventoData.eventId, eventoData.status, eventoData.userId, eventoData.refIdAtencion);

                var pushNotifier = new SeguimientoPushNotifier(hdb);
                string pushDetail;
                if (!pushNotifier.TryNotifyConfirmation(eventoData.refIdAtencion.Value, eventoData.status.Value, out pushDetail))
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "No se envio push de confirmacion para atencion {0}, evento {1}, estado {2}: {3}",
                        eventoData.refIdAtencion.Value,
                        eventoData.eventId.Value,
                        eventoData.status.Value,
                        pushDetail);
                }

                return Ok("Evento confirmado con exito.");
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("ObtenerLogConfirmacionEvento")] // /api/Proveedor/ObtenerLogConfirmacionEvento/
        public IHttpActionResult ObtenerLogConfirmacionEvento(int eventId, int refIdAtencion)
        {
            try
            {
                if (eventId <= 0 || refIdAtencion <= 0)
                {
                    return BadRequest("Ingrese un evento y una atencion valida.");
                }

                var confirmacion = db.Database.SqlQuery<ApiData.help_EventLogStatusResult>(
                    @"SELECT TOP 1
                            l.logId,
                            l.eventId,
                            l.refIdAtencion,
                            l.status,
                            l.userId,
                            l.logDate,
                            e.EstadoAtencion AS estadoAtencion,
                            e.Color AS color
                      FROM dbo.help_prv_LogConfirmacionEventos l WITH (NOLOCK)
                      LEFT JOIN help.dbo.help_RefEstadoAtencion e WITH (NOLOCK)
                        ON e.Id = l.status
                      WHERE l.eventId = @eventId
                        AND l.refIdAtencion = @refIdAtencion
                      ORDER BY l.logId DESC",
                    new System.Data.SqlClient.SqlParameter("@eventId", eventId),
                    new System.Data.SqlClient.SqlParameter("@refIdAtencion", refIdAtencion))
                    .FirstOrDefault();

                if (confirmacion == null)
                {
                    return NotFound();
                }

                return Ok(confirmacion);
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("InsertLogChatProveedor")] // /api/Proveedor/InsertLogChatProveedor/
        public IHttpActionResult InsertLogChatProveedor(ApiData.helpChatDataProveedor chatData)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                db.help_prv_P_Insert_LogChat_Proveedor(chatData.chatUserId, chatData.chatMessage, chatData.chatStamp, chatData.chatAtencionId, chatData.chatFecha, chatData.chatHora, chatData.chatRefTipoServicioId, chatData.chatStatus);
                
                return Ok("Chat Agregado con exito.");
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        // ListadoProveedores

        [HttpGet]
        [Route("ListadoProveedores")] // /api/Proveedor/ListadoAjustadores/
        public IHttpActionResult ListadoProveedores(int IdPais, int IdCiudad)
        {
            try
            {
                var listaAjustadores = db.help_prv_F_Api_ListadoProveedores(IdPais, IdCiudad).ToList();
                listaAjustadores.ForEach(item => {
                    Data.Add(new
                    {
                        AgenteNombre = item.NombreAgente,
                        IdProveedor = item.IdProveedor,
                        IdAgente = item.IdAgente,
                        Paid = item.Pais
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        [HttpGet]
        [Route("ObtenerDatosExpedientes")] // /api/Proveedor/ObtenerMisAtenciones/
        public IHttpActionResult ObtenerDatosExpedientes(int IdAtencion)
        {
            try
            {
                if (IdAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                    List<object> Data = new List<object>();
                    ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                    ObjectParameter Mensaje  = new ObjectParameter("Mensaje", typeof(int));

                //if (cambio == 0)
                //{
                    var misAtenciones = db.help_prv_P_Api_Datos_Expediente_Siniestro(IdAtencion).ToList();

                    if (misAtenciones.Count() > 0)
                    {
                        return Ok(misAtenciones);
                    }
                    else
                    {
                        return BadRequest("El codigo que ingreso no tiene un proveedor agente registrado en los expedientes.  ");
                    }

                //}
                //else {

                //    var misAtenciones = db.help_F_Api_Datos_Expediente_Siniestro_Menos_Datos(IdAtencion).ToList();

                //    if (misAtenciones.Count() > 0)
                //    {
                //        return Ok(misAtenciones);
                //    }
                //    else
                //    {
                //        return BadRequest("El codigo que ingreso no tiene un proveedor agente registrado en los expedientes.");
                //    }

                //}

                


            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }


        [HttpPost]
        [Route("ObtenerEstadoServicio")] // /api/Proveedor/ObtenerEstadoServicio/
        public IHttpActionResult ObtenerEstadoServicio(int IdAtencion)
        {
            try
            {
                if (IdAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();


                var misServicio = db.help_prv_F_Listado_Seguimiento_Atencion(IdAtencion).FirstOrDefault();  //hdb.help_F_Api_Obtener_Mi_Atencion(IdAtencion).FirstOrDefault();
                Data.Add(new
                {
                    idAtencion = IdAtencion,
                    EstadoServicio = misServicio.TipoSeguimiento,
                    fechaRegistro = misServicio.FechaHoraRegistro,
                    status = misServicio.Observaciones
                });
                return Ok(Data);

            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        [HttpGet]
        [Route("ObtenerDatosDeAtencion")] // /api/Proveedor/ObtenerDatosDeAtencion/
        public IHttpActionResult ObtenerDatosDeAtencion(int IdAtencion)
        {
            try
            {
                if (IdAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();

                //if (cambio == 0)
                //{
                var misAtenciones = db.help_prv_F_Lista_Mi_Atencion_FICOHSA_APP_Ajustadores(IdAtencion).ToList();

                if (misAtenciones.Count() > 0)
                {
                    return Ok(misAtenciones);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        [HttpPost]
        [Route("ActualizarCordenadasProveedor")] // /api/Proveedor/ActualizarCordenadasProveedor/
        public IHttpActionResult ActualizarCordenadasProveedor(double? Longitud,double? Latitud,int IdProveedorAgente)
        {

            try
            {
                if (Longitud == null || Latitud == null || IdProveedorAgente == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);


                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));


                db.help_prv_P_Actualizar_Coordenadas_Ajustador(Latitud,
                                                     Longitud,
                                                     IdProveedorAgente,
                                                     Respuesta,
                                                     Mensaje);




                return Ok("Coordenadas Ingresadas con exito.");
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("SubirFirmas")]
        public IHttpActionResult SubirFirmas(List<FotoSiniestroRequest> Fotos)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                if (Fotos == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (String.IsNullOrEmpty(Fotos.First().Foto) && Fotos.First().IdAtencion != 0)
                {
                    return BadRequest("Ingrese una imagen y un numero de atencion validos.");
                }
                else
                {
                    //help_Atenciones_Fotografias AtencionFoto = new help_Atenciones_Fotografias();
                    int RespuestasExitosas = 0;
                    int ConteoErrores = 0;
                    foreach (var item in Fotos)
                    {
                        byte[] FotoData = System.Convert.FromBase64String(item.Foto);
                        var nombreFoto = "Foto" + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + item.IdAtencion;


                        db.help_prv_P_Atenciones_Fotografias(item.IdAtencion, FotoData, 1, DateTime.Now, null, null, nombreFoto, "image/png",
                            item.RefTipoFotoId, item.NombreFirmante, item.FechaFirma, item.Descripcion, Respuesta, Mensaje); //help_Atenciones_Fotografias.Add(AtencionFoto);
                        //return Ok("Capturas guardadas con exito.");
                        if (Convert.ToInt32(Respuesta.Value) == 1)
                        {
                            RespuestasExitosas++;
                        }
                        else
                        {
                            ConteoErrores++;
                        }
                    }

                    if (RespuestasExitosas == Fotos.Count())
                    {
                        return Ok("Capturas guardadas con exito.");
                    }
                    else
                    {
                        return Ok(RespuestasExitosas + " Imágenes cargadas correctamente. " + ConteoErrores + " Imágenes no pudieron cargarse.");
                    }
                    

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("SubirFirmaProveedor")]
        public IHttpActionResult SubirFirmaProveedor(FotoFirmaAjustadorRequest Firma)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));
                if (Firma == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (String.IsNullOrEmpty(Firma.FotoFirma) && Firma.IdAgente != 0)
                {
                    return BadRequest("Ingrese una imagen y un codigo de agente valido.");
                }
                else
                {
                    byte[] FotoData = System.Convert.FromBase64String(Firma.FotoFirma);
                    /*
                    var proveedoragente = db.help_ProveedorAgentes.Find(Firma.IdAgente);
                    byte[] FotoData = System.Convert.FromBase64String(Firma.FotoFirma);
                    proveedoragente.FirmaAjustador = FotoData;
                    db.Entry(proveedoragente).State = EntityState.Modified;
                    db.SaveChanges();
                    */

                    db.help_prv_P_GuardarFirmaAgenteProveedor(Firma.IdAgente, FotoData
                        , Respuesta, Mensaje);

                    if (Convert.ToInt32(Respuesta.Value) == 1)
                    {
                        return Ok("Firma del ajustador guardada con exito.");
                    }
                    else
                    {
                        return Ok(Mensaje);
                    }
                    

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        [HttpPost]
        [Route("SubirFotografiaAjustador")]
        public IHttpActionResult SubirFotografiaAjustador(FotoAjustadorRequest Foto)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));
                if (Foto == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (String.IsNullOrEmpty(Foto.Fotografia) && Foto.IdAgente != 0)
                {
                    return BadRequest("Ingrese una fotografia y un codigo de agente valido.");
                }
                else
                {
                    db.help_prv_P_GuardarFotografiaProveedor(Foto.IdAgente, Foto.Fotografia, Respuesta, Mensaje);

                    if (Convert.ToInt32(Respuesta.Value) == 1)
                    {
                        return Ok("Firma del ajustador guardada con exito.");
                    }
                    else
                    {
                        return Ok(Mensaje);
                    }

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        [HttpPost]
        [Route("SubirVideo")]
        public IHttpActionResult SubirVideo(byte[] VideoData,int IdAtencion)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));
                if (VideoData == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (VideoData  != null && IdAtencion != 0)
                {
                    return BadRequest("Ingrese un video y un numero de atencion validos.");
                }
                else
                {
                    /*
                    help_Atenciones_Fotografias AtencionFoto = new help_Atenciones_Fotografias();

                        db.Database.CommandTimeout = 3000;

                        AtencionFoto.Fotografia = VideoData;
                        AtencionFoto.NombreFoto = "Video" + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + IdAtencion;
                        AtencionFoto.TipoFoto = ".MP4";
                        AtencionFoto.RefUsuarioId = 1;
                        AtencionFoto.FechaRegistro = DateTime.Now;
                        AtencionFoto.RefAtencionId = IdAtencion;
                        AtencionFoto.RefTipoId = 73;
                        db.help_Atenciones_Fotografias.Add(AtencionFoto);
                        db.SaveChanges();


                    

                    return Ok("Video guardado con exito.");
                    */
                    var nombreVideo = "Video" + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + IdAtencion;

                    db.help_prv_P_Atenciones_Video(IdAtencion, VideoData, 1, DateTime.Now, null, null, nombreVideo, ".MP4",
                            73, null, DateTime.Now, null, Respuesta, Mensaje); //help_Atenciones_Fotografias.Add(AtencionFoto);
                                                                                                                             //return Ok("Capturas guardadas con exito.");
                    if (Convert.ToInt32(Respuesta.Value) == 1)
                    {
                        return Ok("Capturas guardadas con exito.");
                    }
                    else
                    {
                        return Ok(Mensaje.Value.ToString());
                    }
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }
        

        // Comentado por Omar McClellan ya que no es parte del reuqrimiento del app
        /*
        [HttpPost]
        [Route("MostrarVideo")]
        public IHttpActionResult MostrarVideo(int? IdAtencion) {
          
            try
            {
                if (IdAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                var datavideo = db.help_Atenciones_Fotografias.Where(x => x.RefAtencionId == IdAtencion && x.RefTipoId == 73).ToList();

                if (datavideo.Count() > 0)
                {
                    return Ok(datavideo);
                }
                else
                {
                    return BadRequest("La atencion no tiene un video registrado, por favor grabe un video y subalo.");
                }

                
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }
        */

        /// <summary>
        /// Estado igual 8 = Tomado Por Proveedor, 9 = Cancelada Por Proveedor, 10 = Reasignada Por Proveedor.
        /// </summary>

       

        [HttpGet]
        [Route("ObtenerSeguimientoAtencion")] // /api/Proveedor/ObtenerSeguimientoAtencion/
        public IHttpActionResult ObtenerSeguimientoAtencion(int IdAtencion)
        {
            try
            {
                if (IdAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();

                var misSeguimientos = db.help_prv_F_Listado_Seguimiento_Atencion(IdAtencion).ToList();

                if (misSeguimientos.Count() > 0)
                {
                    return Ok(misSeguimientos);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        [HttpGet]
        [Route("ObtenerEstadoSeguimientoAtencion")] // /api/Proveedor/ObtenerEstadoSeguimientoAtencion/
        public IHttpActionResult ObtenerEstadoSeguimientoAtencion(int IdAtencion)
        {
            try
            {
                if (IdAtencion <= 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                var estadoSeguimiento = db.Database.SqlQuery<ApiData.help_EstadoSeguimientoAtencionResult>(
                    @"SELECT TOP 1
                            Id,
                            RefAtencionId,
                            FechaHoraRegistro,
                            Latitud,
                            Longitud,
                            TipoSeguimiento,
                            Observaciones,
                            ClienteId,
                            ProveedorId,
                            GestorId,
                            RefAtencionServicioId,
                            FechaFinalizacion,
                            RefEstadoSeguimiento,
                            KilometrajeRecorrido,
                            FechaFinalizadoSeg,
                            RefUsuarioId,
                            RefUsarioIdFinalizacion
                      FROM dbo.help_prv_F_Listado_Seguimiento_Atencion(@IdAtencion)",
                    new System.Data.SqlClient.SqlParameter("@IdAtencion", IdAtencion))
                    .FirstOrDefault();

                if (estadoSeguimiento == null)
                {
                    return NotFound();
                }

                return Ok(estadoSeguimiento);
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("TomarORCancelarAtencion")]
        public IHttpActionResult TomarORCancelarAtencion(int IdAtencion, int IdAtencionServicio, int Estado, string TiempoEspera, string RazonCancelacion)
        {
            try
            {

                //ESTADOS A RECIBIR = 4 ,7,6,9,11
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));
                ObjectParameter Respuesta2 = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje2 = new ObjectParameter("mensaje", typeof(string));
                ObjectParameter Respuesta3 = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje3 = new ObjectParameter("mensaje", typeof(string));
                var idatencionservicio = db.help_prv_F_Atenciones_Servicios(IdAtencion).ToList().Where(x => x.RefTipoServicioId == 32).FirstOrDefault();//= db.help_Atenciones_Servicios.Where(x => x.RefAtencionId == IdAtencion && x.RefTipoServicioId == 32).FirstOrDefault();

                if (idatencionservicio == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                /*  Estados DE ATENCION ENCABEZADO

                   1	NO ATENDIDA	yellow
                   2	ABIERTA	green
                   3	CANCELADA POR CLIENTE	red
                   4	CANCELADA POR PROVEEDOR	red
                   5	CANCELADO AL MOMENTO	red = Al 50% del pago de la distancia inicial a donde llego 
                   6	ASIGNADA/ABIERTA	green
                   7	CERRADO	red
                   8	ANULADA	orange
                   9	CANCELACION POSTERIOR	NULL
                   11   FINALIZADO POR PROVEEDOR red
                */

                TimeSpan tiempo = TimeSpan.Parse(TiempoEspera);

                db.help_prv_P_Cerrar_Proceso_Atencion(IdAtencion, DateTime.Now, Estado, Respuesta3, tiempo, Mensaje3);

                /*   Estado DE ATENCION (SERVICIOS) 

                    1   No Atendido
                    2   Asignado
                    3   Finalizado
                    4   Anulado
                    5   Cancelado al momento
                    6   Cancelado Posterior
                    7   Eliminado por usuario
                    8   Tomado Por Proveedor
                    9   Cancelada Por Proveedor
                    10  Reasignada Por Proveedor
                    11  Programado
                    12  ELIMINADO POR AUTORIZACIÓN PLAN
                    13 FINALIZADO POR PROVEEDOR APP
                 */

                int estadoatencionServicios = 0;

                if (Estado == 4)
                {
                    estadoatencionServicios = 9;
                }
                else if (Estado == 9)
                {
                    estadoatencionServicios = 6;
                }
                else if (Estado == 7)
                {
                    estadoatencionServicios = 3;
                }
                else if (Estado == 6)
                {
                    estadoatencionServicios = 2;
                }
                else if (Estado == 11)
                {
                    estadoatencionServicios = 13;
                }

                db.help_prv_P_Update_Status_Aten_Serv(idatencionservicio.Id, estadoatencionServicios, Respuesta, Mensaje); //help_SP_Update_Status_Aten_Serv(idatencionservicio.Id, Estado);

                int estadoProveedorAgente = 0;

                /* ESTADO DE PROVEEDORES AGENTES 
                    1   DISPONIBLE
                    2   DESCONECTADO
                    3   OCUPADO
                    4   ASIGNACION
                    5   FUERA DE SERVICIO
                    6   BAJA ADMINISTRATIVA
                */

                if (Estado == 6)
                {
                    estadoProveedorAgente = 3;
                }
                else if (Estado == 7)
                {

                    estadoProveedorAgente = 1;

                }
                else if (Estado == 4)
                { //Rechazado por el proveedor

                    estadoProveedorAgente = 1;
                }
                else if (Estado == 9)
                { //Cancelada posterior

                    estadoProveedorAgente = 1;
                }
                else if (Estado == 11)
                {
                    estadoProveedorAgente = 1;
                }

                this.db.help_prv_P_SetEstadoProveedor(idatencionservicio.RefProveedorId, estadoProveedorAgente);

                string mensaje = "";
                //NOTA
                if (Estado == 6)
                {
                    db.help_prv_P_Crear_Seguimiento_Manual(IdAtencion, IdAtencionServicio, null, null, 46, "SERVICIO TOMADO POR EL PROVEEDOR DE LA APLICACION DE PROVEEDORES", 1, 6);
                    mensaje = "Servicio Tomado";
                }
                else if (Estado == 4)
                {
                    db.help_prv_P_Crear_Seguimiento_Manual(IdAtencion, IdAtencionServicio, null, null, 46, "SERVICIO CANCELADO o RECHAZADO POR EL PROVEEDOR DESDE LA APP", 1, 6);
                    mensaje = "Servicio Cancelado";
                }
                else if (Estado == 7 || Estado == 11)
                {
                    db.help_prv_P_Crear_Seguimiento_Manual(IdAtencion, IdAtencionServicio, null, null, 46, "SERVICIO FINALIZADO POR EL PROVEEDOR", 1, 6);
                    mensaje = "Servicio Finalizado por la app de proveedores";
                }
                else if (Estado == 9)
                {
                    db.help_prv_P_Crear_Seguimiento_Manual(IdAtencion, IdAtencionServicio, null, null, 46, "SERVICIO CANCELADO POSTERIOR POR EL PROVEEDOR", 1, 6);
                    mensaje = "Servicio Cancelado Posterior";
                }

                return Ok(mensaje);
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        [HttpPost]
        [Route("ReAsignarExpediente")]
        public IHttpActionResult ReAsignarExpediente(int IdAtencion, int IdAgenteProveedor)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));
                db.help_prv_P_ReAsignarProveedor(IdAtencion, IdAgenteProveedor, null, null, Respuesta, Mensaje);

                var idatencionservicio = db.help_prv_F_Atenciones_Servicios(IdAtencion).ToList().Where(x => x.RefTipoServicioId == 32).FirstOrDefault();//= db.help_Atenciones_Servicios.Where(x => x.RefAtencionId == IdAtencion && x.RefTipoServicioId == 32).FirstOrDefault();
                db.help_prv_P_Update_Status_Aten_Serv(idatencionservicio.Id, 10, Respuesta, Mensaje); //help_SP_Update_Status_Aten_Serv(idatencionservicio.Id, Estado);
                this.db.help_prv_P_SetEstadoProveedor(idatencionservicio.RefProveedorId, 1);



                return Ok("Proveedor ReAsignado");
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("TokenPushOneSignal")]
        public IHttpActionResult TokenPushOneSignal(int IdProveedorAgente,string TokenPush)
        {
            try
            {
                if (IdProveedorAgente == 0 || TokenPush == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                //db.help_P_Api_Actualizar_Token_Agente_Proveedor(IdProveedorAgente,"",TokenPush,2);
                db.help_prv_P_Actualizar_Token_Agente_Proveedor(IdProveedorAgente, "", TokenPush, 2);

                return Ok("Token Actualizado con exito.");
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("ListaFotografiasFirmasAtencion")] // /api/Proveedor/ListaFotografiasFirmasAtencion/
        public IHttpActionResult ListaFotografiasFirmasAtencion(ApiData.ListaDatosFirma datosfirma) {
            try
            {

                if (datosfirma ==  null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                var listaAjustadores = db.help_prv_F_Atenciones_Fotografias(datosfirma.IdAtencion, datosfirma.TipoFotoFirma).ToList(); //help_Atenciones_Fotografias.Where(x => x.RefAtencionId == datosfirma.IdAtencion && x.RefTipoId == datosfirma.TipoFotoFirma).ToList();

                if (listaAjustadores.Count() > 0)
                {

                    listaAjustadores.ForEach(item =>
                    {
                        Data.Add(new
                        {
                            FotoFirma = item.Fotografia,
                            NombreFoto = item.NombreFoto,
                            Extension = item.TipoFoto
                        });
                    });


                    return Ok(Data);
                }
                else {

                    return BadRequest("Este numero de atencion no es valido o no tiene imagines o firmas para este expediente.");

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }
        }

        [HttpPost]
        [Route("IngresarSeguimientosManuales")]
        public IHttpActionResult IngresarSeguimientosManuales(ApiData.SegumientoData SegData)
        {
            try
            {

                //51  PROVEEDOR CONFIRMA CONTACTO(APP PROVEEDOR)
                //52  CONFIRMACION DE TIEMPO DE LLEGADA(APP PROVEEDOR)
                //53  PARTIDA DEL PROVEEDOR(APP PROVEEDOR)
                //54  FINALIZACIÓN DEL SERVICIO(APP PROVEEDOR)
                if (SegData == null)
                {
                    return BadRequest("Complete todos los campos");
                }
                else
                {

                    ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                    ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));
                    //var idServicio = hdb.help_Atenciones_Servicio.Where() .RefTipoServicioId;

                    hdb.help_P_SetSeguimiento_Manual_Mejorado(SegData.IdAtencion, null, SegData.Latitud, SegData.Longitud, SegData.TipoSeguimiento, "Ingresa seguimiento desde el app del proveedor help", 1, 2, 1, 6, Respuesta, Mensaje);

                    if (Convert.ToInt32(Respuesta.Value) == 1)
                    {
                        var pushNotifier = new SeguimientoPushNotifier(hdb);
                        string pushDetail;
                        if (!pushNotifier.TryNotify(SegData.IdAtencion, SegData.TipoSeguimiento, out pushDetail))
                        {
                            System.Diagnostics.Trace.TraceWarning(
                                "No se envio push para atencion {0}, seguimiento {1}: {2}",
                                SegData.IdAtencion,
                                SegData.TipoSeguimiento,
                                pushDetail);
                        }

                        return Ok(Mensaje.Value.ToString());
                    }
                    else
                    {
                        return BadRequest("ERROR: " + Mensaje.Value.ToString());
                    }

                }
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }


        }

        [HttpPost]
        [Route("ActualizarEstadoSeguimiento")] // /api/Proveedor/ActualizarEstadoSeguimiento/
        public IHttpActionResult ActualizarEstadoSeguimiento(int IdSeguimiento)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                db.help_prv_P_Actualiza_Seguimiento_Estado(IdSeguimiento,6,DateTime.Now,Respuesta,Mensaje);

                return Ok(" Seguimiento de Bitacora Actualizada con exito ");
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }
        /*

        [HttpPost]
        [Route("ActualizarCodigoBPMAjustador")] // /api/Proveedor/ActualizarCodigoBPMAjustador/
        public IHttpActionResult ActualizarCodigoBPMAjustador(int IdTablaAjustador,string CodigoBPMFicohsa,string CodigoReclamoFicohsa) {
            try
            {
                if (IdTablaAjustador == 0)
                    throw new Ht[HttpGet]
        [Route("ListaTipoAcuerdos")] // /api/Proveedor/ListaTipoAcuerdos/
        public IHttpActionResult ListaTipoAcuerdos()
        {
            try
            {

                var datosacuerdos = db.help_prv_F_Tipo_Acuerdo_Ficohsa().ToList();

                if (datosacuerdos.Count() > 0)
                {

                    datosacuerdos.ForEach(item =>
                    {
                        Data.Add(new
                        {
                            Id = item.Id,
                            Descripcion = item.DESCRIPCION,
                            Codigo = item.CODIGO
                        });
                    });


                    return Ok(Data);
                }
                else
                {

                    return BadRequest("Este numero de atencion no es valido o no tiene imagines o firmas para este expediente.");

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }
        }tpResponseException(HttpStatusCode.BadRequest);

                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                db.help_P_Actualizar_Ajustador_Audiencia(IdTablaAjustador, CodigoBPMFicohsa, CodigoReclamoFicohsa, Respuesta, Mensaje);

                if (Convert.ToInt32(Respuesta.Value) > 0)
                {
                    SendToMail Enviar = new SendToMail();
                    int IdAtencion = 0;
                    var Idt = db.help_AjustadorAudiencia.Find(IdTablaAjustador);
                    IdAtencion = Convert.ToInt32(Idt.RefAtencionId);
                    if (Enviar.Email_NotificacionAccidente(IdAtencion))
                    {
                        return Ok("Ajustador Audiencia Actualizado con exito.");
                    }
                    else
                    {
                        return BadRequest("Clave no se pudo enviar por correo. validar con soporte.");
                    }

                   
                }
                else {

                    return BadRequest(Mensaje.Value.ToString());

                }
                
            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("InsertarConvenioReparacionTaller")] // /api/Proveedor/InsertarConvenioReparacionTaller/
        public IHttpActionResult InsertarConvenioReparacionTaller(ApiData.ConvenioReparacionRequest ConvenioReparacion)
        {
            try
            {
                if (ConvenioReparacion == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                byte[] FotoData = System.Convert.FromBase64String(ConvenioReparacion.Fotografia);

                db.help_P_API_Insert_Convenio_Reparacion_Danio_Vehiculo_Cliente_Ajustador(ConvenioReparacion.CodigoDanioVehiculo, ConvenioReparacion.Descripcion, ConvenioReparacion.IdAtencion, FotoData, DateTime.Now, ConvenioReparacion.RefTipoFotoId, Respuesta, Mensaje);

                if (Convert.ToInt32(Respuesta.Value) > 0)
                {
                    return Ok("Danio de convenio de reparacion ingresado con exito.");
                }
                else
                {

                    return BadRequest(Mensaje.Value.ToString());

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        */


        [HttpPost]
        [Route("InsertarCoordenadasAtencionAgenteProveedor")] // /api/Proveedor/InsertarCoordenadasAtencionAgenteProveedor/
        public IHttpActionResult InsertarCoordenadasAtencionAgenteProveedor(ApiData.help_LogDatosGps LogGps)
        {
            try
            {
                if (LogGps == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                //string Tipo = "APP_ATENCION";

                db.help_prv_P_Log_Insert_GPS(LogGps.Longitud
                                            , LogGps.Latitud
                                            , LogGps.RefAtencionId
                                            , LogGps.FechaRegistro
                                            , LogGps.RefUsuarioId
                                            , LogGps.Tipo
                                            , LogGps.Contador
                                            , Respuesta
                                            , Mensaje);

                if (Convert.ToInt32(Respuesta.Value) == 1)
                {
                    return Ok("Coordenadas Ingresadas exitosamente.");
                }
                else
                {

                    return BadRequest(Mensaje.Value.ToString());

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("ObtenerRazonesAsignacion")] // /api/Proveedor/ObtenerRazonesAsignacion/
        public IHttpActionResult ObtenerRazonesAsignacion()
        {
            try
            {
                var listaAjustadores = db.help_prv_F_ObtenerRazonesAsignacion().ToList(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();
                listaAjustadores.ForEach(item => {
                    Data.Add(new
                    {
                        razon = item.razon,
                        razonId = item.razonId
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        [HttpPost]
        [Route("SubirFotosSiniestro")]
        public IHttpActionResult SubirFotosSiniestro(List<FotoSiniestroRequest> Fotos)
        {
            try
            {
                if (Fotos == null || !Fotos.Any())
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (String.IsNullOrEmpty(Fotos.First().Foto) || Fotos.First().IdAtencion == 0)
                {
                    return BadRequest("Ingrese una imagen y un numero de atencion validos.");
                }
                else
                {
                    foreach (var item in Fotos)
                    {
                        byte[] FotoData = System.Convert.FromBase64String(ObtenerBase64Limpio(item.Foto));
                        string hashServidor = CalcularSha256Hex(FotoData);
                        string nombreFoto = string.IsNullOrWhiteSpace(item.NombreFoto)
                            ? "Foto" + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + item.IdAtencion
                            : item.NombreFoto;

                        help_Atenciones_Fotografias AtencionFoto = new help_Atenciones_Fotografias();

                        AtencionFoto.Fotografia = FotoData;
                        AtencionFoto.NombreFoto = nombreFoto;
                        AtencionFoto.TipoFoto = string.IsNullOrWhiteSpace(item.TipoFoto) ? "image/png" : item.TipoFoto;
                        AtencionFoto.RefUsuarioId = 1;
                        AtencionFoto.FechaRegistro = DateTime.Now;
                        AtencionFoto.RefAtencionId = item.IdAtencion;
                        AtencionFoto.RefTipoId = item.RefTipoFotoId > 0 ? item.RefTipoFotoId : 77;
                        AplicarMetadataFoto(AtencionFoto, item, FotoData, hashServidor);
                        hdb.help_Atenciones_Fotografias.Add(AtencionFoto);
                        hdb.SaveChanges();
                    }

                    return Ok("Capturas guardadas con exito.");

                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("VectorGpsAtencion")] // /api/Proveedor/VectorGpsAtencion/
        public IHttpActionResult VectorGpsAtencion(int IdAtencion,string Tipo)
        {
            try
            {
                var listaAjustadores = db.help_prv_F_Log_GPS(IdAtencion, Tipo).ToList(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();
                listaAjustadores.ForEach(item => {
                    Data.Add(new
                    {
                        Latitud = item.Latitud,
                        Longitud = item.Longitud,
                        IdAtencion = item.RefAtencionId,
                        Tipo = item.Tipo,
                        FechaRegistro = item.FechaRegistro
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        [HttpGet]
        [Route("ObtenerLogAsignacionProveedorUnidadAgente")] // /api/Proveedor/VectorGpsAtencion/
        public IHttpActionResult ObtenerLogAsignacionProveedorUnidadAgente(int IdAtencion)
        {
            try
            {
                var log = db.help_prv_F_ObtenerLogAsignacionProveedorUnidadAgente(IdAtencion).FirstOrDefault(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();
                
                    Data.Add(new
                    {
                        RefAtencionId = log.RefAtencionId,
                        RefProveedorId = log.RefProveedorId,
                        RefIdServicio = log.RefIdServicio,
                        FechaSiniestro = log.FechaSiniestro,
                        FechaRegistro = log.FechaRegistro,
                        RefAgenteId = log.RefAgenteId,
                        RefEstadoId = log.RefEstadoId,
                        razonId = log.razonId,
                        Observaciones = log.Observaciones
                    });
                

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        [HttpGet]
        [Route("ObtenerUnidadesPorProveedor")] // /api/Proveedor/ObtenerUnidadesPorProveedor/
        public IHttpActionResult ObtenerUnidadesPorProveedor(int IdProveedor)
        {
            try
            {
                var unidades = db.help_prv_ObtenerUnidadesPorProveedor(IdProveedor).ToList(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();

                unidades.ForEach(unidad => {
                    Data.Add(new
                    {
                      unidadId = unidad.unidadId,
                      refProveedorAgenteIdPropietario = unidad.refProveedorAgenteIdPropietario,
                      unidadNombre = unidad.unidadNombre,
                      unidadFoto = unidad.unidadFoto,
                      unidadCodigo = unidad.unidadCodigo,
                      unidadPlaca = unidad.unidadPlaca,
                      unidadTipoId = unidad.unidadTipoId,
                      unidadSubTipoId = unidad.unidadSubTipoId,
                      unidadCiudadId = unidad.unidadCiudadId,
                      unidadRutaId = unidad.unidadRutaId,
                      unidadEstado = unidad.unidadEstado,
                      refProveedorId = unidad.refProveedorId
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        [HttpGet]
        [Route("ObtenerUnidadPorConductor")] // /api/Proveedor/ObtenerUnidadesPorProveedor/
        public IHttpActionResult ObtenerUnidadPorConductor(int IdAgente)
        {
            try
            {
                var unidades = db.help_prv_ObtenerUnidadPorConductor(IdAgente).ToList(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();

                unidades.ForEach(unidad => {
                    Data.Add(new
                    {
                        unidadId = unidad.unidadId,
                        refProveedorAgenteIdPropietario = unidad.refProveedorAgenteIdPropietario,
                        unidadNombre = unidad.unidadNombre,
                        unidadFoto = unidad.unidadFoto,
                        unidadCodigo = unidad.unidadCodigo,
                        unidadPlaca = unidad.unidadPlaca,
                        unidadTipoId = unidad.unidadTipoId,
                        unidadSubTipoId = unidad.unidadSubTipoId,
                        unidadCiudadId = unidad.unidadCiudadId,
                        unidadRutaId = unidad.unidadRutaId,
                        unidadEstado = unidad.unidadEstado,
                        refProveedorId = unidad.refProveedorId,
                        unidadTipo = unidad.unidadTipo,
                        unidadSubTipo = unidad.unidadSubTipo
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        [HttpGet]
        [Route("ListaEstadosAgenteProveedor")] // /api/Proveedor/ObtenerUnidadesPorProveedor/
        public IHttpActionResult ListaEstadosAgenteProveedor()
        {
            try
            {
                var razonesnidades = db.help_Prv_F_ListarReferenciaEstadoAgentesPRoveedor().ToList(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();

                razonesnidades.ForEach(razon => {
                    Data.Add(new
                    {
                        Id = razon.Id,
                        Estado = razon.Estado,
                        Icono = razon.Icono,
                        Color = razon.Color

                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        [HttpGet]
        [Route("ListaEstadosUnidad")] // /api/Proveedor/ListaEstadosUnidad/
        public IHttpActionResult ListaEstadosUnidad()
        {
            try
            {
                var razonesnidades = db.help_Prv_F_ListarReferenciaEstadoUnidadesPRoveedor().ToList(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();

                razonesnidades.ForEach(razon => {
                    Data.Add(new
                    {
                        Id = razon.estadoId,
                        Estado = razon.estado
                    });
                });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }

        //ObtenerFirmaAgenteProveedor  - help_prv_F_Api_ObtenerFirmaProveedor

        [HttpGet]
        [Route("ObtenerFirmaAgenteProveedor")] // /api/Proveedor/ObtenerFirmaAgenteProveedor/
        public IHttpActionResult ObtenerFirmaAgenteProveedor(int IdAgente)
        {
            try
            {
                var firma = db.help_prv_F_Api_ObtenerFirmaProveedor(IdAgente).FirstOrDefault(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();

                
                    Data.Add(new
                    {
                        NombreAgente = firma.NombreAgente,
                        FirmaAgente = firma.FirmaAgente
                    });
                

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        //GuardarFirmaAgenteProveedor  - help_prv_P_Api_GuardarFirmaProveedor
        [HttpPost]
        [Route("GuardarFirmaAgenteProveedor")]
        public IHttpActionResult GuardarFirmaAgenteProveedor(FotoFirmaAjustadorRequest Firma)
        {
            try
            {
                if (Firma == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (String.IsNullOrEmpty(Firma.FotoFirma) && Firma.IdAgente != 0)
                {
                    return BadRequest("Ingrese una imagen y un codigo de agente valido.");
                }
                else
                {
                    byte[] FotoData = System.Convert.FromBase64String(Firma.FotoFirma);
                    db.help_prv_P_Api_GuardarFirmaProveedor(Firma.IdAgente, FotoData);

                    return Ok("Firma del proveedor guardada con exito.");
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        // InsertarLogChatProveedor
        [HttpPost]
        [Route("InsertarLogChatProveedor")]
        public IHttpActionResult InsertarLogChatProveedor(EstadoAgenteData Estado)
        {
            try
            {
                if (Estado == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (Estado.agenteId == null || Estado.estadoId == null)
                {
                    return BadRequest("Ingrese una imagen y un codigo de agente valido.");
                }
                else
                {

                    db.help_prv_P_ActualizarEstadoProveedorAgente(Estado.proveedorId, Estado.agenteId, Estado.estadoId, Estado.activoId);

                    return Ok("Estado actualizado con exito.");
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        // ActualizarEstadoProveedorAgente
        [HttpPost]
        [Route("ActualizarEstadoProveedorAgente")]
        public IHttpActionResult ActualizarEstadoProveedorAgente(EstadoAgenteData Estado)
        {
            try
            {
                if (Estado == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (Estado.agenteId == null || Estado.estadoId == null)
                {
                    return BadRequest("Ingrese una imagen y un codigo de agente valido.");
                }
                else
                {
                    
                    db.help_prv_P_ActualizarEstadoProveedorAgente(Estado.proveedorId, Estado.agenteId, Estado.estadoId, Estado.activoId);

                    return Ok("Estado actualizado con exito.");
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        // ObtenerAgenteProveedor  help_prv_F_Obtener_Agente_Proveedor
        [HttpPost]
        [Route("ObtenerAgenteProveedor")] // /api/Proveedor/ObtenerAgenteProveedor/
        public IHttpActionResult ObtenerAgenteProveedor(int IdAgente)
        {
            try
            {
                var agente = db.help_prv_F_Obtener_Agente_Proveedor(IdAgente).FirstOrDefault(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();

                Data.Add(new
                {
                    Id = agente.Id,
                    RefProveedorId = agente.RefProveedorId,
                    NombreAgente = agente.NombreAgente,
                    MovilServicio = agente.MovilServicio,
                    Usuario = agente.Usuario,
                    Contrasena = agente.Contrasena,
                    Fotografia = agente.Fotografia,
                    LatitudActual = agente.LatitudActual,
                    LongitudActual = agente.LongitudActual,
                    EstadoId = agente.EstadoId,
                    RefUsuarioRegistro = agente.RefUsuarioRegistro,
                    FechaLocation = agente.FechaLocation,
                    TypoAgenteString = agente.TypoAgenteString,
                    Correo = agente.Correo,
                    Admin = agente.Admin,
                    RefFicohsaId = agente.RefFicohsaId,
                    Token = agente.Token,
                    TokenPush = agente.TokenPush,
                    ClaveRecuperarPassword = agente.ClaveRecuperarPassword,
                    Intentos = agente.Intentos,
                    TipoDeAsesor = agente.TipoDeAsesor,
                    UsuarioBPM = agente.UsuarioBPM,
                    FirmaAjustador = agente.FirmaAjustador
                });
               

            return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        [HttpPost]
        [Route("ObtenerNotificacionesAgenteProveedor")] // /api/Proveedor/ObtenerNotificacionesAgenteProveedor/
        public IHttpActionResult ObtenerNotificacionesAgenteProveedor(int IdAtencion)
        {
            try
            {
                var notificacion = db.help_prv_F_Obtener_Notificaciones_Proveedor(IdAtencion).FirstOrDefault(); //help_Log_GPS.Where(x => x.RefAtencionId == IdAtencion && x.Tipo == Tipo).ToList();
               
                    Data.Add(new
                    {
                        notificacionId = notificacion.notificacionId,
                        titulo = notificacion.titulo,
                        mensaje = notificacion.mensaje,
                        IdAtencion = notificacion.IdAtencion,
                        Servicio = notificacion.Servicio,
                        Direccion = notificacion.Direccion,
                        Ciudad = notificacion.Ciudad,
                        Colonia = notificacion.Colonia,
                        Cliente = notificacion.Cliente,
                        Plan = notificacion.Plan,
                        Fecha = notificacion.Fecha,
                        Estado = notificacion.Estado,
                        ColorEstado = notificacion.ColorEstado,
                        Hora = notificacion.Hora,
                        ProveedorAgenteId = notificacion.ProveedorAgenteId
                    });

                return Ok(Data);

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        // help_prv_F_Obtener_LogChats
        [HttpGet]
        [Route("ObtenerLogChats")] // /api/Proveedor/ObtenerLogChats/
        public IHttpActionResult ObtenerLogChats(int IdAgente)
        {
            try
            {
                if (IdAgente == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("Mensaje", typeof(int));
                var misChats = db.help_prv_F_Obtener_LogChats(IdAgente).ToList();

                if (misChats.Count() > 0)
                {
                    return Ok(misChats);
                }
                else
                {
                    return BadRequest("No existen chats para este agente.  ");
                }
            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }


        // help_prv_F_Obtener_LogChats_Atencion
        [HttpGet]
        [Route("ObtenerLogChatsAtencion")] // /api/Proveedor/ObtenerLogChatsAtencion/
        public IHttpActionResult ObtenerLogChatsAtencion(int IdAtencion)
        {
            try
            {
                if (IdAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                List<object> Data = new List<object>();
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("Mensaje", typeof(int));
                var misChats = db.help_prv_F_Obtener_LogChats_Atencion(IdAtencion).ToList();

                if (misChats.Count() > 0)
                {
                    return Ok(misChats);
                }
                else
                {
                    return BadRequest("No existen chats para este agente.  ");
                }
            }
            catch (Exception Ex)
            {


                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }

        }

        [HttpPost]
        [Route("InsertLogEstadoServicio")] // /api/Proveedor/InsertLogEstadoServicio/
        public IHttpActionResult InsertLogEstadoServicio(ApiData.help_EstadoServicioData eServicioData)
        {
            try
            {
                db.help_prv_InsertLog_EstadoServicio(eServicioData.atencionId, eServicioData.fechaRegistro, eServicioData.estadoId);
                return Ok("Evento registrado exitosamente");

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }

            }

        }


        // help_prv_P_ActualizaEstadoLogAtencion
        [HttpPost]
        [Route("ActualizaEstadoLogAtencion")]
        public IHttpActionResult ActualizaEstadoLogAtencion(EstadoAtencionData Estado)
        {
            try
            {
                if (Estado == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (Estado.idAtencion == null || Estado.estado == null)
                {
                    return BadRequest("Ingrese data válida.");
                }
                else
                {

                    db.help_prv_P_ActualizarEstadoLogAtencion(Estado.idAtencion, Estado.estado,"",null);

                    return Ok("Estado actualizado con exito.");
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        //
        [HttpPost]
        [Route("ActualizaEstadoNotificacion")]
        public IHttpActionResult ActualizaEstadoNotificacion(ApiData.help_NotificacionEstadoData notificacionData)
        {
            try
            {
                if (notificacionData.IdAtencion == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (notificacionData.IdAtencion == null || notificacionData.IdAtencion == null)
                {
                    return BadRequest("Ingrese data válida.");
                }
                else
                {

                    db.help_prv_P_ActualizarEstadoNotificacion(notificacionData.IdAtencion, notificacionData.estado);

                    return Ok("Estado actualizado con exito.");
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        [HttpPost]
        [Route("ActualizaLogAtencion")] // /api/Proveedor/ActualizaLogAtencion/
        public IHttpActionResult ActualizaLogAtencion(int IdAtencion)
        {

            try
            {
                if (IdAtencion == 0)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));
                Mensaje.Value = "Atencián Actualizada con éxito";
                db.help_prv_P_ActualizaEstadoLogAtencion(IdAtencion);

                return Ok(Mensaje.Value.ToString());

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("ObtenerEstadoAtencionNueva")]
        public IHttpActionResult ObtenerEstadoAtencionNueva(ApiData.EstadoAtencionNueva NuevaData)
        {
            try
            {
                if (NuevaData.idAtencion == 0)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (NuevaData.idAtencion == 0)
                {
                    return BadRequest("Ingrese data válida.");
                }
                else
                {

                    var miAtencion = db.help_P_ObtenerEstadoLogAtencionNueva(NuevaData.idAtencion).FirstOrDefault();
                    Data.Add(new
                    {
                        mensaje = "Estado de la nueva atención",
                        status = miAtencion
                    });
                    return Ok(Data);
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }

        // help_prv_P_ObtenerEstadoAtencion
        [HttpPost]
        [Route("ObtenerEstadoAtencion")]
        public IHttpActionResult ObtenerEstadoAtencion(EstadoAtencionAceptacionData Estado)
        {
            try
            {
                if (Estado == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (Estado.idAtencion == null)
                {
                    return BadRequest("Ingrese data válida.");
                }
                else
                {

                    var miAtencion = db.help_prv_P_ObtenerEstadoAtencion(Estado.idAtencion).FirstOrDefault();
                    Data.Add(new
                    {
                        idAtencion = miAtencion.idAtencion,
                        fechaRegistro = miAtencion.fechaRegistro,
                        status = miAtencion.status
                    });
                    return Ok(Data);
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }


        // ActualizarEstadoUnidadProveedorAgente  help_prv_P_ActualizarEstadoUnidadProveedorAgente
        [HttpPost]
        [Route("ActualizarEstadoUnidadProveedorAgente")]
        public IHttpActionResult ActualizarEstadoUnidadProveedorAgente(EstadoAgenteData Estado)
        {
            try
            {
                if (Estado == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (Estado.agenteId == null || Estado.estadoId == null)
                {
                    return BadRequest("Ingrese una imagen y un codigo de agente valido.");
                }
                else
                {

                    db.help_prv_P_ActualizarEstadoUnidadProveedorAgente(Estado.proveedorId, Estado.agenteId, Estado.estadoId);

                    return Ok("Estado actualizado con exito.");
                }

            }
            catch (Exception Ex)
            {

                if (Ex.InnerException != null)
                {
                    return BadRequest(Ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(Ex.Message);
                }
            }
        }
    }
}
