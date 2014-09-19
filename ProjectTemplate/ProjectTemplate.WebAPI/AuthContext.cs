using Microsoft.AspNet.Identity.EntityFramework;
using ProjectTemplate.WebAPI.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ProjectTemplate.WebAPI
{
    /// <summary>
    /// Database context class which is responsible for communicating with our database.
    /// IdentityDbContext - its like a special version of the traditional DbContext Class.
    ///                   - This will provide all of the EF code-first mapping and DBSet Properties needed to manage the identity tables in SQL server.
    ///                   - This created AspNetRoles,AspNetUserClaims,AspNetUserLogins,AspNetUserRoles,AspNetUsers and __MigrationHistory tables
    /// </summary>
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public AuthContext()
            : base("Development") //Connection string
        { 
        
        }

        //this will create our tables in the DB
        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}