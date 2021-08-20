using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebApplication1.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        void Update(ApplicationUser user);
    }
}
