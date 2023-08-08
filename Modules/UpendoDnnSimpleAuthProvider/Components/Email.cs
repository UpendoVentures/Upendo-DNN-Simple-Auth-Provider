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
            string fecha = DateTime.Now.ToString();
            string emailBody = @"
<!DOCTYPE html>
<html xmlns:v=""urn:schemas-microsoft-com:vml"">
<head>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=unicode"">
  <title>Upendo Authenticator Provider confirmation code:""; emailBody += randomCode; emailBody += $@""</title>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Montserrat:wght@300;400;700&display=swap');
    body {
      font-family: 'Montserrat', Arial, sans-serif; /* Utiliza la fuente Montserrat */
    }
    .header h1 {
      font-weight: 700; 
    }
    .heroparagraph,
    .contentparagraph {
      font-weight: 300;
    }
    .container {
      width: 100%;
      background: white;
      border-collapse: collapse;
      border-radius: 8px;
      box-shadow: 0px 0px 15px rgba(0, 0, 0, 0.1);
    }
    .header img {
      width: 120px;
      height: 36px;
      margin: 32px 0;
    }
    .code-box {
      background: #F5F4F5;
      text-align: center;
      vertical-align: middle;
      color: black;
      font-size: 22.5pt;
    }
    .footer {
      text-align: center;
      padding: 15px;
      color: dimgray;
    }
  </style>
</head>
<body>
  <table class=""container"">
    <tr>
      <td class=""header"" style=""padding: 24pt 0.75pt"">
        <div align=""center"">
         <img src=""http://"; emailBody += logo; emailBody += $@""" alt=""logo"">
        </div>
      </td>
    </tr>
    <tr>
      <td style=""padding: 0 0.75pt"">
        <div style=""margin: 0 37.5pt 22.5pt"">
          <h1>Confirm your email address</h1>
          <p class=""heroparagraph"">Your confirmation code is below — enter it in your open browser window and we'll help you get signed in.</p>
        </div>
        <div class=""code-box"">"; emailBody += randomCode; emailBody += $@"</div>
        <div style=""margin: 0 37.5pt 22.5pt"">
          <p class=""contentparagraph"">If you didn’t request this email, there’s nothing to worry about — you can safely ignore it.</p>
        </div>
      </td>
    </tr>
    <tr>
      <td style=""padding: 15pt 0.75pt"">
        <div class=""footer"">
          ©2023 Upendo Ventures.<br>
          548 Market St. #65401, San Francisco, CA 94104<br>All rights reserved.
        </div>
      </td>
    </tr>
  </table>
</body>
</html>";
            
            DotNetNuke.Services.Mail.Mail.SendEmail(hostEmail, toEmailAddress, emailSubject, emailBody);
        }

       
    }
}