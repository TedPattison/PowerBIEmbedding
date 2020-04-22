using System.IO;
using System.Web;
using System.Web.SessionState;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace PowerBiWebApp.Models {

  public class CustomSessionTokenCache {

    private static readonly ReaderWriterLockSlim sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);


    private static HttpSessionState session = HttpContext.Current.Session;
    private static string tokenCacheKey = "msal-token-cache";

    public static void EnableSerialization(ITokenCache tokenCache) {
      tokenCache.SetBeforeAccess(BeforeAccessNotification);
      tokenCache.SetAfterAccess(AfterAccessNotification);
    }

   
    private static void BeforeAccessNotification(TokenCacheNotificationArgs args) {
      sessionLock.EnterReadLock();
      try {
        // Load the cache from the session
        args.TokenCache.DeserializeMsalV3((byte[])session[tokenCacheKey]);
      }
      finally {
        sessionLock.ExitReadLock();
      }
    }

    private static void AfterAccessNotification(TokenCacheNotificationArgs args) {
      if (args.HasStateChanged) {
        sessionLock.EnterWriteLock();
        try {
          // Store the serialized cache in the session
          session[tokenCacheKey] = args.TokenCache.SerializeMsalV3();
        }
        finally {
          sessionLock.ExitWriteLock();
        }
      }
    }

  }
}