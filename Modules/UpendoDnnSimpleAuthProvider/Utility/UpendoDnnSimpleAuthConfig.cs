using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Authentication;
using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Utility
{
    [Serializable]
    public class UpendoDnnSimpleAuthConfig : AuthenticationConfigBase
    {
        private const string CACHEKEY = "Authentication.Upendo Simple Auth";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpendoDnnSimpleAuthConfig));

        public bool Enabled { get; set; }

        public bool UseCaptcha { get; set; }

        protected UpendoDnnSimpleAuthConfig(int portalID)
            : base(portalID)
        {
            UseCaptcha = Null.NullBoolean;
            Enabled = true;
            try
            {
                string value = Null.NullString;
                if (ServiceLocator<IPortalController, PortalController>.Instance.GetPortalSettings(portalID).TryGetValue("Upendo Simple Auth_Enabled", out value))
                {
                    Enabled = bool.Parse(value);
                }

                value = Null.NullString;
                if (ServiceLocator<IPortalController, PortalController>.Instance.GetPortalSettings(portalID).TryGetValue("Upendo Simple Auth_UseCaptcha", out value))
                {
                    UseCaptcha = bool.Parse(value);
                }
            }
            catch (Exception message)
            {
                Logger.Error(message);
            }
        }

        public static void ClearConfig(int portalId)
        {
            string cacheKey = "Authentication.UpendoSimpleAuth_" + portalId;
            DataCache.RemoveCache(cacheKey);
        }

        public static UpendoDnnSimpleAuthConfig GetConfig(int portalId)
        {
            string cacheKey = "Authentication.UpendoSimpleAuth_" + portalId;
            UpendoDnnSimpleAuthConfig authenticationConfig = (UpendoDnnSimpleAuthConfig)DataCache.GetCache(cacheKey);
            if (authenticationConfig == null)
            {
                authenticationConfig = new UpendoDnnSimpleAuthConfig(portalId);
                DataCache.SetCache(cacheKey, authenticationConfig);
            }

            return authenticationConfig;
        }

        public static void UpdateConfig(UpendoDnnSimpleAuthConfig config)
        {
            PortalController.UpdatePortalSetting(config.PortalID, "Upendo Simple Auth_Enabled", config.Enabled.ToString());
            PortalController.UpdatePortalSetting(config.PortalID, "Upendo Simple Auth_UseCaptcha", config.UseCaptcha.ToString());
            ClearConfig(config.PortalID);
        }
    }
}