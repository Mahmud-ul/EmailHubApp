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
    public class SearchQueryManager : BaseManager<SearchQuery>, ISearchQueryManager
    {
        private readonly ISearchQueryRepository _repository;
        public SearchQueryManager(ISearchQueryRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
