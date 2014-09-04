using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using ProjectTemplate.WebAPI.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectTemplate.WebAPI
{
    /// <summary>
    /// This class will be fired once our server starts.
    /// in our assembly attribute we stated to fire Startup class
    /// </summary>
    [assembly: OwinStartup(typeof(ProjectTemplate.WebAPI.Startup))]
    public class Startup
    {
        /// <summary>
        /// Start up configuration for the application
        /// </summary>
        /// <param name="app">IAppBuilder will be supplied by the host during runtime</param>
        public void Configuration(IAppBuilder app) 
        {

            //used to configure API routes
            HttpConfiguration config = new HttpConfiguration();

            ConfigureOAuth(app);
           
            //register the config to the WebApiConfig.Register
            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            //Responsible to wire up ASP.Net Web API to our Owin server pipeline
            app.UseWebApi(config);
        }
        /// <summary>
        /// Configuration for the OAuth
        /// </summary>
        /// <param name="app"></param>
        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"), //The path for generating tokens will be as :”http://localhost:port/token”
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1), //set token expiry to 24 hours
                Provider = new SimpleAuthorizationServerProvider() //specified the implementation on how to validate the user credentials asking for tokens in custom class named SimpleAuthorizationServrProvider
                
            };

            //Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

    }
}