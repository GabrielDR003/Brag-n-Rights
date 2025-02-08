using Microsoft.EntityFrameworkCore;
using GymBro_App.Models;
using System.Net.Http.Headers;
using GymBro_App.Services;

namespace GymBro_App;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddSwaggerGen();

        // This works with user secrets. 
        // When storing the connection string in appsettings, instead use builder.GetConnectionString("GymBroConnection");
        var connectionString = builder.Configuration["GymBroConnection"];

        builder.Services.AddDbContext<GymBroDbContext>(options => options
            .UseLazyLoadingProxies()    // Will use lazy loading, but not in LINQPad as it doesn't run Program.cs
            .UseSqlServer(connectionString));

        string foodApiUrl = "https://platform.fatsecret.com/rest/server.api";
        string foodApiKey = builder.Configuration["FoodApiKey"] ?? "";

        builder.Services.AddHttpClient<IFoodService, FoodService>((client, services) =>
        {
            client.BaseAddress = new Uri(foodApiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", foodApiKey);
            return new FoodService(client, services.GetRequiredService<ILogger<FoodService>>());
        });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
