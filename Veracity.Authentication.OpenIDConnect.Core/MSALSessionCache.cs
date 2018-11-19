using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

namespace Veracity.Authentication.OpenIDConnect.Core
{
    public class MSALSessionCache
    {
        private static readonly ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly string _cacheId;
        readonly HttpContext _httpContext;

        public TokenCache Cache { get; } = new TokenCache();

        public MSALSessionCache( HttpContext httpcontext)
        {
            _cacheId = httpcontext.Session.Id + "_TokenCache";
            _httpContext = httpcontext;
            Load();
        }

        public TokenCache GetMsalCacheInstance()
        {
            Cache.SetBeforeAccess(BeforeAccessNotification);
            Cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return Cache;
        }

        public void SaveUserStateValue(string state)
        {
            SessionLock.EnterWriteLock();
            _httpContext.Session.SetString(_cacheId + "_state", state);
            SessionLock.ExitWriteLock();
        }

        public string ReadUserStateValue()
        {
            string state = string.Empty;
            SessionLock.EnterReadLock();
            state = (string) _httpContext.Session.GetString(_cacheId + "_state");
            SessionLock.ExitReadLock();
            return state;
        }

        public void Load()
        {
            SessionLock.EnterReadLock();
            Cache.Deserialize(_httpContext.Session.Get(_cacheId));
            SessionLock.ExitReadLock();
        }

        public void Persist()
        {
            SessionLock.EnterWriteLock();

            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            Cache.HasStateChanged = false;

            // Reflect changes in the persistent store
            _httpContext.Session.Set(_cacheId, Cache.Serialize());
            SessionLock.ExitWriteLock();
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (Cache.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
