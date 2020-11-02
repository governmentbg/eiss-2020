// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;

namespace IOWebApplication.ModelBinders
{
    /// <summary>
    /// Author: Stamo Petkov
    /// Created: 07.01.2017
    /// Description: Корекция на десетичния разделител
    /// </summary>
    public class DecimalModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory loggerFactory;

        public DecimalModelBinderProvider(ILoggerFactory logFactory)
        {
            loggerFactory = logFactory;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(Decimal) || context.Metadata.ModelType == typeof(Decimal?))
            {
                return new DecimalModelBinder(loggerFactory);
            }

            return null;
        }
    }
}
