using Microsoft.AspNetCore.Identity;
using ReadBooks.Models;

namespace ReadBooks.Models.Context
{
    public static class SeederUser
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //Creamos el rol si no existe en la base de datos
            string roleName = "admin";
            if(!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
            //Creamos el usuario si no existe
            string adminEmail = "adminreadbooks@gmail.com";
            var adminUser = await userManager.FindByNameAsync(adminEmail);
            if(adminUser == null)
            {
                var newUser = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                //CreateAsync toma la contraseña y la encripta
                var result = await userManager.CreateAsync(newUser, "Admin123*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, roleName);
                }
            }
        }
    }
}
