using System.IO;
using Microsoft.Identity.Client;

namespace PowerBiWebApp.Models {

  // this token cache should just be used for demos.
  public class CustomFileTokenCache {

    public static void EnableSerialization(ITokenCache tokenCache) {
      tokenCache.SetBeforeAccess(BeforeAccessNotification);
      tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    public static readonly string CacheFilePath = System.Web.HttpContext.Current.Server.MapPath("~/msalcache.json");

    private static readonly object FileLock = new object();

    private static void BeforeAccessNotification(TokenCacheNotificationArgs args) {
      lock (FileLock) {
        args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                ? File.ReadAllBytes(CacheFilePath) : null);
      }
    }

    private static void AfterAccessNotification(TokenCacheNotificationArgs args) {
      // if the access operation resulted in a cache update
      if (args.HasStateChanged) {
        lock (FileLock) {
          // reflect changesgs in the persistent store
          File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
        }
      }
    }

  }
}
