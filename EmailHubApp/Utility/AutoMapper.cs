using AutoMapper;
using EmailHubApp.Model.Models;
using EmailHubApp.Models;
using System.Diagnostics;

namespace Ideal.Utility
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<User, UserOperation>();
            CreateMap<UserOperation, User>();

            CreateMap<UserType, UserTypeOperation>();
            CreateMap<UserTypeOperation, UserType>();

            CreateMap<Operations, OperationsOperation>();
            CreateMap<OperationsOperation, Operations>();

            CreateMap<SearchQuery, SearchQueryOperation>();
            CreateMap<SearchQueryOperation, SearchQuery>();

            CreateMap<SearchedData, SearchedDataOperation>();
            CreateMap<SearchedDataOperation, SearchedData>();

            CreateMap<SearchRequirements, SearchRequirementsOperation>();
            CreateMap<SearchRequirementsOperation, SearchRequirements>();
        }
    }
}
