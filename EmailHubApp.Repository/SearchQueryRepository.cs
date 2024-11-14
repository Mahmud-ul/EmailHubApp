using EmailHubApp.Model.Models;
using EmailHubApp.Repository.Base;
using EmailHubApp.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Repository
{
    public class SearchQueryRepository : BaseRepository<SearchQuery>, ISearchQueryRepository
    {
    }
}
