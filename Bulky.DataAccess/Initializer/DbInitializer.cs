using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Initializer;

public class DbInitializer : IDbInitializer
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public DbInitializer(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_dbContext.Database.GetPendingMigrations().Any())
            {
                _dbContext.Database.Migrate();
            }

            if (!_roleManager.Roles.AsNoTracking().Any())
            {
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
            var admin = await _userManager.FindByEmailAsync("admin@bulky.com");
            if (admin == null)
            {
                await _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@bulky.com",
                    Email = "admin@bulky.com",
                    EmailConfirmed = true,
                    Name = "Bulky Admin",
                    PhoneNumber = "01068218987",
                    PhoneNumberConfirmed = true,
                    StreetAddress = "55 Ali st",
                    City = "El Mahalla El Kubra",
                    State = "Gharbia",
                    PostalCode = "31951",

                }, "Admin@12345");

                admin = await _userManager.FindByEmailAsync("admin@bulky.com");

                await _userManager.AddToRoleAsync(admin, SD.Role_Admin);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
