// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.ModelBinders
{
    public class NomenclatureModelBinder : ComplexTypeModelBinder
    {
        private readonly ILogger logger;

        public NomenclatureModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory) : base(propertyBinders, loggerFactory)
        {
            logger = loggerFactory.CreateLogger(typeof(NomenclatureModelBinder));
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override Task BindProperty(ModelBindingContext bindingContext)
        {
            var form = bindingContext.HttpContext.Request.Form;
            ControllerActionDescriptor controllerActionDescriptor = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            var controllerName = string.Empty;
            if (controllerActionDescriptor != null)
            {
                controllerName = controllerActionDescriptor.ControllerName;
            }

            if (controllerName == "Nomenclature" && typeof(ICommonNomenclature).IsAssignableFrom(bindingContext.ModelMetadata.ContainerType)
            && (form.ContainsKey(bindingContext.ModelMetadata.PropertyName) || (bindingContext.ModelMetadata.IsEnumerableType)))
            {
                try
                {
                    if (bindingContext.ModelMetadata.ModelType == typeof(Boolean))
                    {
                        bool value = form[bindingContext.ModelMetadata.PropertyName].Contains("true") ? true : false;

                        bindingContext.Result = ModelBindingResult.Success(value);
                    }
                    else if (bindingContext.ModelMetadata.ModelType == typeof(DateTime) || bindingContext.ModelMetadata.ModelType == typeof(DateTime?))
                    {
                        string value = form[bindingContext.ModelMetadata.PropertyName];
                        DateTime result = DateTime.MinValue;

                        if (DateTime.TryParse(value, new CultureInfo("bg-bg").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces, out result))
                        {
                            bindingContext.Result = ModelBindingResult.Success(result);
                        }
                        else if (DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                        {
                            bindingContext.Result = ModelBindingResult.Success(result);
                        }
                        else if (bindingContext.ModelMetadata.ModelType == typeof(DateTime?))
                        {
                            bindingContext.Result = ModelBindingResult.Success(null);
                        }
                        else
                        {
                            bindingContext.Result = ModelBindingResult.Failed();
                        }
                    }
                    else
                    {
                        var value = Convert.ChangeType((string)form[bindingContext.ModelMetadata.PropertyName], bindingContext.ModelMetadata.ModelType);
                        bindingContext.Result = ModelBindingResult.Success(value);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("Error binding generic nomenclature properties", ex);
                    bindingContext.Result = ModelBindingResult.Failed();
                }

            }
            else
            {
                base.BindProperty(bindingContext);
            }

            return Task.CompletedTask;
        }

        protected override bool CanBindProperty(ModelBindingContext bindingContext, ModelMetadata propertyMetadata)
        {
            return base.CanBindProperty(bindingContext, propertyMetadata);
        }

        protected override object CreateModel(ModelBindingContext bindingContext)
        {

            var factory = bindingContext.HttpContext.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
            var tempData = factory.GetTempData(bindingContext.HttpContext);
            string nomenclatureName = (string)tempData.Peek("NomenclatureName");

            if (bindingContext.ModelMetadata.ModelType == typeof(object) && nomenclatureName != null)
            {
                Type nomenclatureType = Type.GetType(String.Format(NomenclatureConstants.AssemblyQualifiedName, nomenclatureName), false);
                var model = Activator.CreateInstance(nomenclatureType);
                bindingContext.ModelMetadata = bindingContext.ModelMetadata.GetMetadataForType(nomenclatureType);

                return model;
            }

            return base.CreateModel(bindingContext);
        }

        protected override void SetProperty(ModelBindingContext bindingContext, string modelName, ModelMetadata propertyMetadata, ModelBindingResult result)
        {
            base.SetProperty(bindingContext, modelName, propertyMetadata, result);
        }
    }
}
