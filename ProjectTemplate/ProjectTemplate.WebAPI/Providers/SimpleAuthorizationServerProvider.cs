using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using ProjectTemplate.WebAPI.Entities;
using ProjectTemplate.WebAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace ProjectTemplate.WebAPI.Providers
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        /// <summary>
        /// used for validating client authentication.
        /// unfortunately we only have one client so we'll always return that its validated?
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            Client client = null;
            
            //we are supporting both Authorization header and form encoded to get the clientId and clientSecret
            if(!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                //this is to get the x-www-form-urlencoded
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            //we are checking if the consumer did not set any client information.
            if(context.ClientId == null)
            {
                context.Validated();
                //context.SetError("invalid_clientId","ClientId should be sent.");
                return Task.FromResult<object>(null);

            }

            /*after we receive the client id we need to check our database if the client is
            already registered with our back end API.*/
            using(AuthRepository _repo = new AuthRepository())
            {
                client = _repo.FindClient(context.ClientId);
            }

            /*if client_id is not registered then invalidate the context and reject the request*/
            if(client == null)
            {
                context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
                return Task.FromResult<object>(null);
            }

            //if the client is registered we need to check his application type.
            //if client is JavaScript - Non Confidential then we will not ask for the secret
            if(client.ApplicationType == Models.Enum.ApplicationTypes.NativeConfidential)
            {
                if(string.IsNullOrWhiteSpace(clientSecret))
                {
                    context.SetError("invalid_clientId","Client secret is invalid.");
                    return Task.FromResult<object>(null);
                }
                else
                {
                    //if Native - Confidential App then client secret will be validated against the secret stored in the DB
                    if(client.Secret != Helper.GetHash(clientSecret))
                    {
                        context.SetError("invalid_clientId","Client secret is invalid.");
                        return Task.FromResult<object>(null);
                    }
                }
            }

            //we will check if the client is active
            if(!client.Active)
            {
                context.SetError("invalid_clientId","Client is inactive.");
                return Task.FromResult<object>(null);
            }

            /*We will store the client allowed origin and refresh token life time on the Owin
             context so it will be available once we generate the refresh token and set expiry life time*/
            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime",client.RefreshTokenLifeTime.ToString());

            //if all is valid then we will mark the context as valid.
            context.Validated();
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// used to validate the username and password sent to the authorization server's token endpoint.
        /// we will use the AuthRepository.FindUser method to check the username and password in the DB.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

            if (allowedOrigin == null) allowedOrigin = "*";
 
            ///This will allow CORS(Cross-origin resource sharing) in our token middleware provider
            /// if we forget to add this in our Owin context, generating the token will fail when you try to call it from the browser.
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            using(AuthRepository _repo = new AuthRepository())
            {
                //check for the user identity
                IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect");
                    return;
                }
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName);
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role","user"));

            var props = new AuthenticationProperties(new Dictionary<string,string>
                {
                    {
                        "as:client_id",(context.ClientId == null)? string.Empty : context.ClientId
                    },
                    {
                        "userName",context.UserName
                    }
                
                });

            var ticket = new AuthenticationTicket(identity, props);

            ///token genation happens here
            context.Validated(ticket);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {

            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
    }
}