using AutoMapper;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;

namespace ASI.Basecode.WebApp
{
    // AutoMapper configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure auto mapper
        /// </summary>
        private void ConfigureAutoMapper()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProfileConfiguration());
            });

            this._services.AddSingleton<IMapper>(sp => mapperConfiguration.CreateMapper());
        }

        private class AutoMapperProfileConfiguration : Profile
        {
            public AutoMapperProfileConfiguration()
            {
                CreateMap<UserViewModel, User>();
                CreateMap<UserModel, User>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.Password, o => o.Ignore())
                    .ForMember(d => d.CreatedTime, o => o.Ignore());
                CreateMap<User, UserModel>()
                    .ForMember(d => d.Password, o => o.Ignore());

                CreateMap<OrganizationViewModel, Organization>()
                    .ForMember(d => d.Id, o => o.Ignore());
                CreateMap<Organization, OrganizationViewModel>();

                // Status only changes through IEventService.ChangeStatus
                CreateMap<EventViewModel, Event>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.Status, o => o.Ignore())
                    .ForMember(d => d.Organization, o => o.Ignore());
            }
        }
    }
}
