using Microsoft.Owin.Security.Infrastructure;
using ProjectTemplate.WebAPI.Entities;
using ProjectTemplate.WebAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ProjectTemplate.WebAPI.Providers
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientid = context.Ticket.Properties.Dictionary["as:client_id"];

            if (string.IsNullOrEmpty(clientid))
            {
                return;
            }
            
            //we are generating a unique identifier for the refresh token
            var refreshTokenId = Guid.NewGuid().ToString("n");

            using (AuthRepository _repo = new AuthRepository())
            {
                //we are reading the token lifetime value from the OWIN context
                //this will determine how long will the refreshtoken will be valid
                var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");

                var token = new RefreshToken() { 
                    Id = Helper.GetHash(refreshTokenId),
                    ClientId = clientid,
                    Subject = context.Ticket.Identity.Name,
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime)) // we are setting the lifetime of the refresh token here
                };

                context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
                context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

                //responsible for serializing the contect and inserting it into the database
                token.ProtectedTicket = context.SerializeTicket();

                //Addrefreshtoken in the AuthRepo
                //This will add our refreshtoken in the DB
                var result = await _repo.AddRefreshToken(token);

                if (result)
                {
                    context.SetToken(refreshTokenId);
                }
                
            }
        }
    }
}