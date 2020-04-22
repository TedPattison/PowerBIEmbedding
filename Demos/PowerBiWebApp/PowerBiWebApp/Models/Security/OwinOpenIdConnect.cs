using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using Owin;

using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Identity.Client;


namespace PowerBiWebApp.Models.Security {
  public class OwinOpenIdConnect {

    private const string tenantCommonAuthority = "https://login.microsoftonline.com/organizations/v2.0";

    private static string applicationId = ConfigurationManager.AppSettings["application-id"];
    private static string applicationSecret = ConfigurationManager.AppSettings["Application-secret"];
    private static string redirectUri = ConfigurationManager.AppSettings["redirect-uri"];

    public static void ConfigureAuth(IAppBuilder app) {

      app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

      app.UseCookieAuthentication(new CookieAuthenticationOptions());

      app.UseOpenIdConnectAuthentication(
          new OpenIdConnectAuthenticationOptions {
            ClientId = applicationId,
            Authority = tenantCommonAuthority,
            TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false },
            RedirectUri = redirectUri,
            Scope = "openid email profile " + String.Join(" ", PowerBiPermissionScopes.ReadWorkspaceAssets),
            PostLogoutRedirectUri = redirectUri,
            Notifications = new OpenIdConnectAuthenticationNotifications() {
              AuthorizationCodeReceived = onAuthorizationCodeCallback
            }
          });
    }

    private static async Task onAuthorizationCodeCallback(AuthorizationCodeReceivedNotification context) {


      ClaimsIdentity userClaims = context.AuthenticationTicket.Identity;
      string userName = userClaims.Name;
      string tenantID = userClaims.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

      // create URL for tenant-specific authority 
      string tenantSpecifcAuthority = "https://login.microsoftonline.com/" + tenantID + "/v2.0";


      var appConfidential = ConfidentialClientApplicationBuilder.Create(applicationId)
                         .WithClientSecret(applicationSecret)
                         .WithRedirectUri(redirectUri)
                         .WithAuthority(tenantSpecifcAuthority)
                         .Build();

      CustomFileTokenCache.EnableSerialization(appConfidential.UserTokenCache);

      string[] scopes = PowerBiPermissionScopes.ReadWorkspaceAssets;

      IAccount user = appConfidential.GetAccountAsync(userName).Result;

      var authResult = await appConfidential.AcquireTokenByAuthorizationCode(scopes, context.Code).ExecuteAsync();
    }


  }
}