using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ProjectTemplate.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ProjectTemplate.WebAPI.Repository
{
    public class AuthRepository :IDisposable
    {
        private AuthContext _ctx;

        private UserManager<IdentityUser> _userManager;

        public async Task<IdentityResult> RegisterUser(ModelUser ModelUser)
        {
            IdentityUser user = new IdentityUser
            {
                UserName = ModelUser.UserName
            };

            var result = await _userManager.CreateAsync(user, ModelUser.Password);

            return result;
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await _userManager.FindAsync(userName,password);

            return user;
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();
        }
    }
}