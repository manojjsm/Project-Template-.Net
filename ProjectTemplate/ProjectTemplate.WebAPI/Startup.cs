using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectTemplate.WebAPI
{
    [assembly: OwinStartup(typeof(ProjectTemplate.WebAPI.Startup))]
    public class Startup
    {
        public void Configuration(IAppBuilder app) 
        {
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}