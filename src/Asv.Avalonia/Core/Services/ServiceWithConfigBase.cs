using Asv.Cfg;
using Asv.Common;

namespace Asv.Avalonia
{
    public class ServiceWithConfigBase<TConfig> : AsyncDisposableOnce
        where TConfig : new()
    {
        private readonly IConfiguration _cfgService;
        private readonly Lock _sync = new();
        private readonly TConfig _config;

        protected ServiceWithConfigBase(IConfiguration cfg)
        {
            _cfgService = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _config = cfg.Get<TConfig>();
        }

        protected TConfigValue InternalGetConfig<TConfigValue>(
            Func<TConfig, TConfigValue> getProperty
        )
        {
            using (_sync.EnterScope())
            {
                return getProperty(_config);
            }
        }

        protected void InternalSaveConfig(Action<TConfig> changeCallback)
        {
            using (_sync.EnterScope())
            {
                changeCallback(_config);
                _cfgService.Set(_config);
            }
        }
    }
}
