using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WSRestApiProveedoresHelp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_BeginRequest()
        {
            if (Request.IsLocal && Request.AppRelativeCurrentExecutionFilePath == "~/")
            {
                Response.Redirect("~/swagger/ui/index", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}
