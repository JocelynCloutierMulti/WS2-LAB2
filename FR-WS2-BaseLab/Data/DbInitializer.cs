using FR_WS2_BaseLab.Data;
using FR_WS2_BaseLab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FR_WS2_BaseLab.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<FrWs2BaselabContext>();
        await context.Database.EnsureCreatedAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "User");

        var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@test.com";
        var adminPassword = configuration["SeedAdmin:Password"] ?? "Admin1234?";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        
        Console.Write("\n\n");
        Console.Write(admin);
        Console.Write("regarde en haut#####################################################\n\n");
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Impossible de créer l'utilisateur admin : " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
        
        
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var userEmail = "etudiant@frws2.local";
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, "Passw0rd!");
            await userManager.AddToRoleAsync(user, "User");
        }

        if (!await context.Categories.AnyAsync())
        {
            var general = new Category
            {
                Name = "Général",
                Description = "Discussions générales du forum.",
                Inactive = false
            };

            var aspnet = new Category
            {
                Name = "ASP.NET Core",
                Description = "Questions et exemples liés à ASP.NET Core MVC.",
                Inactive = false
            };

            context.Categories.AddRange(general, aspnet);
            await context.SaveChangesAsync();

            var topic = new Topic
            {
                CatId = aspnet.Id,
                UserId = admin.Id,
                Inactive = false,
                Title = "Bienvenue dans BaseLab",
                Texte = "Ce sujet sert de point de départ pour tester les services, SMTP et les téléversements.",
                Date = DateTime.Today,
                Views = 0
            };

            context.Topics.Add(topic);
            await context.SaveChangesAsync();

            context.Posts.Add(new Post
            {
                TopId = topic.Id,
                UserId = user.Id,
                Inactive = false,
                Texte = "Premier message de test.",
                Date = DateTime.Today
            });

            await context.SaveChangesAsync();
        }
    }
    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }
    private static async Task EnsureCourse4TablesAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();
    }
}