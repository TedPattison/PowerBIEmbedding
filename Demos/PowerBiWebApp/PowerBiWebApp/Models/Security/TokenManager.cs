using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace PowerBiWebApp.Models.Security {

  class TokenManager {

    private static string applicationId = ConfigurationManager.AppSettings["application-id"];
    private static string applicationSecret = ConfigurationManager.AppSettings["Application-secret"];
    private static string redirectUri = ConfigurationManager.AppSettings["redirect-uri"];

    private const string claimsIdentifierForTenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
    private readonly static string tenantID = ClaimsPrincipal.Current.FindFirst(claimsIdentifierForTenantId).Value;

    private const string urlPowerBiServiceApiRoot = "https://api.powerbi.com/";

    public static string GetAccessToken() {
      return GetAccessToken(PowerBiPermissionScopes.ReadUserWorkspaces);
    }

    public static string GetAccessToken(string[] scopes) {

      var userClaims = ClaimsPrincipal.Current.Identity as System.Security.Claims.ClaimsIdentity;

      // this is the tenant-specific authorization URL for the Azure AD v2 endpoint
      string tokenIssuerAuthority = ClaimsPrincipal.Current.FindFirst("iss").Value;

      // TenantId is the current organization's ID in Azure AD
      string tenantId = userClaims?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

      // objectidentifier is GUID-based identifier for Azure AD User Account of current user
      string currentUserId = userClaims?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

      // parse together Home Account ID for current user
      string homeAccountId = currentUserId + "." + tenantId;


      var appConfidential = ConfidentialClientApplicationBuilder.Create(applicationId)
                            .WithClientSecret(applicationSecret)
                            .WithRedirectUri(redirectUri)
                            .WithAuthority(tokenIssuerAuthority)
                            .Build();

      CustomFileTokenCache.EnableSerialization(appConfidential.UserTokenCache);

      var user = appConfidential.GetAccountAsync(homeAccountId).Result;

      AuthenticationResult authResult = null;
      try {
        authResult = appConfidential.AcquireTokenSilent(scopes, user).ExecuteAsync().Result;
      }
      catch {
        // upn is email afddress for current user
        string upn = userClaims?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

        string url = appConfidential.GetAuthorizationRequestUrl(scopes).ExecuteAsync().Result.AbsoluteUri;
        HttpContext.Current.Response.Redirect(url);

        
      }

      // return access token back to user
      return authResult.AccessToken;

    }

    public static PowerBIClient GetPowerBiClient() {
      return GetPowerBiClient(PowerBiPermissionScopes.ReadUserWorkspaces);
    }

    public static PowerBIClient GetPowerBiClient(string[] scopes) {
      var tokenCredentials = new TokenCredentials(GetAccessToken(scopes), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
    }

    public static void ClearUserCache() {

      var userClaims = ClaimsPrincipal.Current.Identity as System.Security.Claims.ClaimsIdentity;

      // this is the tenant-specific authorization URL for the Azure AD v2 endpoint
      string tokenIssuerAuthority = ClaimsPrincipal.Current.FindFirst("iss").Value;

      // TenantId is the current organization's ID in Azure AD
      string tenantId = userClaims?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

      // objectidentifier is GUID-based identifier for Azure AD User Account of current user
      string currentUserId = userClaims?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

      // parse together Home Account ID for current user
      string homeAccountId = currentUserId + "." + tenantId;


      var appConfidential = ConfidentialClientApplicationBuilder.Create(applicationId)
                            .WithClientSecret(applicationSecret)
                            .WithRedirectUri(redirectUri)
                            .WithAuthority(tokenIssuerAuthority)
                            .Build();

      CustomFileTokenCache.EnableSerialization(appConfidential.UserTokenCache);

      var user = appConfidential.GetAccountAsync(homeAccountId).Result;
      appConfidential.RemoveAsync(user);

    }

  }


}