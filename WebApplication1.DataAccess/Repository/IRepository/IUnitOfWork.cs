using System;
using System.Collections.Generic;
using System.Text;

namespace WebApplication1.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICompanyRepository Company { get; }
        IApplicationUserRepository ApplicationUser { get; }
        ISP_Call SP_Call { get; }

        void Save();
    }
}
