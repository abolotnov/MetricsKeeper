using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core.Data;

namespace Core.Context
{
    public class CoreAuthContext : IdentityDbContext<ApplicationUser>
    {
        public CoreAuthContext(DbContextOptions<CoreAuthContext> options) : base(options) 
        {
        }
    }
}
