using FR_WS2_BaseLab.Data;
using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Implementations;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FR_WS2_BaseLab.Options;
using FR_WS2_BaseLab.Services.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Build.Framework;

namespace FR_WS2_BaseLab;

public class Program
{
    public static async Task  Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

       
        var connectionString = builder.Configuration.GetConnectionString("FR-WS2-BASELAB") 
        ?? throw new InvalidOperationException("Connection string 'FR-WS2-BASELAB' not found.");
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));  
        
        builder.Services.AddDbContext<FrWs2BaselabContext>(options =>
            options.UseSqlServer(connectionString));
        
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        //Identity avec .AddRoles
        builder.Services.AddDefaultIdentity<IdentityUser>(options => 
        options.SignIn.RequireConfirmedAccount = true)
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();
            
        builder.Services.AddControllersWithViews();

        // Services Métier 
        builder.Services.AddScoped<ITopicService, TopicService>();
        builder.Services.AddScoped<ICategoryImageService, CategoryImageService>();
       // builder.Services.AddScoped<ICategoryService, CategoryService>();
       // builder.Services.AddScoped<IPostService, PostService>();
        

        //Conf. Email
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
        builder.Services.AddTransient<IEmailSender, IdentityEmailSender>();
        builder.Services.AddTransient<IApplicationEmailSender, MailKitEmailSender>();

        var app = builder.Build();

        //seeding admin
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            if(!await roleManager.RoleExistsAsync("ADMINISTRATOR"))
                await roleManager.CreateAsync(new IdentityRole("ADMINISTRATOR"));
            
           var adminEmail = "admin@baselab.com";
           if (await userManager.FindByEmailAsync(adminEmail) == null)
           {
                var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                 await userManager.CreateAsync(admin, "Admin123!");
                 await userManager.AddToRoleAsync(admin, "ADMINISTRATOR");
           }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
            app.UseMigrationsEndPoint();
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}
