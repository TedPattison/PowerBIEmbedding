using System;
using System.Configuration;
using System.IO;
using System.Security;
using System.Reflection;
using Microsoft.Rest;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;

namespace PowerBIPublicClient.Models {

  class TokenManager {

    public const string urlPowerBiServiceApiRoot = "https://api.powerbi.com/";
    private const string tenantCommonAuthority = "https://login.microsoftonline.com/organizations";

    private static string applicationId = ConfigurationManager.AppSettings["application-id"];
    private static string redirectUri = ConfigurationManager.AppSettings["redirect-uri"];
    private static string userName = ConfigurationManager.AppSettings["aad-account-name"];
    private static string userPassword = ConfigurationManager.AppSettings["aad-account-password"];

    public static string GetAccessTokenInteractive(string[] scopes) {

      // create new public client application
      var appPublic = PublicClientApplicationBuilder.Create(applicationId)
                    .WithAuthority(tenantCommonAuthority)
                    .WithRedirectUri(redirectUri)
                    .Build();

      AuthenticationResult authResult = appPublic.AcquireTokenInteractive(scopes).ExecuteAsync().Result;

      // return access token to caller
      return authResult.AccessToken;
    }

    public static PowerBIClient GetPowerBiClientInteractive(string[] scopes) {

      string accessToken = TokenManager.GetAccessTokenInteractive(PowerBiPermissionScopes.ReadWorkspaceAssets);
      TokenCredentials tokenCredentials = new TokenCredentials(accessToken, "Bearer");
      PowerBIClient pbiClient = new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
      return pbiClient;
    }

    public static string GetAccessTokenWithUserPassword(string[] scopes) {

      // create new authentication context 
      var appPublic = PublicClientApplicationBuilder.Create(applicationId)
                        .WithAuthority(tenantCommonAuthority)
                        .Build();

      SecureString userPasswordSecure = new System.Security.SecureString();
      foreach (char c in userPassword) {
        userPasswordSecure.AppendChar(c);
      }

      AuthenticationResult authResult =
        appPublic.AcquireTokenByUsernamePassword(scopes,
                                                  userName,
                                                  userPasswordSecure).ExecuteAsync().Result;


      // return access token to caller
      return authResult.AccessToken;
    }

    public static PowerBIClient GetPowerBiClientWithUserPassword(string[] scopes) {
      var tokenCredentials = new TokenCredentials(GetAccessTokenWithUserPassword(scopes), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
    }

    public static string GetAccessToken(string[] scopes) {

      // create new authentication context 
      var appPublic = PublicClientApplicationBuilder.Create(applicationId)
                    .WithAuthority(tenantCommonAuthority)
                    .WithRedirectUri(redirectUri)
                    .Build();

      TokenCacheHelper.EnableSerialization(appPublic.UserTokenCache);

      AuthenticationResult authResult;
      try {
        // try to acquire token from token cache
        authResult = appPublic.AcquireTokenSilent(scopes, userName).ExecuteAsync().Result;
      }
      catch {
        try {
          // try to acquire token with non-interactive User Password Credential Flow
          SecureString userPasswordSecure = new System.Security.SecureString();
          foreach (char c in userPassword) {
            userPasswordSecure.AppendChar(c);
          }
          authResult = appPublic.AcquireTokenByUsernamePassword(scopes, userName, userPasswordSecure).ExecuteAsync().Result;
        }
        catch {
          // try to acquire token with interactive flow
          authResult = appPublic.AcquireTokenInteractive(scopes).ExecuteAsync().Result;
        }
      }

      // return access token to caller
      return authResult.AccessToken;
    }

    public static PowerBIClient GetPowerBiClient(string[] scopes) {
      var tokenCredentials = new TokenCredentials(GetAccessToken(scopes), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
    }

    static class TokenCacheHelper {

      private static readonly string CacheFilePath = Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
      private static readonly object FileLock = new object();

      public static void EnableSerialization(ITokenCache tokenCache) {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
      }

      private static void BeforeAccessNotification(TokenCacheNotificationArgs args) {
        lock (FileLock) {
          // repopulate token cache from persisted store
          args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath) ? File.ReadAllBytes(CacheFilePath) : null);
        }
      }

      private static void AfterAccessNotification(TokenCacheNotificationArgs args) {
        // if the access operation resulted in a cache update
        if (args.HasStateChanged) {
          lock (FileLock) {
            // write token cache changes to persistent store
            File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
          }
        }
      }
    }

  }

}
