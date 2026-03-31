using IOWebApplication.Infrastructure.Extensions;
using System;

namespace IOWebApplicationService.Infrastructure.Extensions
{
    public static class GenericExtensions
    {
        public static bool IsEmpty(this Guid? value)
        {
            if(value == null || value == Guid.Empty)
            {
                return true;
            }
            return false;
        }

        public static void SanitizeNames(this Integration.Epep.Person model)
        {
            if(model == null)
            {
                return;
            }

            model.Firstname = (model.Firstname ?? " ").TrimLength(100);
            model.Secondname = (model.Secondname ?? " ").TrimLength(100);
            model.Lastname = (model.Lastname ?? ".").TrimLength(100);
        }

        public static void SanitizeNames(this Integration.Epep.Entity model)
        {
            if (model == null)
            {
                return;
            }

            model.Name = (model.Name ?? " ").TrimLength(500);
            model.Address = (model.Address ?? " ").TrimLength(500);
        }

        public static void SanitizeNames(this IOWebApplication.Infrastructure.Models.Integrations.EpepRest.Person model)
        {
            if (model == null)
            {
                return;
            }

            model.Firstname = (model.Firstname ?? " ").TrimLength(100);
            model.Secondname = (model.Secondname ?? " ").TrimLength(100);
            model.Lastname = (model.Lastname ?? ".").TrimLength(100);
        }

        public static void SanitizeNames(this IOWebApplication.Infrastructure.Models.Integrations.EpepRest.Entity model)
        {
            if (model == null)
            {
                return;
            }

            model.Name = (model.Name ?? " ").TrimLength(500);
            model.Address = (model.Address ?? " ").TrimLength(500);
        }
    }
}
