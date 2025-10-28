using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Seeds;

public class RolesSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        if (_roleManager.Roles.AsNoTracking().Any()) return;

        var roles = new[] { SD.Role_Customer, SD.Role_Company, SD.Role_Admin, SD.Role_Employee };

        foreach (var role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role)) continue;

            var result = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
            }
        }
    }
}

