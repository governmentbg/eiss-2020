using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace IOWebApplication.ModelBinders
{
    public class NomenclatureModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory loggerFactory;

        public NomenclatureModelBinderProvider(ILoggerFactory logFactory)
        {
            loggerFactory = logFactory;
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }

            if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType)
            {
                var propertyBinders = context.Metadata.Properties.ToDictionary(property => property, context.CreateBinder);
                return new NomenclatureModelBinder(propertyBinders, loggerFactory);
            }

            return null;
        }
    }
}
