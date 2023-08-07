using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Caching;
using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Models.Interfaces;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Models
{
    [TableName("Upendo_Simple_Auth_VerificationCodes")]
    //setup the primary key for table
    [PrimaryKey("Id", AutoIncrement = true)]
    //configure caching using PetaPoco
    [Cacheable("VerificationCode", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    public class VerificationCode : IVerificationCode
    {
        public VerificationCode()
        {
            Id = -1;
        }
        public int Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int Try { get; set; }
    }


}