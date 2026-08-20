using Balut.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Balut.Web.Infrastructure
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "SuperAdmin", "Admin", "Secretary", "Teacher", "Parent", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            const string userName = "superadmin";
            if (await userManager.FindByNameAsync(userName) is null)
            {
                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = "superadmin@balut.ir",
                    FirstName = "مدیر",
                    LastName = "کل سیستم",
                    EmailConfirmed = true,
                    Status = 1
                };

                var result = await userManager.CreateAsync(user, "Super@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "SuperAdmin");
            }
        }
    }
}