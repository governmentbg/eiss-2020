using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;

namespace IOWebApplication.ModelBinders
{
    public class DoubleModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory loggerFactory;

        public DoubleModelBinderProvider(ILoggerFactory logFactory)
        {
            loggerFactory = logFactory;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(Double) || context.Metadata.ModelType == typeof(Double?))
            {
                return new DoubleModelBinder(loggerFactory);
            }

            return null;
        }
    }
}
