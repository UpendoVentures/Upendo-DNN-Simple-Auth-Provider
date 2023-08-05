using DotNetNuke.Data;
using DotNetNuke.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Models;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Data
{
    public interface IVerificationCodeRepository
    {
        void CreateItem(VerificationCode t);
        void DeleteItem(int Id);
        void DeleteItem(VerificationCode t);
        IEnumerable<VerificationCode> GetItems();
        VerificationCode GetItem(int Id);
        void UpdateItem(VerificationCode t);
    }

    public class VerificationCodeRepository : ServiceLocator<IVerificationCodeRepository, VerificationCodeRepository>, IVerificationCodeRepository
    {
        public void CreateItem(VerificationCode t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VerificationCode>();
                rep.Insert(t);
            }
        }

        public void DeleteItem(int Id)
        {
            var t = GetItem(Id);
            DeleteItem(t);
        }

        public void DeleteItem(VerificationCode t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VerificationCode>();
                rep.Delete(t);
            }
        }

        public IEnumerable<VerificationCode> GetItems()
        {
            IEnumerable<VerificationCode> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VerificationCode>();
                t = rep.Get();
            }
            return t;
        }

        public VerificationCode GetItem(int Id)
        {
            VerificationCode t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VerificationCode>();
                t = rep.GetById(Id);
            }
            return t;
        }

        public void UpdateItem(VerificationCode t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VerificationCode>();
                rep.Update(t);
            }
        }

        protected override System.Func<IVerificationCodeRepository> GetFactory()
        {
            return () => new VerificationCodeRepository();
        }
   
    }
}