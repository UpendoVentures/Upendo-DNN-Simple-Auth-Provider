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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.PeerToPeer;
using DotNetNuke.Common.Utilities;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Data;
using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Data.Cryptography;
using DotNetNuke.Entities.Portals;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components
{
    /// <summary>
    /// Class containing various methods used in Login.ascx.cs
    /// </summary>
    public class UtilityMethods
    {
        // Method to validate if a verification code exists in the database for a given user.
        // Parameters:
        //   userName: The username for which the verification code is being checked.
        //   code: The verification code to be checked for existence in the database.
        // Returns:
        //   A boolean value indicating whether the verification code exists in the database (true) or not (false).
        public static bool ValidateCodeInDB(string userName, string code)
        {
            // Retrieve the user information for the provided username from the user controller.
            UserInfo objUser = UserController.GetUserByName(userName);

            // Check if any verification code items in the database have a matching email with the provided username.
            // The VerificationCodeRepository is assumed to be a repository or data access layer handling verification codes.
            // It is checked if there are any items in the repository whose email matches the provided username.
            // This implies that a verification code has been generated for the user if there is a match.
            if (VerificationCodeRepository.Instance.GetItems().Any(s => s.Username.Equals(userName)))
            {
                // If a verification code exists for the user, return true.
                return true;
            }

            // If no verification code is found for the user, return false.
            return false;
        }

        // Method to validate a user based on the provided verification code.
        // Parameters:
        //   userName: The username for which the user is being validated.
        //   code: The verification code to be checked for validation.
        // Returns:
        //   A boolean value indicating whether the user is valid (true) or not (false).
        public static bool ValidateUser(string userName, string code)
        {
            // Retrieve the user information for the provided username from the user controller.
            UserInfo objUser = UserController.GetUserByName(userName);

            // Retrieve the latest verification code data from the database for the user's email.
            // The VerificationCodeRepository is assumed to be a repository or data access layer handling verification codes.
            var dataInDB = VerificationCodeRepository.Instance.GetItems().LastOrDefault(s => s.Username.Trim().ToLower().Equals(objUser.Username.Trim().ToLower()));

            // Get the encryption key for decoding the verification code.
            var key = KeyManager.GetKey(0);

            // Decrypt the verification code using AES encryption.
            var desncrypted = AesEncryption.Decode(dataInDB.ValidationPacket, key);

            // Check if the decrypted verification code contains the provided code.
            // If it does, it means the user is valid.
            if (desncrypted.Contains(code))
            {
                // If the code is valid, return true.
                return true;
            }

            // If the code is not valid or does not match the decrypted verification code, return false.
            return false;
        }


        // Method to validate if a verification code has expired for a given user.
        // Parameters:
        //   userName: The username for which the verification code is being checked for expiration.
        //   code: The verification code to be checked for expiration.
        // Returns:
        //   A boolean value indicating whether the verification code has expired (true) or not (false).
        public static bool ValidateExpiredVerificationCode(string userName, string code)
        {
            // Retrieve the user information for the provided username from the user controller.
            UserInfo objUser = UserController.GetUserByName(userName);

            // Retrieve the latest verification code data from the database for the user's.
            // The VerificationCodeRepository is assumed to be a repository or data access layer handling verification codes.
            var dataInDB = VerificationCodeRepository.Instance.GetItems().LastOrDefault(s => s.Username.Trim().ToLower().Equals(objUser.Username.Trim().ToLower()));

            // Check if the expiration date of the verification code is less than or equal to the current date and time.
            // If the verification code has expired, it needs to be deleted from the repository.
            if (dataInDB.ExpirationDate <= DateTime.Now && dataInDB.Try <= 2)
            {
                // Delete the expired verification code from the repository.
                VerificationCodeRepository.Instance.DeleteItem(dataInDB.Id);

                // Return true to indicate that the verification code has expired.
                return true;
            }
            if (dataInDB.ExpirationDate <= DateTime.Now && dataInDB.Try == 3 && dataInDB.ExpirationDate.AddHours(1) <= DateTime.Now)
            {
                // Delete the expired verification code from the repository.
                VerificationCodeRepository.Instance.DeleteItem(dataInDB.Id);

                // Return true to indicate that the verification code has expired.
                return true;
            }

            // If the verification code has not expired, return false.
            return false;
        }


        // This method calculates the difference in seconds between the current time (DateTime.Now)
        // and the provided DateTime value (DateTimeAlmacenado).
        // Parameters:
        // - DateTime DateTimeAlmacenado: The DateTime value to calculate the difference from.
        // Returns:
        // - int: The difference in seconds between the current time and the provided DateTime.
        public static int CalculateDifferenceInSeconds(DateTime DateTimeStored)
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Calculate the difference between the current time and the provided DateTime
            TimeSpan difference = currentTime - DateTimeStored;

            // Get the total difference in seconds
            int differenceInSeconds = (int)difference.TotalSeconds;

            // Return the difference in seconds
            return differenceInSeconds;
        }

        // This method takes a total number of seconds and formats it into a string representation
        // in the format "hours:minutes:seconds".
        // Parameters:
        // - int totalSeconds: The total number of seconds to be formatted.
        // Returns:
        // - string: A formatted string representing the time in "hours:minutes:seconds" format.
        public static string FormatTime(int totalSeconds)
        {
            // Calculate the number of hours by dividing the totalSeconds by 3600 (60 seconds * 60 minutes)
            int hours = totalSeconds / 3600;

            // Calculate the remaining minutes after removing the hours, by using the modulo operator (%)
            // to get the remainder when dividing by 3600, and then dividing that by 60 (60 seconds per minute).
            int minutes = (totalSeconds % 3600) / 60;

            // Calculate the remaining seconds after removing both the hours and minutes, using the modulo operator (%)
            // to get the remainder when dividing by 60 (60 seconds per minute).
            int seconds = totalSeconds % 60;

            // Return the formatted time as a string using string interpolation.
            // The ":D2" format specifier ensures that both minutes and seconds are displayed as two digits,
            // with leading zeros if necessary.
            return $"{hours}:{minutes:D2}:{seconds:D2}";
        }

        // This method is used to retrieve the complete URL of a site's logo
        public static string GetSiteIconUrl()
        {
            // Get the configuration of the current portal using the PortalController's instance
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            // Get the filename of the site logo from the portal settings
            string logoFile = portalSettings.LogoFile;

            // Build the complete URL of the logo by combining portal alias, home directory, and logo filename
            string portalAlias = portalSettings.PortalAlias.HTTPAlias;
            string portalHomeDirectory = portalSettings.HomeDirectory;
            string completeLogoUrl = $"{portalAlias}{portalHomeDirectory}{logoFile}";

            // Return the complete logo URL
            return completeLogoUrl;
        }
    }
}