using System.Web.Http;
using WebActivatorEx;
using WSRestApiProveedoresHelp;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace WSRestApiProveedoresHelp
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration.EnableSwagger(c => 
            { 
                c.SingleApiVersion("v1", "API HELP APP PROVEEDORES").Contact(x => x.Name(" Ing. Manuel Palma ").Email("manuel.palma@porsalud.net"));
                c.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
            })
             .EnableSwaggerUi(c => { });
        }
    }

    public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                operation.parameters = new List<Parameter>();

            operation.parameters.Add(new Parameter
            {
                name = "Authorization",
                @in = "header",
                description = "Introduce Token JWT",
                required = false,
                type = "string"
            });
        }
    }
}
