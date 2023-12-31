/*
Copyright � Upendo Ventures, LLC

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

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider
{
    using System;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Exceptions;
    using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components;

    public partial class Settings : AuthenticationSettingsBase
    {
        protected string AuthSystemApplicationName
        {
            get { return Const.AUTH_SYSTEM_TYPE; }
        }

        public override void UpdateSettings()
        {
            if(this.SettingsEditor.IsValid && this.SettingsEditor.IsDirty)
            {
                var config = (AuthConfigBase)SettingsEditor.DataSource;
                if (config.Enabled == true)
                {
                    var authSystems = AuthenticationController.GetAuthenticationServiceByType("Upendo Simple Auth");
                    authSystems.IsEnabled = true;
                    AuthenticationController.UpdateAuthentication(authSystems);
                }
                if (config.Enabled == false)
                {
                    var authSystems = AuthenticationController.GetAuthenticationServiceByType("Upendo Simple Auth");
                    authSystems.IsEnabled = false;
                    AuthenticationController.UpdateAuthentication(authSystems);
                }

                AuthConfigBase.UpdateConfig(config);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                AuthConfigBase config = AuthConfigBase.GetConfig(AuthSystemApplicationName, this.PortalId);
                this.SettingsEditor.DataSource = config;
                this.SettingsEditor.DataBind();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}