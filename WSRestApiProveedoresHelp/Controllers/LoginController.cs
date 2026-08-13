using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static WSRestApiProveedoresHelp.Controllers.ProveedorController;
using WSRestApiProveedoresHelp.Models;
using System.Data.Entity.Core.Objects;
using WSRestApiProveedoresHelp.Clases;
using System.Web.Http.Cors;
using System.Web;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WSRestApiProveedoresHelp.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [AllowAnonymous]
    [RoutePrefix("api/Login")]
    
    public class LoginController : ApiController
    {
        
        private help_prvEntities db = new help_prvEntities();

        public List<object> Data = new List<object>();

        [HttpPost]
        [Route("Autenticacion")] // /api/Login/Autenticacion/
        public IHttpActionResult Autenticacion(ApiData.LoginDataRequest loginData)
        {
            try
            {
                ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                if (loginData == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                if (String.IsNullOrEmpty(loginData.User) && String.IsNullOrEmpty(loginData.Password))
                {
                    return BadRequest("Correo o contraseña estan vacios. por favor ingrese un valor valido.");
                }
                else
                {
                    var VaildarCorreo = db.help_prv_P_Api_ValidaCorreo(loginData.User, Respuesta, Mensaje);
                    if (Convert.ToInt32(Respuesta.Value) == 1)
                    {

                        var PassEnc = db.help_prv_F_ObtenerPassword(loginData.User).ToList();
                        var user = PassEnc.First();
                        //var statusUser = dbm.help_F_Api_UserCheckStatus(user.userId);
                        if (user.Contrasena != null)
                        {
                            //SophiaSecurityCrypt NewCripta = new SophiaSecurityCrypt(SophiaSecurityCrypt.SophiaTypeCrypt.Simmetric);
                            //string key = SophiaBase64Encoder.Encoder(Password + user.userId);
                            //var Desencriptada = NewCripta.Desencriptar(user.password2, (new PasswordDeriveBytes(key, null)).GetBytes(32));

                            if (user.Contrasena == loginData.Password)
                            {
                                if (user.UsuarioBPM != null)
                                {
                                    var token = TokenGenerator.GenerateTokenJwt(loginData.User);

                                    db.help_prv_P_Actualizar_Token_Agente_Proveedor(user.Id, token.ToString(), null, 1);
                                    var nombreDeProveedor = db.help_prv_F_Proveedor(user.RefProveedorId).FirstOrDefault().NombreProveedor;
                                    //int ciudadId = db.help_prv_P_ObtenerCiudad_Proveedor(user.RefProveedorId).RefCiudadId;
                                    if (user.Admin == 1)
                                    {
                                        var misAgentes = db.help_prv_F_ObtenerAgentesProveedor(user.RefProveedorId).ToList();
                                        var misServicios = db.help_prv_F_ObtenerServiciosEsteProveedor(user.RefProveedorId);
                                        Data.Add(new
                                        {
                                            ProveedorAgenteId = user.Id,
                                            ProveedorName = nombreDeProveedor, //help_Proveedor.Find(user.RefProveedorId).NombreProveedor,
                                            TipoAgente = user.TypoAgenteString,
                                            Correo = user.Correo,
                                            ProveedorId = user.RefProveedorId,
                                            Usuario = user.Usuario,
                                            NombreAgente = user.NombreAgente,
                                            Token = token.ToString(),
                                            Foto = user.Fotografia,
                                            Lat = user.LatitudActual,
                                            Long = user.LongitudActual,
                                            Movil = user.MovilServicio,
                                            IdPais = user.RefIdPais,
                                            IdCiudad = user.RefCiudadId,
                                            Pais = user.Pais,
                                            Firma = user.FirmaAjustador,
                                            Admin = user.Admin,
                                            Agentes = misAgentes,
                                            Servicios = misServicios,
                                            Rtn = user.Rtn
                                            //UsuarioBPM = user.UsuarioBPM
                                            //status = status
                                        });
                                    }
                                    else
                                    {
                                        Data.Add(new
                                        {
                                            ProveedorAgenteId = user.Id,
                                            ProveedorName = nombreDeProveedor, //help_Proveedor.Find(user.RefProveedorId).NombreProveedor,
                                            TipoAgente = user.TypoAgenteString,
                                            Correo = user.Correo,
                                            ProveedorId = user.RefProveedorId,
                                            Usuario = user.Usuario,
                                            NombreAgente = user.NombreAgente,
                                            Token = token.ToString(),
                                            Foto = user.Fotografia,
                                            Lat = user.LatitudActual,
                                            Long = user.LongitudActual,
                                            Movil = user.MovilServicio,
                                            IdPais = user.RefIdPais,
                                            IdCiudad = user.RefCiudadId,
                                            Pais = user.Pais,
                                            Firma = user.FirmaAjustador,
                                            Admin = user.Admin
                                            //UsuarioBPM = user.UsuarioBPM
                                            //status = status
                                        });
                                    }
                                    

                                    return Ok(Data);
                                }
                                else {
                                    return BadRequest("El Ajustador no tiene codigo BPM asignado contacte a soporte.");
                                }
                                
                            }
                            else
                            {
                                return BadRequest("Contraseña invalida");
                            }
                        }
                        else
                        {

                            return BadRequest("Por favor verifica tu contraseña o valida con el administrador de HELP.");
                        }
                    }
                    else
                    {
                        return BadRequest("Email invalido");

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
        [Route("RecuperarPassword")] // /api/Login/RecuperarPassword/
        public IHttpActionResult RecuperarPassword(string user) {

            try
            {
                SendToMail Enviar = new SendToMail();

                if (String.IsNullOrEmpty(user))
                {
                    return BadRequest("El campo se encuentra vacio. por favor ingrese un campo");
                }
                else
                {

                    List<object> Data = new List<object>();
                    ObjectParameter Respuesta = new ObjectParameter("Respuesta", typeof(int));
                    ObjectParameter Mensaje = new ObjectParameter("mensaje", typeof(string));

                    db.help_prv_P_Insert_Clave_Recuperar_Password(user, Respuesta, Mensaje);
                    
                    if (Convert.ToInt32(Respuesta.Value) == 1) {

                        if (Enviar.RecuperarPassword(user, (Mensaje.Value.ToString())))
                        {
                            return Ok(Mensaje.Value.ToString());
                        }
                        else {
                            return BadRequest("Clave no se pudo enviar por correo. validar con soporte.");
                        }           
                    }
                    else {

                        return BadRequest(Mensaje.Value.ToString());
                    
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
        [Route("ValidarClaveNo")]
        public IHttpActionResult ValidarClaveNo(string clave,string user) {
            try
            {
                if (String.IsNullOrEmpty(clave))
                {

                    return BadRequest("El campo se encuentra vacio. por favor ingrese un campo");

                }
                else {

                    var validar = db.help_prv_F_ProveedorAgentes(clave, user).ToList(); //help_ProveedorAgentes.Where(x => x.ClaveRecuperarPassword == clave && x.Usuario == user).ToList();
                    if (validar.Count() > 0) {
                        return Ok("Clave y usurio coinciden proceda autenticarse.");
                    }
                    else {
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
        [Route("ActualizarPassword")]
        public IHttpActionResult ActualizarPassword(string user,string password) {
            try
            {
                if (String.IsNullOrEmpty(user) && String.IsNullOrEmpty(password))
                {
                    return BadRequest("Correo o contraseña estan vacios. por favor ingrese un valor valido.");
                }
                else { 
                
                    db.help_prv_P_Actualizar_Password_Proveedor_Agente(user,password);
                    return Ok("Contraseña actulizada con exito.");
                
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
        [Route("EnviarNotificacionAccidente")]
        public IHttpActionResult EnviarNotificacionAccidente(int IdAtencion)
        {
            try
            {
                if (IdAtencion == 0)
                {
                    return BadRequest("Correo o contraseña estan vacios. por favor ingrese un valor valido.");
                }
                else
                {

                    SendToMail Enviar = new SendToMail();

                    if (Enviar.Email_NotificacionAccidente(IdAtencion))
                    {
                        return Ok("Correo de notificacion enviado con exito.");
                    }
                    else
                    {
                        return BadRequest("Clave no se pudo enviar por correo. validar con soporte.");
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

                    var versionAppData = db.help_prv_F_GetAppVersion_Proveedores(plataforma).ToList();
                    var versionApp = versionAppData.First();

                    Data.Add(new
                    {
                        version = versionApp.versionCodigo,
                        descripcion = versionApp.descripcion
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


    }
}
