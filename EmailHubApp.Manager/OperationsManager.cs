using Azure;
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
    public class OperationsManager : BaseManager<Operations>, IOperationsManager
    {
        private readonly IOperationsRepository _repository;
        public OperationsManager(IOperationsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
