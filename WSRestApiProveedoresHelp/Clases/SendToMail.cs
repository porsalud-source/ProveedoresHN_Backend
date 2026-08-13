using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using WSRestApiProveedoresHelp.Models;

namespace WSRestApiProveedoresHelp.Clases
{
    public class SendToMail
    {

        private help_prvEntities db = new help_prvEntities();

        protected MailMessage Correo = new MailMessage();
        protected string StiloEmail { get { return "<style type='text/css'>*{margin:0;padding:0;font-family:'Helvetica Neue',Helvetica,Helvetica,Arial,sans-serif;box-sizing:border-box;font-size:14px}img{max-width:100%}body{-webkit-font-smoothing:antialiased;-webkit-text-size-adjust:none;width:100%!important;height:100%;line-height:1.6}table td{vertical-align:top}body{background-color:#f6f6f6}.body-wrap{background-color:#f6f6f6;width:100%}.container{display:block!important;max-width:600px!important;margin:0 auto!important;clear:both!important}.content{max-width:600px;margin:0 auto;display:block;padding:20px}.main{background:#fff;border:1px solid #e9e9e9;border-radius:3px}.content-wrap{padding:20px}.content-block{padding:0 0 20px}.header{width:100%;margin-bottom:20px}.footer{width:100%;clear:both;color:#999;padding:20px}.footer a{color:#999}.footer a,.footer p,.footer td,.footer unsubscribe{font-size:12px}h1,h2,h3{font-family:'Helvetica Neue',Helvetica,Arial,'Lucida Grande',sans-serif;color:#000;margin:40px 0 0;line-height:1.2;font-weight:400}h1{font-size:32px;font-weight:500}h2{font-size:24px}h3{font-size:18px}h4{font-size:14px;font-weight:600}ol,p,ul{margin-bottom:10px;font-weight:400}ol li,p li,ul li{margin-left:5px;list-style-position:inside}a{color:#1ab394;text-decoration:underline}.btn-primary{text-decoration:none;color:#FFF;background-color:#1ab394;border:solid #1ab394;border-width:5px 10px;line-height:2;font-weight:700;text-align:center;cursor:pointer;display:inline-block;border-radius:5px;text-transform:capitalize}.last{margin-bottom:0}.first{margin-top:0}.aligncenter{text-align:center}.alignright{text-align:right}.alignleft{text-align:left}.clear{clear:both}.alert{font-size:16px;color:#fff;font-weight:500;padding:20px;text-align:center;border-radius:3px 3px 0 0}.alert a{color:#fff;text-decoration:none;font-weight:500;font-size:16px}.alert.alert-warning{background:#f8ac59}.alert.alert-bad{background:#ed5565}.alert.alert-good{background:#1ab394}.invoice{margin:40px auto;text-align:left;width:80%}.invoice td{padding:5px 0}.invoice .invoice-items{width:100%}.invoice .invoice-items td{border-top:#eee 1px solid}.invoice .invoice-items .total td{border-top:2px solid #333;border-bottom:2px solid #333;font-weight:700}@media only screen and (max-width:640px){h1,h2,h3,h4{font-weight:600!important;margin:20px 0 5px!important}h1{font-size:22px!important}h2{font-size:18px!important}h3{font-size:16px!important}.container{width:100%!important}.content,.content-wrap{padding:10px!important}.invoice{width:100%!important}}</style>"; } }

        protected SmtpClient Cliente = new SmtpClient();

        protected string PublicDir { get; set; }

        protected int Modulo { get; set; }

        protected int Usuario { get; set; }

        public Exception Error { get; set; }

        public SendToMail() {

            InicializarHELP();
            //InicializarFICOHSATGU();
        }

        protected void InicializarHELP()
        {
            ConfigurarSmtp("SmtpHelp");
        }

        protected void InicializarFICOHSATGU()
        {
            ConfigurarSmtp("SmtpFicohsaTgu");
        }

        protected void InicializarFICOHSASPS()
        {
            ConfigurarSmtp("SmtpFicohsaSps");
        }

        private void ConfigurarSmtp(string prefijo)
        {
            var from = ObtenerConfiguracionRequerida(prefijo + "From");
            var user = ObtenerConfiguracionRequerida(prefijo + "User");
            var password = ObtenerConfiguracionRequerida(prefijo + "Password");
            var host = ObtenerConfiguracionRequerida(prefijo + "Host");
            var portText = ObtenerConfiguracionRequerida(prefijo + "Port");
            var enableSslText = ObtenerConfiguracionRequerida(prefijo + "EnableSsl");

            int port;
            bool enableSsl;
            if (!int.TryParse(portText, out port) || !bool.TryParse(enableSslText, out enableSsl))
            {
                throw new ConfigurationErrorsException("La configuracion SMTP de " + prefijo + " es invalida.");
            }

            Correo.SubjectEncoding = Encoding.UTF8;
            Correo.BodyEncoding = Encoding.UTF8;
            Correo.IsBodyHtml = true;
            Correo.From = new MailAddress(from);
            Cliente.Credentials = new NetworkCredential(user, password);
            Cliente.Port = port;
            Cliente.EnableSsl = enableSsl;
            Cliente.Host = host;
        }

        private static string ObtenerConfiguracionRequerida(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value) || value == "CONFIGURAR_EN_SERVIDOR")
            {
                throw new ConfigurationErrorsException("Falta configurar " + key + ".");
            }

            return value;
        }
        public bool RecuperarPassword(string email, string key)
        {
            this.Usuario = 1;

            Correo.To.Add(email);
            Correo.Subject = "HELP APP";
            Correo.Body = ("<html><head><meta http-equiv='Content-Type' content='text/html; charset=UTF-8' /> ");
            Correo.Body += (this.StiloEmail);
            Correo.Body += ("</head>");
            Correo.Body += ("<body>");
            Correo.Body += ("<table class='body-wrap'><tr><td></td><td class='container' width='600'><div class='content'><table class='main' width='100%' cellpadding='0' cellspacing='0'><tr>");
            Correo.Body += ("<td class='content-wrap'><table width='100%' cellpadding='0' cellspacing='0'>");
            Correo.Body += ("<tr><td><img class='img-responsive' src='http://portal.porsalud.net/Images/porsalud%20LOGO.png' /></td></tr>");
            Correo.Body += ("<tr><td class='content-block'><h3>HELP APP</h3></td></tr>");
            Correo.Body += ("<tr><td class='content-block'>Hola! <br /> Ingresa el siguiente código de activación. <br />Código de activación: <b>" + key + "</b></td></tr>");
            Correo.Body += ("<tr><td class='aligncenter'>Gracias por utilizar HELP APP</td></tr>");
            Correo.Body += ("<tr><td class='aligncenter'><br /></tr>");
            Correo.Body += ("<tr><td class='aligncenter'>Visite nuestro sitio web para conocer mas nuestros servicios <a href='https://www.porsalud.net'>www.porsalud.net</a></td></tr>");
            Correo.Body += ("<tr><td class='aligncenter'>O llame a nuestras lineas de atención al cliente <a>(+504) 2280.5454</a></td></tr>");
            Correo.Body += ("</table></td></tr></table>");
            Correo.Body += ("<div class=footer><table width='100%'><tr><td class='aligncenter content-block'>PORSALUD " + DateTime.Now.Year + "</td></tr></table></div>");
            Correo.Body += ("</div></td><td></td></tr></table></body></html>");
            return this.Send();

        }

        public bool Email_NotificacionAccidenteConAudiencia(int IdAtencion)
        {
            //Consultar datos y direcciones de envio a la DB
            try
            {
                //trear informacion base 
                var atencion = db.help_prv_F_Atenciones_Servicios(IdAtencion).ToList(); //db.help_Atenciones_Servicios.Where(x => x.RefAtencionId == IdAtencion).ToList();
                int? IdAtencionServicio = null;
                IdAtencionServicio = atencion.First().Id;
                var registro = db.help_prv_P_GetInformacion_Correo_NotificacionAccidenteConAudiencia(IdAtencionServicio).ToList().FirstOrDefault();
                var atencionlist = db.help_prv_F_Atenciones(registro.IdExpediente).FirstOrDefault(); //db.help_Atenciones.Where(x => x.Id == registro.IdExpediente).ToList().FirstOrDefault();
                if (registro != null)
                {
                    //Construir HTML de envio
                    this.Modulo = 1031;
                    this.Usuario = 1;
                    string correo = "";

                    if (registro.IdCiudad == 7)
                    {
                        correo = "gestionsiniestrosautostgu@ficohsa.com";
                    }
                    else
                    {
                        correo = "gestionsiniestrosautossps@ficohsa.com";
                    }

                    Correo.To.Add(correo);

                    Correo.Subject = "APP AJUSTADORES - Notificación Automática de Accidente - Asegurado: " + registro.Contratante;
                    //ANDJUNTANDO ARCHIVOS
                    //Correo.Attachments.Add(new System.Net.Mail.Attachment("/Reporte detallado de ventas.pdf"));
                    //END
                    Correo.Body = ("<html><head><meta http-equiv='Content-Type' content='text/html; charset=UTF-8' /> ");
                    Correo.Body += (this.StiloEmail);
                    Correo.Body += ("</head>");
                    Correo.Body += ("<body>");
                    Correo.Body += ("<table class='body-wrap'><tr><td></td><td class='container' width='600'><div class='content'><table class='main' width='100%' cellpadding='0' cellspacing='0'><tr>");
                    Correo.Body += ("<td class='content-wrap'><table width='100%' cellpadding='0' cellspacing='0'>");
                    Correo.Body += ("<tr><td><img class='img-responsive' src='http://" + this.PublicDir + "/Images/help_logo.png' /></td></tr>");
                    Correo.Body += ("<tr><td class='content-block'><h3>Notificación Automática de Accidente - Asegurado: " + registro.Contratante + "</h3></td></tr>");
                    Correo.Body += ("<tr><td class='content-block'><b>Buenos días!</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Señor " + registro.NombreDestinatario + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>" + registro.PuestoDestinatario + " de la ciudad de " + registro.LbCiudad + "</td></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'><br /></tr>");
                    Correo.Body += ("<tr><td class='content-block'>");
                    Correo.Body += ("Por este medio me permito informarle que el día " + Convert.ToDateTime(registro.FechaOcurrencia).ToString("dd/MM/yyyy") + ", se comunica el señor(a) " + registro.NombreConductor + ", asegurado");
                    Correo.Body += (registro.Contratante + " con número de póliza " + registro.Poliza + " ASISTENCIA, para reportar un accidente en el");
                    Correo.Body += ("departamento de " + registro.LbDepartamento + ", municipio de " + registro.LbCiudad + ", en la ciudad de " + registro.LbCiudad + ", en: " + registro.DireccionDetalle);
                    Correo.Body += ("en el vehículo marca " + registro.MarcaAutomovil + ", modelo " + ", año " + registro.AnioAutomovil + ", color " + registro.ColorAutomovil + ", placa " + registro.PlacaAutomovil + ", chasis " + registro.ChasisAutomovil + ".");
                    Correo.Body += ("</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>");
                    Correo.Body += ("El señor(a) " + registro.NombreConductor + ", comenta que: " + registro.DescripcioDetallada);
                    Correo.Body += ("</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>DINÁMICA: " + registro.DescripcionAudiencia + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Lugar Audiencia: " + registro.Lugar + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Fecha Ocurrencia: " + Convert.ToDateTime(registro.FechaOcurrencia).ToString("dd/MM/yyyy") + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Hora de Ocurencia: " + registro.HoraOcurrencia + "</td></tr>");
                    Correo.Body += ("</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Acciones: " + registro.DescripcionAcciones + "</td></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'><br /></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Expediente: " + registro.IdExpediente + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Asistencia: " + registro.IdAtencion + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Fecha Accidente: " + String.Format("{0:MM/dd/yyyy}", atencionlist.FechaDeAccidente) + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Hora Accidente: " + String.Format("{0:t}", atencionlist.HoraDeAccidente) + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>El teléfono de contacto del conductor del vehículo es: " + registro.TelefonoOrigen.ToString() + "</td></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'><br /></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'>Visite la gestion de la atencion como tambien sus formularios y la supervicion de la misma accediento en el link a continuacion <a href='"+this.PublicDir+ "/help_FicohsaSeguros/Edit/" + IdAtencion+ "'>portal.porsalud.net/help_FicohsaSeguros</a></td></tr>");
                    //Footer para cuando lancemos nuestra APP
                    //Correo.Body += ("<tr><td class='aligncenter'>Descargue nuestra aplicación móvil <b>HELP Asistencia</b></td></tr>");
                    //Correo.Body += ("<tr><td class='aligncenter'><a href=''><img class='img-responsive' src='https://" + this.PublicDir + "/Images/img_btn_play_strore.png' style='width: 20%;' /></a><a href=''><img class='img-responsive' src='https://" + this.PublicDir + "/Images/img_btn_apple_store.png' style='width: 18%; margin: 6px;' /></a></td></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'>O llame a nuestras lineas de atención al cliente <a href='tel:+504 2280 5454'>+(504) 2280 5454</a></td></tr>");
                    Correo.Body += ("</table></td></tr></table>");
                    Correo.Body += ("<div class=footer><table width='100%'><tr><td class='aligncenter content-block'>GRUPO FICOHSA - Centroamérica " + DateTime.Now.Year + "</td></tr></table></div>");
                    Correo.Body += ("</div></td><td></td></tr></table></body></html>");

                    return this.Send();
                }
                else
                {
                    this.Error = new Exception("No hay datos para enviar. " + IdAtencionServicio);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.Error = ex;
                return false;
            }
        }

        public bool Email_NotificacionAccidente(int IdAtencion)
        {
            //Consultar datos y direcciones de envio a la DB
            try
            {
                //trear informacion base 
                var atencion = db.help_prv_F_Atenciones_Servicios(IdAtencion).ToList(); //db.help_Atenciones_Servicios.Where(x => x.RefAtencionId == IdAtencion).ToList();
                int? IdAtencionServicio = null;
                IdAtencionServicio = atencion.First().Id;
                var registro = db.help_prv_P_GetInformacion_Correo_NotificacionAccidenteConAudiencia(IdAtencionServicio).ToList().FirstOrDefault();

                if (registro != null)
                {
                    //Construir HTML de envio
                    this.Modulo = 1031;
                    this.Usuario = 1;
                    string correo = "";

                    if (registro.IdCiudad == 7)
                    {
                        correo = "gestionsiniestrosautostgu@ficohsa.com";
                    }
                    else
                    {
                        correo = "gestionsiniestrosautossps@ficohsa.com";
                    }

                    Correo.To.Add(correo);
                   

                    Correo.Subject = "APP AJUSTADORES - Notificación Automática de Accidente - Asegurado: " + registro.Contratante;
                    //ANDJUNTANDO ARCHIVOS
                    //Correo.Attachments.Add(new System.Net.Mail.Attachment("/Reporte detallado de ventas.pdf"));
                    //END
                    Correo.Body = ("<html><head><meta http-equiv='Content-Type' content='text/html; charset=UTF-8' /> ");
                    Correo.Body += (this.StiloEmail);
                    Correo.Body += ("</head>");
                    Correo.Body += ("<body>");
                    Correo.Body += ("<table class='body-wrap'><tr><td></td><td class='container' width='600'><div class='content'><table class='main' width='100%' cellpadding='0' cellspacing='0'><tr>");
                    Correo.Body += ("<td class='content-wrap'><table width='100%' cellpadding='0' cellspacing='0'>");
                    Correo.Body += ("<tr><td><img class='img-responsive' src='http://" + this.PublicDir + "/Images/help_logo.png' /></td></tr>");
                    Correo.Body += ("<tr><td class='content-block'><h3>Notificación Automática de Accidente - Asegurado: " + registro.Contratante + "</h3></td></tr>");
                    Correo.Body += ("<tr><td class='content-block'><b>Buenos días!</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Señor " + registro.NombreDestinatario + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>" + registro.PuestoDestinatario + " de la ciudad de " + registro.LbCiudad + "</td></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'><br /></tr>");
                    Correo.Body += ("<tr><td class='content-block'>");
                    Correo.Body += ("Por este medio me permito informarle que el día " + Convert.ToDateTime(registro.FechaOcurrencia).ToString("dd/MM/yyyy") + ", se comunica el señor(a) " + registro.NombreConductor + ", asegurado");
                    Correo.Body += (registro.Contratante + " con número de póliza " + registro.Poliza + " ASISTENCIA, para reportar un accidente en el");
                    Correo.Body += ("departamento de " + registro.LbDepartamento + ", municipio de " + registro.LbCiudad + ", en la ciudad de " + registro.LbCiudad + ", en: " + registro.DireccionDetalle);
                    Correo.Body += ("en el vehículo marca " + registro.MarcaAutomovil + ", modelo " + ", año " + registro.AnioAutomovil + ", color " + registro.ColorAutomovil + ", placa " + registro.PlacaAutomovil + ", chasis " + registro.ChasisAutomovil + ".");
                    Correo.Body += ("</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>");
                    Correo.Body += ("El señor " + registro.NombreConductor + ", comenta que: " + registro.DescripcioDetallada);
                    Correo.Body += ("</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Acciones: " + registro.DescripcionAcciones + "</td></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'><br /></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Expediente: " + registro.IdExpediente + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Fecha Accidente: " + String.Format("{0:MM/dd/yyyy}", registro.FechaOcurrencia) + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Hora Accidente: " + String.Format("{0:t}", registro.HoraOcurrencia) + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>Asistencia: " + registro.IdAtencion + "</td></tr>");
                    Correo.Body += ("<tr><td class='content-block'>El teléfono de contacto del conductor del vehículo es: " + registro.TelefonoOrigen.ToString() + "</td></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'><br /></tr>");
                    Correo.Body += ("<tr><td class='aligncenter'>Visite la gestion de la atencion como tambien sus formularios y la supervicion de la misma accediento en el link a continuacion <a href='" + this.PublicDir + "/help_FicohsaSeguros/Edit/" + IdAtencion + "'>portal.porsalud.net/help_FicohsaSeguros</a></td></tr>");
                    //Footer para cuando lancemos nuestra APP
                    //Correo.Body += ("<tr><td class='aligncenter'>Descargue nuestra aplicación móvil <b>HELP Asistencia</b></td></tr>");
                    //Correo.Body += ("<tr><td class='aligncenter'><a href=''><img class='img-responsive' src='https://" + this.PublicDir + "/Images/img_btn_play_strore.png' style='width: 20%;' /></a><a href=''><img class='img-responsive' src='https://" + this.PublicDir + "/Images/img_btn_apple_store.png' style='width: 18%; margin: 6px;' /></a></td></tr>");                   
                    Correo.Body += ("<tr><td class='aligncenter'>O llame a nuestras lineas de atención al cliente <a href='tel:+502 2283-8046'>+(502) 2283-8046</a></td></tr>");
                    Correo.Body += ("<tr><td><img class='img-responsive' src='https://" + this.PublicDir + "/Images/HELPFicohsaGT3.jpg' /></td></tr>");
                    Correo.Body += ("</table></td></tr></table>");
                    Correo.Body += ("<div class=footer><table width='100%'><tr><td class='aligncenter content-block'>HELP - Centroamérica " + DateTime.Now.Year + "</td></tr></table></div>");

                    Correo.Body += ("</div></td><td></td></tr></table></body></html>");

                    return this.Send();
                }
                else
                {
                    this.Error = new Exception("No hay datos para enviar. " + IdAtencionServicio);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.Error = ex;
                return false;
            }
        }


        protected bool Send()
        {
            try
            {
                Cliente.Send(Correo);
                try
                {
                    string text = "";
                    foreach (MailAddress item in Correo.To.ToList())
                    {
                        text = text + item.Address + ";";
                    }

                    db.help_prv_P_Set_Email_Log(1, 1, Correo.Subject, text, Correo.Body, 0);
                }
                catch
                {
                }

                return true;
            }
            catch (SmtpException ex)
            {
                try
                {
                    Error = ex;
                    string text2 = "";
                    foreach (MailAddress item2 in Correo.To)
                    {
                        text2 = text2 + item2.Address + "; ";
                    }

                    if (ex.InnerException != null)
                    {
                        db.help_prv_P_Set_Email_Log(1, 1, Correo.Subject, text2, ex.Message + " " + ex.InnerException.Message, 0);
                    }
                    else
                    {
                        db.help_prv_P_Set_Email_Log(1, 1, Correo.Subject, text2, ex.Message, 0);
                    }
                }
                catch
                {
                }

                return false;
            }
        }
    }
}
