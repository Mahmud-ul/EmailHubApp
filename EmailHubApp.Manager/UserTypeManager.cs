using EmailHubApp.Manager.Base;
using EmailHubApp.Manager.Contract;
using EmailHubApp.Model.Models;
using EmailHubApp.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Manager
{
    public class UserTypeManager : BaseManager<UserType>, IUserTypeManager
    {
        private readonly IUserTypeRepository _irepository;
        public UserTypeManager(IUserTypeRepository irepository) : base(irepository)
        {
            _irepository = irepository;
        }
    }
}
