﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoucmentManagmentSys.RoleManagment
{
    public interface IRoleManagment
    {
        UserManager<IdentityUser> userManager { get; set; }
        RoleManager<IdentityRole> roleManager { get; set; }




        public Task<bool> CreateRoles(IServiceProvider serviceProvider);

        public Task<bool> AssignRole(ClaimsPrincipal User, string role);

        public Task<bool> CheckRole(ClaimsIdentity User, string role);
    }
}
