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
    /// <summary>
    /// Repository class to support ASP.Net Identity System
    /// </summary>
    public class AuthRepository :IDisposable
    {
        private AuthContext _ctx;

        /// <summary>
        /// provides logic for working with user information.
        /// responsible for hashing the password, how and when to calidate a user and manage claims.
        /// </summary>
        private UserManager<IdentityUser> _userManager;

        public AuthRepository()
        {
            _ctx = new AuthContext();
            _userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
        }

        /// <summary>
        /// method that will create a new user account.
        /// </summary>
        /// <param name="ModelUser"></param>
        /// <returns></returns>
        public async Task<IdentityResult> RegisterUser(ModelUser ModelUser)
        {
            //instantiate IdentityUser object
            //assign the new username
            IdentityUser user = new IdentityUser
            {
                UserName = ModelUser.UserName
            };

            //Create a new user
            //it will automatically hash the password
            var result = await _userManager.CreateAsync(user, ModelUser.Password);

            return result;
        }

        /// <summary>
        /// method to search a user account.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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