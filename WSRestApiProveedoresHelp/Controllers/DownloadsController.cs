using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;

namespace WSRestApiProveedoresHelp.Controllers
{
    [RoutePrefix("api/Proveedor")]
    public sealed class DownloadsController : ApiController
    {
        [AllowAnonymous]
        [AcceptVerbs("GET", "HEAD")]
        [Route("DescargarProveedoresApk")]
        public HttpResponseMessage DescargarProveedoresApk()
        {
            var configuredPath = ConfigurationManager.AppSettings["PROVEEDORES_APK_FILE_PATH"];
            var path = string.IsNullOrWhiteSpace(configuredPath)
                ? HostingEnvironment.MapPath("~/App_Data/Downloads/ProveedoresHN.apk")
                : Path.IsPathRooted(configuredPath) ? configuredPath : HostingEnvironment.MapPath(configuredPath);
            FileStream stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete,
                    64 * 1024, FileOptions.SequentialScan);
                if (stream.Length == 0)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "La APK de Proveedores no esta disponible.");

                var length = stream.Length;
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                if (Request.Method == HttpMethod.Head)
                    response.Content = new ByteArrayContent(new byte[0]);
                else
                {
                    response.Content = new StreamContent(stream);
                    stream = null; // The response owns and disposes the file stream.
                }
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");
                response.Content.Headers.ContentLength = length;
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "ProveedoresHN.apk" };
                response.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true, MustRevalidate = true };
                return response;
            }
            catch (FileNotFoundException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "La APK de Proveedores no esta disponible.");
            }
            catch (DirectoryNotFoundException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "La APK de Proveedores no esta disponible.");
            }
            catch (UnauthorizedAccessException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "La descarga no esta disponible temporalmente.");
            }
            catch (IOException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "La descarga no esta disponible temporalmente.");
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
        }
    }
}
