using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LanBackup.WebApp.Data;
using LanBackup.DataCore;
using LanBackup.WebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LanBackup.WebApp.Services;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.Swagger.Model;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using LanBackup.WebApp.Hubs;
using LanBackup.WebApp.Middleware;
using System.Linq;
using AutoMapper;
using LanBackup.WebApp.Controllers;
using LanBackup.WebApp.Models.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;

namespace LanBackup.WebApp
{
  public class Startup
  {

    public const string AuthenticationSchemeName = "MyAuthScheme";
    private bool isApplicationInsightEnabled = false;

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddEnvironmentVariables();

      if (env.IsDevelopment())
      {
        // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
        builder.AddUserSecrets();
        builder.AddApplicationInsightsSettings(developerMode: true);
      }

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();

      this.isApplicationInsightEnabled = Configuration.GetSection("AppSettings").GetValue<bool>("InstrumentationEnabled");

    }

    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      //AppInsighttelemetry
      services.AddApplicationInsightsTelemetry(Configuration);
      TelemetryConfiguration.Active.DisableTelemetry = !Configuration.GetSection("AppSettings").GetValue<bool>("InstrumentationEnabled");

      //conf telemetry logger 
      services.AddSingleton<ITelemetryLogger, TelemetryLogger>();

      //client config
      ClientConfiguration.Instance = new ClientConfiguration(this.Configuration);
      services.AddSingleton<ClientConfiguration>(ClientConfiguration.Instance);

      //db contexts
      services.AddEntityFramework<ApplicationDbContext>(Configuration.GetConnectionString("DefaultConnection"));
      services.AddEntityFramework<BackupsContext>(Configuration.GetConnectionString("BackupsConnectionString"));

      //custom user key storage
      services.AddSingleton<IUserKeysRepository, UserKeysRepository>();

      services.AddIdentity<ApplicationUser, IdentityRole>(op =>
        {
          op.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

      // Add framework services.
      services.AddMvc();
      //Add Automapper
      services.AddAutoMapper(typeof(Startup));


      services.AddScoped<LanAgentsController>();
      services.AddSingleton<IUserIdProvider, HubUserIdProvider>();
      services.AddSignalR(options =>
      {
        options.Hubs.EnableJavaScriptProxies = true;
        options.Hubs.EnableDetailedErrors = true;
        options.EnableJSONP = true;
      });

      // Add application services.
      services.AddTransient<IEmailSender, AuthMessageSender>();
      services.AddTransient<ISmsSender, AuthMessageSender>();

      // Inject an implementation of ISwaggerProvider with defaulted settings applied
      services.AddSwaggerGen();
      services.ConfigureSwaggerGen(options =>
      {
        options.SingleApiVersion(new Info
        {
          Version = "v1",
          Title = "LanBackups API",
          Description = "ASP.NET Core Web API for LAN Backup service",
          TermsOfService = "None",
          Contact = new Contact { Name = "HouseOfSoftware Ltd", Email = "", Url = "http://www.asp.net" },
          License = new License { Name = "Free", Url = "http://url.com" }
        });
        //Determine base path for the application.
        var basePath = PlatformServices.Default.Application.ApplicationBasePath;
        //Set the comments path for the swagger json and ui.
        var xmlPath = Path.Combine(basePath, "LanBackup.WebApp.xml");
        options.IncludeXmlComments(xmlPath);
      });

      //Add the CORS services 
      services.AddCors();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {

      //configure loggers
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      loggerFactory.AddFile(Configuration["FileLogging:Path"]);

      // Add Application Insights monitoring to the request pipeline as a very first middleware.
      if(this.isApplicationInsightEnabled)
        app.UseApplicationInsightsRequestTelemetry();

      //conf CORS
      app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
      //// Shows UseCors with CorsPolicyBuilder.
      //app.UseCors(builder =>
      //   builder.WithOrigins("http://example.com"));



      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();

        app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
        {
          HotModuleReplacement = true
        });
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
      }
      // Add Application Insights exceptions handling to the request pipeline. - after error page and any other error handling middleware:
      if(this.isApplicationInsightEnabled)
        app.UseApplicationInsightsExceptionTelemetry();


      app.UseStaticFiles();


      app.UseIdentity();
      //app.UseCookieAuthentication(new CookieAuthenticationOptions()
      //{
      //  AuthenticationScheme = Startup.AuthenticationSchemeName,
      //  AutomaticAuthenticate = true
      //  //AutomaticChallenge = true,
      //  //SlidingExpiration = true,
      //  //ExpireTimeSpan = TimeSpan.FromMilliseconds(5000)
      //});
      //app.ApplyUserKeyValidation();//custom keys authentication




      app.UseWebSockets();
      app.UseSignalR("/signalr");


      app.UseMvcWithDefaultRoute();
      // Enable middleware to serve generated Swagger as a JSON endpoint
      app.UseSwagger();
      // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
      app.UseSwaggerUi();


      app.UseMvc(routes =>
      {
        routes.MapRoute(
                  name: "default",
                  template: "{controller=Home}/{action=Index}/{id?}");

        routes.MapSpaFallbackRoute(
                  name: "spa-fallback",
                  defaults: new { controller = "Home", action = "Index" });
      });



      RolesData.SeedDbWithData(app.ApplicationServices).Wait();

    }

  }




  public static class RolesData
  {
    private static readonly string[] Roles = new string[] {
      "Admin",
      //"Editor",
      //"Subscriber"
    };
    private static readonly string[] Users = new string[] {
      "admin@admin",
      "user0@user0",
    };
    private static readonly string[] Passwords = new string[] {
      "Admin$0",
      "User$0",
    };
    private static readonly string[] AdminUsers = new string[] {
      "admin@admin",
    };

    public static async Task SeedDbWithData(IServiceProvider serviceProvider)
    {
      using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
      {
        var dbApp = serviceScope.ServiceProvider.GetService<BackupsContext>();
        await dbApp.Database.EnsureCreatedAsync();

        var dbIdentity = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

        if (await dbIdentity.Database.EnsureCreatedAsync())
        {
          using (var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>())
          {
            foreach (var role in Roles)
            {
              if (!await roleManager.RoleExistsAsync(role))
              {
                await roleManager.CreateAsync(new IdentityRole(role));
              }
            }
          }

          using (var uManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>())
          {
            for (int i = 0; i < Users.Length; i++)
            {
              var userEmail = Users[i];
              var user = new ApplicationUser { UserName = userEmail, Email = userEmail };
              var resUserCreate = await uManager.CreateAsync(user, Passwords[i]);
              if (resUserCreate.Succeeded)
              {
                if (AdminUsers.ToList().Contains(userEmail))
                {
                  await uManager.AddToRolesAsync(user, Roles);
                }
              }
              else
              {
                throw new ArgumentException(resUserCreate.Errors.Select(s=> s.Description).ToArray().Aggregate((current, next) => current + ", " + next));
              }
            }
          }
        }
      }
    }
  }

}
