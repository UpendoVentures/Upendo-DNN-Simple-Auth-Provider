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