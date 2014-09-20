using Microsoft.AspNet.Identity.EntityFramework;
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
        /// ?unfortunately we only have one client so we'll always return that its validated?
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
        
            if(!context.TryGetBasicCredentials(out clientId, out clientSecret)
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if(context.ClientId == null)
            {
                context.Validated();
                //context.SetError("invalid_clientId","ClientId should be sent.");
                return Task.FromResult<object>(null);

            }

            using(AuthRepository _repo = new AuthRepository())
            {
                client = _repo.FindClient(context.ClientId);
            }

            if(client == null)
            {
                context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
                return Task.FromResult<object>(null);
            }

            if(client.ApplicationType == Models.Enum.ApplicationTypes.NativeConfidential)
            {
                if(string.IsNullOrWhiteSpace(clientSecret))
                {
                    context.SetError("invalid_clientId","Client secret is invalid.");
                    return Task.FromResult<object>(null);
                }
                else
                {
                    if(client.Secret != Helper.GetHash(clientSecret))
                    {
                        context.SetError("invalid_clientId","Client secret is invalid.");
                        return Task.FromResult<object>(null);
                    }
                }
            }

            if(!client.Active)
            {
                context.SetError("invalid_clientId","Client is inactive.");
                return Task.FromResult<object>(null);
            }

            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime",client.RefreshTokenLifeTime.ToString());

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
            ///This will allow CORS(Cross-origin resource sharing) in our token middleware provider
            /// if we forget to add this in our Owin context, generating the token will fail when you try to call it from the browser.
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

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
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role","user"));

            ///token genation happens here
            context.Validated(identity);
        }
    }
}