using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using PowerBiWebApp.Models;
using PowerBiWebApp.Models.Security;

namespace PowerBiWebApp.Controllers {

  public class AccountController : Controller {

    public void SignIn() {
      if (!Request.IsAuthenticated) {
        HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" },
            OpenIdConnectAuthenticationDefaults.AuthenticationType);
      }
    }

    public void SignOut() {

      // clear cache for current user in token cache
      TokenManager.ClearUserCache();

      // sign out and reidrect to home page
      string callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
      HttpContext.GetOwinContext().Authentication.SignOut(
          new AuthenticationProperties { RedirectUri = callbackUrl },
          OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);

      
    }

    [Authorize]
    public ActionResult UserProfile() {
      return View();
    }

  }
}