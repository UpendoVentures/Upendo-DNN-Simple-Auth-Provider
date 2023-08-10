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

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider
{
    using System;
    using System.IO;
    using System.Threading;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// The Logoff UserModuleBase is used to log off a registered user.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Logoff : UserModuleBase
    {
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                // Get the Authentication System associated with the current User
                var authSystem = AuthenticationController.GetAuthenticationType();

                if (authSystem != null && !string.IsNullOrEmpty(authSystem.LogoffControlSrc))
                {
                    var authLogoffControl = (AuthenticationLogoffBase)this.LoadControl("~/" + authSystem.LogoffControlSrc);

                    // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
                    // this is necessary for the Localization in PageBase
                    authLogoffControl.AuthenticationType = authSystem.AuthenticationType;
                    authLogoffControl.ID = Path.GetFileNameWithoutExtension(authSystem.LogoffControlSrc) + "_" + authSystem.AuthenticationType;
                    authLogoffControl.LocalResourceFile = authLogoffControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                          Path.GetFileNameWithoutExtension(authSystem.LogoffControlSrc);
                    authLogoffControl.ModuleConfiguration = this.ModuleConfiguration;

                    authLogoffControl.LogOff += this.UserLogOff;
                    authLogoffControl.Redirect += this.UserRedirect;

                    // Add Login Control to Control
                    this.pnlLogoffContainer.Controls.Add(authLogoffControl);
                }
                else
                {
                    // The current auth system has no custom logoff control so LogOff
                    this.DoLogoff();
                    this.Redirect();
                }
            }
            catch (ThreadAbortException)
            {
                // Do nothing Response.redirect
            }
            catch (Exception exc) // Page failed to load
            {
                Exceptions.ProcessPageLoadException(exc);
            }
        }

        protected void UserLogOff(object sender, EventArgs e)
        {
            this.DoLogoff();
        }

        protected void UserRedirect(object sender, EventArgs e)
        {
            this.Redirect();
        }

        private void Redirect()
        {
            // Redirect browser back to portal
            this.Response.Redirect(AuthenticationController.GetLogoffRedirectURL(this.PortalSettings, this.Request), true);
        }

        private void DoLogoff()
        {
            try
            {
                // Remove user from cache
                if (this.User != null)
                {
                    DataCache.ClearUserCache(this.PortalSettings.PortalId, this.Context.User.Identity.Name);
                }

                var objPortalSecurity = PortalSecurity.Instance;
                objPortalSecurity.SignOut();
            }
            catch (Exception exc) // Page failed to load
            {
                Exceptions.ProcessPageLoadException(exc);
            }
        }
    }
}