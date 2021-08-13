using AutoMapper;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;

namespace IntegrationService.Test.Mockups
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Common
            CreateMap<LawUnit, LawUnitH>();

            //Case
            CreateMap<Case, CaseH>();
            CreateMap<CasePerson, CasePersonH>();
            CreateMap<CasePersonAddress, CasePersonAddressH>();
            CreateMap<CaseSession, CaseSessionH>();
            CreateMap<CaseSessionAct, CaseSessionActH>();
            CreateMap<CaseNotification, CaseNotificationH>();
        }
    }
}
