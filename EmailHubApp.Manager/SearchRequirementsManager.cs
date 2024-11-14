using EmailHubApp.Manager.Base;
using EmailHubApp.Manager.Contract;
using EmailHubApp.Model.Models;
using EmailHubApp.Repository.Base;
using EmailHubApp.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Manager
{
    public class SearchRequirementsManager : BaseManager<SearchRequirements>, ISearchRequirementsManager
    {
        private readonly ISearchRequirementsRepository _repository;
        public SearchRequirementsManager(ISearchRequirementsRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
