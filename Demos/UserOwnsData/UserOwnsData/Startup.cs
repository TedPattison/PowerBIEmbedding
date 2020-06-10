using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Identity.Web.TokenCacheProviders.Session;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using Microsoft.Identity.Web.UI;
using UserOwnsData.Services;

namespace UserOwnsData {

  public class Startup {
  
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

  
    public void ConfigureServices(IServiceCollection services) {

      services.AddSignIn(Configuration);

      services
        .AddWebAppCallsProtectedWebApi(Configuration, PowerBiServiceApi.RequiredScopes)
        .AddInMemoryTokenCaches();

     services.AddScoped(typeof(PowerBiServiceApi));

      services.AddControllersWithViews(options => {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
      }).AddMicrosoftIdentityUI();

      services.AddRazorPages();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }
      else {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints => {
        endpoints.MapRazorPages();
        endpoints.MapControllers();
      });
    }
  }
}
