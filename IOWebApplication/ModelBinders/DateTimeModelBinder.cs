// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace IOWebApplication.ModelBinders
{
    /// <summary>
    /// Author: Stamo Petkov
    /// Created: 07.01.2017
    /// Description: Корекция на формата на датата
    /// </summary>
    public class DateTimeModelBinder : IModelBinder
    {
        #region Fields

        private readonly string _customFormat;
        private readonly SimpleTypeModelBinder _baseBinder;

        #endregion

        public DateTimeModelBinder(string customFormat, ILoggerFactory loggerFactory)
        {
            this._customFormat = customFormat;
            this._baseBinder = new SimpleTypeModelBinder(typeof(DateTime), loggerFactory);
        }

        Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
            ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value != ValueProviderResult.None && !String.IsNullOrEmpty(value.FirstValue))
            {
                string valueAsString = value.FirstValue;

                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
                DateTime result = DateTime.MinValue;
                bool success = false;

                try
                {
                    result = DateTime.ParseExact(valueAsString, this._customFormat, CultureInfo.InvariantCulture);
                    success = true;
                }
                catch (FormatException)
                {
                    try
                    {
                        result = DateTime.Parse(valueAsString, new CultureInfo("bg-bg").DateTimeFormat);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, e, bindingContext.ModelMetadata);
                    }
                }
                catch (Exception e)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, e, bindingContext.ModelMetadata);
                }

                if (success)
                {
                    bindingContext.Result = ModelBindingResult.Success(result);
                    return Task.CompletedTask;
                }
            }

            return _baseBinder.BindModelAsync(bindingContext);
        }
    }
}
