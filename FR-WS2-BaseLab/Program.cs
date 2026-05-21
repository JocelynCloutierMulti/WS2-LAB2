using FR_WS2_BaseLab.Data;
using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Implementations;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using IdentityEmailSender = Microsoft.AspNetCore.Identity.IEmailSender<Microsoft.AspNetCore.Identity.IdentityUser>;

namespace FR_WS2_BaseLab;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("FR-WS2-BASELAB") ?? throw new InvalidOperationException("Connection string 'FR-WS2-BASELAB' not found.");
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));  
        
        builder.Services.AddDbContext<FrWs2BaselabContext>(options =>
            options.UseSqlServer(connectionString));
        
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();
        
        builder.Services.AddControllersWithViews();
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ITopicService, TopicService>();
        builder.Services.AddScoped<IPostService, PostService>();
        builder.Services.AddTransient<SmtpEmailSender>();
        builder.Services.AddTransient<IEmailSender>(sp => sp.GetRequiredService<SmtpEmailSender>());
        builder.Services.AddTransient<IdentityEmailSender>(sp => sp.GetRequiredService<SmtpEmailSender>());
        builder.Services.AddScoped<IForumEmailService, ForumEmailService>();

        var app = builder.Build();
        
        await DbInitializer.InitializeAsync(app.Services, app.Configuration);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
