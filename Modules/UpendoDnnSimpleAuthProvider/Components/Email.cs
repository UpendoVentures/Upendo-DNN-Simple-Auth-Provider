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

using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using DotNetNuke.Entities.Users;
using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components;
using DotNetNuke.Entities.Portals;
using System.IO;
using System.Web.Hosting;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components
{
    public class Email
    {
        public static void Send(string userEmail, string code)
        {
            var randomCode = code;
            string logo = UtilityMethods.GetSiteIconUrl().ToString();
            string emailSubject = "Upendo Authenticator Provider confirmation code: " + randomCode;
            var hostEmail = HostController.Instance.GetString("HostEmail");
            var toEmailAddress = userEmail;
            string valueSettings = PortalController.GetPortalSetting("UpendoSimpleDnnAuth.ConfirmEmail", DnnGlobal.Instance.GetPortalId(), string.Empty);
            string serverPath = HostingEnvironment.MapPath(valueSettings);
            var templateContent = File.ReadAllText(serverPath);
            string emailBody = templateContent;
            emailBody = emailBody.Replace("CODE", randomCode);
            emailBody = emailBody.Replace("LOGO_URL", logo);

            DotNetNuke.Services.Mail.Mail.SendEmail(hostEmail, toEmailAddress, emailSubject, emailBody);
        }

       
    }
}