using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;


namespace Core.Data
{
    public class ApplicationUser // : IdentityUser
    {
        public bool IsSystemAdmin { get; set; } = false;
    }

    public class OrgUserRegistrationData {
        public string Email { get; set; }
        public string Password { get; set; }
        public int OrgId { get; set; }
        public bool IsOrgAdmin { get; set; } = false;
        public bool AllowRead { get; set; } = true;
        public bool AllowWrite { get; set; } = false;
    }

    public class NewUserRegistrationData {
        public string Email { get; set; }
        public string Password { get; set; }
        public Org NewOrg { get; set; }
        public bool IsOrgAdmin = true;
        public bool AllowRead = true;
        public bool AllowWrite = true;
    }
}