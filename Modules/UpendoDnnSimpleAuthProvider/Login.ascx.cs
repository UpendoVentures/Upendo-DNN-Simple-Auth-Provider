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
    using System.Linq;
    using System.Net.Mail;
    using System.Net.PeerToPeer;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Hotcakes.Commerce.Accounts;
    using Microsoft.Extensions.DependencyInjection;
    using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components;
    using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Data;
    using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Data.Cryptography;
    using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Models;
    using Globals = DotNetNuke.Common.Globals;
    using Host = DotNetNuke.Entities.Host.Host;

    /// <summary>
    /// The Login AuthenticationLoginBase is used to provide a login for a registered user
    /// portal.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Login : AuthenticationLoginBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Login));
        private readonly INavigationManager _navigationManager;
        protected int CounterValue = 0;

        public Login()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>
        /// Gets a value indicating whether check if the Auth System is Enabled (for the Portal).
        /// </summary>
        /// <remarks></remarks>
        public override bool Enabled
        {
            get
            {
                return AuthenticationConfig.GetConfig(this.PortalId).Enabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the Captcha control is used to validate the login.
        /// </summary>
        protected bool UseCaptcha
        {
            get
            {
                return AuthenticationConfig.GetConfig(this.PortalId).UseCaptcha;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ClientResourceManager.RegisterStyleSheet(((Control)this).Page, ControlPath + "module.css", 100);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                moodleRestUrl.Value = Request["resturl"];
                moodleWantsUrl.Value = Request["wantsurl"];
            }

            this.cmdLogin.Click += this.OnLoginClick;

            this.cancelLink.NavigateUrl = this.GetRedirectUrl(false);

            if (this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration)
            {
                this.liRegister.Visible = false;
            }

            this.lblLogin.Text = Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_LOGIN_INSTRUCTIONS");
            this.LoginHeader.InnerText = Localization.GetString("LoginHeader", this.LocalResourceFile);

            if (string.IsNullOrEmpty(this.lblLogin.Text))
            {
                this.lblLogin.AssociatedControlID = string.Empty;
            }

            if (this.Request.QueryString["usernameChanged"] == "true")
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_USERNAME_CHANGED_INSTRUCTIONS"), ModuleMessage.ModuleMessageType.BlueInfo);
            }

            var returnUrl = this._navigationManager.NavigateURL();
            string url;
            if (this.PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
            {
                if (!string.IsNullOrEmpty(UrlUtils.ValidReturnUrl(this.Request.QueryString["returnurl"])))
                {
                    returnUrl = this.Request.QueryString["returnurl"];
                }

                returnUrl = HttpUtility.UrlEncode(returnUrl);

                url = Globals.RegisterURL(returnUrl, Null.NullString);
                this.registerLink.NavigateUrl = url;
                if (this.PortalSettings.EnablePopUps && this.PortalSettings.RegisterTabId == Null.NullInteger
                    && !AuthenticationController.HasSocialAuthenticationEnabled(this))
                {
                    this.registerLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(url, this, this.PortalSettings, true, false, 600, 950));
                }
            }
            else
            {
                this.registerLink.Visible = false;
            }

            // see if the portal supports persistant cookies
            this.chkCookie.Visible = Host.RememberCheckbox;

            // no need to show password link if feature is disabled, let's check this first
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                url = this._navigationManager.NavigateURL("SendPassword", "returnurl=" + returnUrl);
                this.passwordLink.NavigateUrl = url;
                if (this.PortalSettings.EnablePopUps)
                {
                    this.passwordLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(url, this, this.PortalSettings, true, false, 300, 650));
                }
            }
            else
            {
                this.passwordLink.Visible = false;
            }

            if (!this.IsPostBack)
            {
                if (!string.IsNullOrEmpty(this.Request.QueryString["verificationcode"]) && this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    if (this.Request.IsAuthenticated)
                    {
                        this.Controls.Clear();
                    }

                    var verificationCode = this.Request.QueryString["verificationcode"];

                    try
                    {
                        UserController.VerifyUser(verificationCode.Replace(".", "+").Replace("-", "/").Replace("_", "="));

                        var redirectTabId = this.PortalSettings.Registration.RedirectAfterRegistration;

                        if (this.Request.IsAuthenticated)
                        {
                            this.Response.Redirect(this._navigationManager.NavigateURL(redirectTabId > 0 ? redirectTabId : this.PortalSettings.HomeTabId, string.Empty, "VerificationSuccess=true"), true);
                        }
                        else
                        {
                            if (redirectTabId > 0)
                            {
                                var redirectUrl = this._navigationManager.NavigateURL(redirectTabId, string.Empty, "VerificationSuccess=true");
                                redirectUrl = redirectUrl.Replace(Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias), string.Empty);
                                this.Response.Cookies.Add(new HttpCookie("returnurl", redirectUrl) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" });
                            }

                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("VerificationSuccess", this.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                        }
                    }
                    catch (UserAlreadyVerifiedException)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserAlreadyVerified", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                    catch (InvalidVerificationCodeException)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                    catch (UserDoesNotExistException)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserDoesNotExist", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                    catch (Exception)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }

            if (!this.Request.IsAuthenticated)
            {
                if (!this.Page.IsPostBack)
                {
                    try
                    {
                        if (this.Request.QueryString["username"] != null)
                        {
                            this.txtUsername.Text = this.Request.QueryString["username"];
                        }
                    }
                    catch (Exception ex)
                    {
                        // control not there
                        Logger.Error(ex);
                    }
                }

                try
                {
                    Globals.SetFormFocus(string.IsNullOrEmpty(this.txtUsername.Text) ? this.txtUsername : this.txtPassword);
                }
                catch (Exception ex)
                {
                    // Not sure why this Try/Catch may be necessary, logic was there in old setFormFocus location stating the following
                    // control not there or error setting focus
                    Logger.Error(ex);
                }
            }

            var registrationType = this.PortalSettings.Registration.RegistrationFormType;
            bool useEmailAsUserName;
            if (registrationType == 0)
            {
                useEmailAsUserName = this.PortalSettings.Registration.UseEmailAsUserName;
            }
            else
            {
                var registrationFields = this.PortalSettings.Registration.RegistrationFields;
                useEmailAsUserName = !registrationFields.Contains("Username");
            }

            this.plUsername.Text = this.LocalizeString(useEmailAsUserName ? "Email" : "Username");
            this.divCaptcha1.Visible = this.UseCaptcha;
            this.divCaptcha2.Visible = this.UseCaptcha;
        }

        protected string GetRedirectUrl(bool checkSettings = true)
        {
            var redirectUrl = string.Empty;
            var redirectAfterLogin = this.PortalSettings.Registration.RedirectAfterLogin;
            if (checkSettings && redirectAfterLogin > 0) // redirect to after registration page
            {
                redirectUrl = this._navigationManager.NavigateURL(redirectAfterLogin);
            }
            else
            {
                if (this.Request.QueryString["returnurl"] != null)
                {
                    // return to the url passed to register
                    redirectUrl = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                    // clean the return url to avoid possible XSS attack.
                    redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);

                    if (redirectUrl.Contains("?returnurl"))
                    {
                        string baseURL = redirectUrl.Substring(
                            0,
                            redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal));
                        string returnURL =
                            redirectUrl.Substring(redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

                        redirectUrl = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                    }
                }

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    // redirect to current page
                    redirectUrl = this._navigationManager.NavigateURL();
                }
            }

            return redirectUrl;
        }
        /// <summary>
        /// A detail to keep in mind. This method has been modified, the parameter that arrives in the txtPassword variable is really the Verification Code.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoginClick(object sender, EventArgs e)
        {
            if ((this.UseCaptcha && this.ctlCaptcha.IsValid) || !this.UseCaptcha)
            {
                var loginStatus = UserLoginStatus.LOGIN_FAILURE;
                string userName = PortalSecurity.Instance.InputFilter(
                    this.txtUsername.Text,
                    PortalSecurity.FilterFlag.NoScripting |
                                        PortalSecurity.FilterFlag.NoAngleBrackets |
                                        PortalSecurity.FilterFlag.NoMarkup);

                // DNN-6093
                // check if we use email address here rather than username
                UserInfo userByEmail = null;
                var emailUsedAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false);

                if (emailUsedAsUsername)
                {
                    // one additonal call to db to see if an account with that email actually exists
                    userByEmail = UserController.GetUserByEmail(PortalController.GetEffectivePortalId(this.PortalId), userName);

                    if (userByEmail != null)
                    {
                        // we need the username of the account in order to authenticate in the next step
                        userName = userByEmail.Username;
                    }
                }

                UserInfo objUser = null;

                if (!emailUsedAsUsername || userByEmail != null)
                {
                    this.txtPassword.Text = this.txtPassword.Text.Trim();
                    objUser = UserController.GetUserByName(userName);

                    //Note: In the password field what comes is the verification code.
                    // Check if the verification code textbox (txtPassword.Text) is not empty.
                    if (!string.IsNullOrEmpty(this.txtPassword.Text))
                    {
                        // If the entered verification code is valid (exists in the database), proceed with validation.
                        if (UtilityMethods.ValidateCodeInDB(userName, this.txtPassword.Text))
                        {
                            // Check if the verification code has expired.
                            if (UtilityMethods.ValidateExpiredVerificationCode(userName, this.txtPassword.Text))
                            {
                                // The verification code has expired for the user.
                                // Log the login failure attempt and display an error message to the user.
                                loginStatus = UserLoginStatus.LOGIN_FAILURE;
                                EventLogController.Instance.AddLog("User Login - Expired Verification Code", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.LOGIN_FAILURE);
                                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ExpiredVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            }
                            else
                            {
                                // The verification code is valid and has not expired.
                                // Now, check if the verification code matches the user's actual account.
                                if (UtilityMethods.ValidateUser(userName, this.txtPassword.Text))
                                {
                                    
                                    // Successful login, add log entry
                                    EventLogController.Instance.AddLog("User Login - Successful", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.LOGIN_SUCCESS);
                                    if (objUser.IsSuperUser)
                                    {
                                        loginStatus = UserLoginStatus.LOGIN_SUPERUSER;
                                    }
                                    else
                                    {
                                        loginStatus = UserLoginStatus.LOGIN_SUCCESS;
                                    }
                                }
                                else
                                {

                                    // The verification code does not match the user's account.
                                    // Set the login status to indicate a login failure and display an error message.
                                    loginStatus = UserLoginStatus.LOGIN_FAILURE;
                                    EventLogController.Instance.AddLog("User Login - Invalid Verification Code", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.LOGIN_FAILURE);
                                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                                }
                            }
                        }
                        else
                        {
                            // The entered verification code is invalid (does not exist in the database).
                            // Set the login status to indicate a login failure and display an error message.
                            loginStatus = UserLoginStatus.LOGIN_FAILURE;
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("AccessDenied", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);

                        }
                    }
                    else
                    {
                        // The verification code textbox is empty.
                        // Set the login status to indicate a login failure and display an error message.
                        loginStatus = UserLoginStatus.LOGIN_FAILURE;
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("AccessDenied", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }

                var authenticated = Null.NullBoolean;
                var message = Null.NullString;
                if (loginStatus == UserLoginStatus.LOGIN_USERNOTAPPROVED)
                {
                    message = "UserNotAuthorized";
                }
                else
                {
                    authenticated = loginStatus != UserLoginStatus.LOGIN_FAILURE;
                }


                // Raise UserAuthenticated Event
                var eventArgs = new UserAuthenticatedEventArgs(objUser, userName, loginStatus, "DNN")
                {
                    Authenticated = authenticated,
                    Message = message,
                    RememberMe = this.chkCookie.Checked,
                };
                this.OnUserAuthenticated(eventArgs);
            }
        }

        /// <summary>
        /// This method is responsible for handling the logic to send a verification code to the user's email address. 
        /// It is triggered when the user clicks the "Send Code" button on the login page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            string userName = PortalSecurity.Instance.InputFilter(
                 this.txtUsername.Text,
                 PortalSecurity.FilterFlag.NoScripting |
                                     PortalSecurity.FilterFlag.NoAngleBrackets |
                                     PortalSecurity.FilterFlag.NoMarkup);

            
            // check if we use email address here rather than username
            UserInfo userByEmail = null;
            var emailUsedAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false);

            if (emailUsedAsUsername)
            {
                // one additonal call to db to see if an account with that email actually exists
                userByEmail = UserController.GetUserByEmail(PortalController.GetEffectivePortalId(this.PortalId), userName);

                if (userByEmail != null)
                {
                    // we need the username of the account in order to authenticate in the next step
                    userName = userByEmail.Username;
                }
            }

            UserInfo objUser = null;

            if (!emailUsedAsUsername || userByEmail != null)
            {
                // Get the user object (objUser) by using the provided username (userName) to retrieve the user by their username.
                objUser = UserController.GetUserByName(userName);
            }

            // Check if the user object (objUser) is not null
            if (objUser != null)
            {
                // Get the user's name from the user object
                string username = objUser.Username;

                // Get the user's email address from the user object
                string emailAddress = objUser.Email;

                // Generate a random verification code
                string code = RandomCodeGenerator.Generate();

                // Convert the code to JSON and encrypt it using AES encryption
                var json = JsonUtility.ObjectToJson(code);
                var key = KeyManager.GetKey(0);
                var encrypted = AesEncryption.Encode(json, key);

                // Get the current date and time
                DateTime createOneDate = DateTime.Now;

                // Set the expiration date for the verification code to be 15 minutes from the creation date
                DateTime expirationdate = createOneDate.AddMinutes(15);

                // Check if the user's email already exists in the database (dataInDB)
                var dataInDB = VerificationCodeRepository.Instance.GetItems().Any(s => s.Username.Equals(username));

                if (dataInDB)
                {
                    // The user's email exists in the database, retrieve the existing verification code entry
                    var existingItem = VerificationCodeRepository.Instance.GetItems().SingleOrDefault(s => s.Username.Equals(username));

                    // Determine the time limit for requesting a new verification code based on the number of attempts (try)
                    int timeToCompare = existingItem.Try < 3 ? 60 : 3600;

                    // Calculate the time difference in seconds between the current time and the time the last verification code was generated
                    int difference = UtilityMethods.CalculateDifferenceInSeconds(existingItem.CreatedOnDate);

                    if (difference <= timeToCompare)
                    {
                        // The time limit for requesting a new verification code has not been reached
                        // Calculate the remaining time in seconds
                        int rest = timeToCompare - difference;

                        // Inform the user of the remaining time in seconds or minutes
                        if (rest > 60)
                        {
                            valueMessageSpan.InnerText = " minutes.";
                            valueTimeSpan.InnerText = UtilityMethods.FormatTime(rest);
                            valueTryMessageSpan.InnerText = "";
                        }
                        else
                        {
                            valueMessageSpan.InnerText = " seconds.";
                            valueTimeSpan.InnerText = rest.ToString();

                            // Inform the user of the remaining attempts, if applicable
                            if (existingItem.Try == 1 || existingItem.Try == 3)
                            {
                                valueTryMessageSpan.InnerText = " (Two more attempts left)";
                            }
                            if (existingItem.Try == 2)
                            {
                                valueTryMessageSpan.InnerText = " (One more attempt left)";

                            }
                        }
                    }
                    else
                    {
                        // The time limit has been exceeded, update the verification code entry in the database with a new code
                        existingItem.Try = existingItem.Try < 3 ? existingItem.Try += 1 : 1;
                        existingItem.ValidationPacket = encrypted;
                        existingItem.CreatedOnDate = DateTime.Now;
                        existingItem.Username = username;
                        existingItem.ExpirationDate = expirationdate;
                        VerificationCodeRepository.Instance.UpdateItem(existingItem);

                        // Inform the user of the new time limit and attempts
                        if (existingItem.Try == 2 || existingItem.Try == 1)
                        {
                            CounterValue = 60;
                            valueMessageSpan.InnerText = " seconds.";
                            valueTimeSpan.InnerText = CounterValue.ToString();
                            if (existingItem.Try == 2)
                            {
                                valueTryMessageSpan.InnerText = " (One more attempt left)";
                                EventLogController.Instance.AddLog("Verification Code Request - Second Attempt", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.ADMIN_ALERT);
                            }
                            else
                            {
                                valueTryMessageSpan.InnerText = " (Two more attempts left)";
                                EventLogController.Instance.AddLog("Verification Code Request - First Attempt", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.ADMIN_ALERT);
                            }
                        }

                        if (existingItem.Try == 3)
                        {
                            CounterValue = 3600;
                            valueMessageSpan.InnerText = " minutes.";
                            valueTimeSpan.InnerText = "1:00:00";
                            valueTryMessageSpan.InnerText = "";
                            EventLogController.Instance.AddLog("Verification Code Request - Third Try", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.ADMIN_ALERT);

                        }
                        try
                        {
                            // Send the new verification code to the user's email
                            Email.Send(emailAddress, code);
                            EventLogController.Instance.AddLog("Verification Code Send - Successful", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.ADMIN_ALERT);
                        }
                        catch (Exception)
                        {
                            EventLogController.Instance.AddLog("Verification Code Send - Failure", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.LOG_NOTIFICATION_FAILURE);
                        }
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SendEmailVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.BlueInfo);
                    }
                }
                else
                {
                    // The user's email does not exist in the database, create a new verification code entry with the generated code
                    var data = new VerificationCode
                    {
                        ValidationPacket = encrypted,
                        CreatedOnDate = DateTime.Now,
                        Username = username,
                        ExpirationDate = expirationdate,
                        Try = 1
                    };

                    CounterValue = 60;
                    valueMessageSpan.InnerText = " seconds.";
                    valueTryMessageSpan.InnerText = " (Two more attempts left)";
                    valueTimeSpan.InnerText = CounterValue.ToString();
                    EventLogController.Instance.AddLog("Verification Code Request - First Attempt", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.ADMIN_ALERT);
                    VerificationCodeRepository.Instance.CreateItem(data);
                    try
                    {
                        // Send the new verification code to the user's email
                        Email.Send(emailAddress, code);
                        EventLogController.Instance.AddLog("Verification Code Send - Successful", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.ADMIN_ALERT);

                    }
                    catch (Exception)
                    {
                        EventLogController.Instance.AddLog("Verification Code Send - Failure", "Username: " + userName, PortalController.Instance.GetCurrentSettings(), objUser.UserID, EventLogController.EventLogType.LOG_NOTIFICATION_FAILURE);
                    }
                  

                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SendEmailVerificationCode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.BlueInfo);
                }
            }
            else
            {
                // The user object is null, display a module message
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserDoesNotExist", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }
        }


       

    }
}

