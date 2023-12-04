using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Sbt.Data;
using Sbt.Data.Repositories;
using Sbt.Services;

namespace Sbt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddScoped<IDivisionRepository, DivisionEfCoreRepository>();
            builder.Services.AddScoped<DivisionService>();
            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration.GetConnectionString("Cosmos_ConnectionString")
                ?? throw new InvalidOperationException("Connection string not found in configuration.");

            builder.Services.AddDbContext<DivisionContext>(options =>
            {
                // to stay within the free limits for Cosmos DB,
                // I am sharing the "database" and Container with the EF Project.
                options.UseCosmos(connectionString, databaseName: "Sbt-EF");
            });

            builder.Services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CustomViewLocationExpander());
            });

            builder.Services.AddScoped<SetCurrentOrganizationActionFilter>();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(SetCurrentOrganizationActionFilter));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<DivisionContext>();
                context.Database.EnsureCreated();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "AdminLoadSchedule",
                pattern: "Admin/LoadSchedule/{organization}",
                defaults: new { controller = "Admin", action = "LoadSchedule" });

            app.MapControllerRoute(
                name: "AdminDivisionsList",
                pattern: "Admin/Divisions/{organization}",
                defaults: new { controller = "Divisions", action = "Index" });

            app.MapControllerRoute(
                name: "AdminDivisionsAction",
                pattern: "Admin/Divisions/{action}/{organization}/{id?}",
                defaults: new { controller = "Divisions" });

            app.MapControllerRoute(
                name: "Admin",
                pattern: "Admin/{organization?}",
                defaults: new { controller = "Admin", action = "Index" });

            app.MapControllerRoute(
                name: "StandingsList",
                pattern: "{organization}",
                defaults: new { controller = "StandingsList", action = "Index" });

            app.MapControllerRoute(
                name: "Standings",
                pattern: "{organization}/{id}",
                defaults: new { controller = "Standings", action = "Index" });

            app.MapControllerRoute(
                name: "Scores",
                pattern: "{organization}/{id}/{gameID}",
                defaults: new { controller = "Scores", action = "Index" });

            app.MapControllerRoute(
                name: "custom",
                pattern: "{controller}/{action}/{organization?}/{id?}",
                defaults: new { controller = "Home", action = "Index" });

            app.Run();
        }
    }
}
