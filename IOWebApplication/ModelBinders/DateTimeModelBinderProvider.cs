using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;

namespace IOWebApplication.ModelBinders
{
    /// <summary>
    /// Author: Stamo Petkov
    /// Created: 07.01.2017
    /// Description: Корекция на формата на датата
    /// </summary>
    public class DateTimeModelBinderProvider : IModelBinderProvider
    {
        private readonly string _customFormat;

        private readonly ILoggerFactory loggerFactory;

        public DateTimeModelBinderProvider(string dateFormat, ILoggerFactory logFactory)
        {
            _customFormat = dateFormat;
            loggerFactory = logFactory;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?))
            {
                return new DateTimeModelBinder(_customFormat, loggerFactory);
            }

            return null;
        }
    }
}
