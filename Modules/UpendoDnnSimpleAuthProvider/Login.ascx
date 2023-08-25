<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Login" %>
<%@ Import Namespace="DotNetNuke.Common.Utilities" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>

<div class="pwLogin dnnForm dnnLoginService dnnClear">

    <div id="LoginHeader" class="dnnFormMessage dnnFormWarning text-center" runat="server">
  
    </div>

    <div class="dnnFormItem">
        <div class="dnnLabel">
            <asp:Label ID="plUsername" AssociatedControlID="txtUsername" runat="server" CssClass="dnnFormLabel" />
        </div>
        <asp:TextBox ID="txtUsername" runat="server" placeholder="Username" ValidationGroup="Upendo" />
    </div>


    <div id="msgCounter" class="dnnFormItem hidden" style="padding-left: 20%;">
        <p>Didn't see the email? You can try to resend the code in <strong><span id="valueTimeSpan" runat="server"></span></strong><span id="valueMessageSpan" runat="server"></span><span id="valueTryMessageSpan" runat="server"></span></p>
    </div>

    <div id="dnnFormItemSendButton" class="dnnFormItem">
        <div class="dnnLabel">
        </div>
        <asp:LinkButton ID="btnSendEmailDisabled" runat="server" Text="Send Code" CssClass="" Enabled="false" ValidationGroup="Upendo" />
        <asp:LinkButton ID="btnSendEmail" runat="server" Text="Send Code" CssClass="" OnClick="btnSendEmail_Click" ValidationGroup="Upendo" />
    </div>

    <div class="dnnFormItem">
        <div class="dnnLabel">
            <asp:Label ID="plPassword" AssociatedControlID="txtPassword" runat="server" resourcekey="Passwords" CssClass="dnnFormLabel" ViewStateMode="Disabled">Verification Code</asp:Label>
        </div>
        <asp:TextBox ID="txtPassword" runat="server" placeholder="Code Verification" />
    </div>

    <div class="dnnFormItem" id="divCaptcha1" runat="server" visible="false">
        <asp:Label ID="plCaptcha" AssociatedControlID="ctlCaptcha" runat="server" resourcekey="Captcha" CssClass="dnnFormLabel" />
    </div>
    <div class="dnnFormItem dnnCaptcha" id="divCaptcha2" runat="server" visible="false">
        <dnn:captchacontrol id="ctlCaptcha" captchawidth="130" captchaheight="40" runat="server" errorstyle-cssclass="dnnFormMessage dnnFormError dnnCaptcha" viewstatemode="Disabled" ValidationGroup="Upendo" />
    </div>
    <div class="dnnFormItem hidden">
        <asp:Label ID="lblLoginRememberMe" runat="server" CssClass="dnnFormLabel" />
        <span class="dnnLoginRememberMe">
            <asp:CheckBox ID="chkCookie" resourcekey="Remember" runat="server" ValidationGroup="Upendo" /></span>
    </div>
    <div class="dnnFormItem">
        <asp:Label ID="lblLogin" runat="server" AssociatedControlID="cmdLogin" CssClass="dnnFormLabel" ViewStateMode="Disabled" ValidationGroup="Upendo" />
        <asp:LinkButton ID="cmdLogin" resourcekey="cmdLogin" CssClass="dnnPrimaryAction" Text="Login" runat="server" CausesValidation="false" ValidationGroup="Upendo" />
        <asp:HyperLink ID="cancelLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="false" ValidationGroup="Upendo" />
    </div>
    <div class="dnnFormItem hidden">
        <span class="dnnFormLabel">&nbsp;</span>
        <div class="dnnLoginActions">
            <ul class="dnnActions dnnClear">
                <li id="liRegister" runat="server">
                    <asp:HyperLink ID="registerLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdRegister" ViewStateMode="Disabled" ValidationGroup="Upendo" /></li>
                <li id="liPassword" runat="server">
                    <asp:HyperLink ID="passwordLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdPassword" ViewStateMode="Disabled" ValidationGroup="Upendo" /></li>
            </ul>
        </div>
    </div>
    <asp:HiddenField ID="moodleRestUrl" runat="server" />
    <asp:HiddenField ID="moodleWantsUrl" runat="server" />
</div>

<dnn:dnnscriptblock runat="server">
    <script type="text/javascript">
        /*globals jQuery, window, Sys */
        (function ($, Sys) {
            const disabledActionClass = "dnnDisabledAction";
            const actionLinks = $('a[id^="dnn_ctr<%=ModuleId > Null.NullInteger ? ModuleId.ToString() : ""%>_Login_Upendo_Simple_Auth_Login_Upendo_Simple_Auth"]');
            function isActionDisabled($el) {
                return $el && $el.hasClass(disabledActionClass);
            }
            function disableAction($el) {
                if ($el == null || $el.hasClass(disabledActionClass)) {
                    return;
                }
                $el.addClass(disabledActionClass);
            }
            function enableAction($el) {
                if ($el == null) {
                    return;
                }
                $el.removeClass(disabledActionClass);
            }
            function setUpLogin() {
                $.each(actionLinks || [], function (index, action) {
                    var $action = $(action);
                    $action.click(function () {
                        var $el = $(this);
                        if (isActionDisabled($el)) {
                            return false;
                        }
                        disableAction($el);
                    });
                });
            }

            $(document).ready(function () {
                $(document).on('keydown', '.dnnLoginService', function (e) {
                    if ($(e.target).is('input:text,input:password,input:code,input:checkbox') && e.keyCode === 13) {
                        var $loginButton = $('#dnn_ctr<%=ModuleId > Null.NullInteger ? ModuleId.ToString() : ""%>_Login_Upendo_Simple_Auth_Login_Upendo_Simple_Auth_cmdLogin');
                        if (isActionDisabled($loginButton)) {
                            return false;
                        }
                        disableAction($loginButton);
                        window.setTimeout(function () { eval($loginButton.attr('href')); }, 100);
                        e.preventDefault();
                        return false;
                    }
                });
                setUpLogin();
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    $.each(actionLinks || [], function (index, item) {
                        enableAction($(item));
                    });
                    setUpLogin();
                });
            });
        }(jQuery, window.Sys));
    </script>
    <script type="text/javascript">
        // Get the elements by their IDs
        var valueMessageSpan = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_valueMessageSpan');
        var valueTimeSpan = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_valueTimeSpan');
        var sendVerificationCodeButton = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_btnSendEmail');
        var sendVerificationCodeButtonDisabled = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_btnSendEmailDisabled');
        var msgCounter = document.getElementById('msgCounter');

        // Check if the verification code type is "seconds."
        if (valueMessageSpan.innerText === " seconds.") {
            // Parse the initial value of the countdown timer to an integer
            var valueTime = parseInt(valueTimeSpan.innerText);

            // Function to update the countdown timer every second
            function updateCounter() {
                // If the timer is greater than 0
                if (valueTime > 0) {
                    // Show the countdown timer and associated elements, hide the sendVerificationCodeButton, and show the disabled button.
                    valueTimeSpan.classList.remove('hidden');
                    msgCounter.classList.remove('hidden');
                    sendVerificationCodeButton.classList.add('hidden');
                    sendVerificationCodeButtonDisabled.classList.remove('hidden');
                }

                // Update the display of the countdown timer
                valueTimeSpan.innerText = valueTime;

                // If the timer reaches 0
                if (valueTime === 0) {
                    // Hide the countdown timer and associated elements, show the sendVerificationCodeButton, and hide the disabled button.
                    valueTimeSpan.style.display = 'none';
                    msgCounter.classList.add('hidden');
                    sendVerificationCodeButton.classList.remove('hidden');
                    sendVerificationCodeButtonDisabled.classList.add('hidden');
                }
                else {
                    // Decrement the timer value by 1 second and call the updateCounter function again after 1 second.
                    valueTime--;
                    setTimeout(updateCounter, 1000);
                }
            }

            // Start the countdown timer update process
            updateCounter();
        }
        else {
            // Function to decrement the countdown timer
            function decrementCounter() {
                // Get the element containing the current time in format HH:mm:ss
                var timerDiv = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_valueTimeSpan');

                // Get the current time as a string and split it into hours, minutes, and seconds
                var currentTime = timerDiv.innerText;
                var timeParts = currentTime.split(':');
                var hours = parseInt(timeParts[0]);
                var minutes = parseInt(timeParts[1]);
                var seconds = parseInt(timeParts[2]);

                // Convert hours, minutes, and seconds to total seconds
                var totalSeconds = hours * 3600 + minutes * 60 + seconds;

                // Decrement one second from the total seconds
                totalSeconds--;

                // Convert the updated total seconds back to time format (HH:mm:ss)
                hours = Math.floor(totalSeconds / 3600);
                minutes = Math.floor((totalSeconds % 3600) / 60);
                seconds = totalSeconds % 60;

                // Update the content of the countdown timer element with the new time value
                timerDiv.textContent = formatTime(hours) + ':' + formatTime(minutes) + ':' + formatTime(seconds);

                // If the counter is greater, show the countdown timer, hide the disabled sendVerificationCodeButton, and show the enabled button.
                if (totalSeconds > 0) {
                    valueTimeSpan.classList.remove('hidden');
                    msgCounter.classList.remove('hidden');
                    sendVerificationCodeButton.classList.add('hidden');
                    sendVerificationCodeButtonDisabled.classList.remove('hidden');
                }

                // If the counter reaches 0, hide the countdown timer, show the sendVerificationCodeButton, and hide the disabled button.
                if (totalSeconds === 0) {
                    valueTimeSpan.style.display = 'none';
                    msgCounter.classList.add('hidden');
                    sendVerificationCodeButton.classList.remove('hidden');
                    sendVerificationCodeButtonDisabled.classList.add('hidden');
                    clearInterval(interval);
                }
            }

            // Function to format the time to two digits (e.g., 01, 02, ..., 09)
            function formatTime(time) {
                return time < 10 ? '0' + time : time;
            }

            // Call the function decrementCounter every second (1000ms) using setInterval
            var interval = setInterval(decrementCounter, 1000);
        }
    </script>
    <script type="text/javascript">
        // Function to enable or disable the button depending on the length of the text in the password field
        function toggleSendVerificationCodeButton() {
            // Get references to the password input field and both send verification code buttons
            var passwordInput = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_txtUsername');
            var sendVerificationCodeButton = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_btnSendEmail');
            var sendVerificationCodeButtonDisabled = document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_btnSendEmailDisabled');

            // Check if the password input length is less than 3 characters
            if (passwordInput.value.length < 3) {
                // If the password is too short, hide the enabled button and show the disabled button
                sendVerificationCodeButton.classList.add('hidden');
                sendVerificationCodeButtonDisabled.classList.remove('hidden');
            } else {
                // If the password is long enough, hide the disabled button and show the enabled button
                sendVerificationCodeButton.classList.remove('hidden');
                sendVerificationCodeButtonDisabled.classList.add('hidden');
            }
        }

        // Attach the toggleSendVerificationCodeButton function to the keyup event of the password field
        document.getElementById('dnn_ctr_Login_Upendo Simple Auth_Login_Upendo Simple Auth_txtUsername').addEventListener('keyup', toggleSendVerificationCodeButton);

        // Call the function initially to set the initial state of the button
        toggleSendVerificationCodeButton();
    </script>
</dnn:dnnscriptblock>
