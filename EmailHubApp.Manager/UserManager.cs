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
    public class UserManager : BaseManager<User>, IUserManager
    {
        private readonly IUserRepository _repository;
        public UserManager(IUserRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
