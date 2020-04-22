using System.Web.Mvc;
using System.Web.Routing;

using Owin;
using PowerBiWebApp.Models.Security;

namespace PowerBiWebApp {
  public class Startup {
    public void Configuration(IAppBuilder app) {

      // init ASP.NET MVC routes
      AreaRegistration.RegisterAllAreas();
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      
      // init OpenId Connect settings
      OwinOpenIdConnect.ConfigureAuth(app);
    }
  }
}