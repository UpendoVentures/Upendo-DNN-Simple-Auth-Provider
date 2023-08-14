#region License

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

#endregion

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using System.IO;
using System.Web.Hosting;
using DotNetNuke.Common;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components
{
    public class Email
    {
        private string _logo; 
        private string _body;
        private string _verificationCode;
        private string _fromEmail;
        private string _toEmail;

        public void Send(string userEmail, string code, string subject)
        {
            Requires.NotNullOrEmpty("userEmail", userEmail);
            Requires.NotNullOrEmpty("code", code);
            Requires.NotNullOrEmpty("subject", subject);

            this._verificationCode = code;
            this._logo = UtilityMethods.GetSiteIconUrl();
            subject = ReplaceTokens(subject);
            this._fromEmail = DnnGlobal.Instance.GetPortalEmail();
            this._toEmail = userEmail;

            string valueSettings = PortalController.GetPortalSetting("UpendoSimpleDnnAuth.ConfirmEmail", DnnGlobal.Instance.GetPortalId(), string.Empty);

            string serverPath = HostingEnvironment.MapPath(valueSettings);
            var templateContent = File.ReadAllText(serverPath);
            string emailBody = templateContent;

            emailBody = ReplaceTokens(emailBody);

            DotNetNuke.Services.Mail.Mail.SendEmail(this._fromEmail, this._toEmail, subject, emailBody);
        }

        private string ReplaceTokens(string value)
        {
            var portalSettings = DnnGlobal.Instance.GetCurrentPortalSettings();

            value = value.Replace("{CODE}", this._verificationCode);
            value = value.Replace("{LOGO_URL}", this._logo);
            value = value.Replace("{PORTALNAME}", portalSettings.PortalName);
            value = value.Replace("{PORTALALIAS}", portalSettings.DefaultPortalAlias);

            return value;
        }
    }
}