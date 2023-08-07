/*
Copyright © Upendo Ventures, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial 
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES 
OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;

    /// <summary>
    /// The Config class provides a central area for management of Module Configuration Settings.
    /// </summary>
    [Serializable]
    public class AuthConfigBase: AuthenticationConfigBase
    {
        private const string _cacheKey = "Authentication";

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthConfigBase"/> class.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="portalId"></param>
        protected AuthConfigBase(string service, int portalId)
            : base(portalId)
        {
            this.Service = service;

            if (this.HostConfig)
            {
                this.Enabled = HostController.Instance.GetBoolean(this.Service + "_Enabled", false);
            }
            else
            {
                this.Enabled = PortalController.GetPortalSettingAsBoolean(this.Service + "_Enabled", portalId, false);
            }
        }

        public bool Enabled { get; set; }

        public bool HostConfig { get; set; }

        protected string Service { get; set; }

        public static void ClearConfig(string service, int portalId)
        {
            DataCache.RemoveCache(GetCacheKey(service, portalId));
        }

        public static AuthConfigBase GetConfig(string service, int portalId)
        {
            string key = GetCacheKey(service, portalId);
            var config = (AuthConfigBase)DataCache.GetCache(key);
            if (config == null)
            {
                config = new AuthConfigBase(service, portalId);
                DataCache.SetCache(key, config);
            }

            return config;
        }

        public static void UpdateConfig(AuthConfigBase config)
        {
            if (config.HostConfig)
            {
                HostController.Instance.Update(config.Service + "_Enabled", config.Enabled.ToString(CultureInfo.InvariantCulture), true);
                PortalController.DeletePortalSetting(config.PortalID, config.Service + "_Enabled");
            }
            else
            {
                PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_Enabled", config.Enabled.ToString(CultureInfo.InvariantCulture));
            }

            ClearConfig(config.Service, config.PortalID);
        }

        private static string GetCacheKey(string service, int portalId)
        {
            return _cacheKey + "." + service + "_" + portalId;
        }
    }
}