using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Models.Interfaces
{
    public interface IVerificationCode
    {
        int Id { get; set; }
        string Email { get; set; }
        string Code { get; set; }
        DateTime CreatedOnDate { get; set; }
        DateTime ExpirationDate { get; set; }
    }
}