using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace IOWebApplication.ModelBinders
{
    public class DoubleModelBinder : IModelBinder
    {
        private readonly SimpleTypeModelBinder _baseBinder;

        public DoubleModelBinder(ILoggerFactory loggerFactory)
        {
            this._baseBinder = new SimpleTypeModelBinder(typeof(Double), loggerFactory);
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);

            if (valueResult != ValueProviderResult.None && !String.IsNullOrEmpty(valueResult.FirstValue))
            {
                double actualValue = 0;
                bool success = false;

                try
                {
                    string decValue = valueResult.FirstValue;
                    decValue = decValue.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    decValue = decValue.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    actualValue = Convert.ToDouble(decValue,
                        CultureInfo.CurrentCulture);
                    success = true;
                }
                catch (FormatException e)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, e, bindingContext.ModelMetadata);
                }

                if (success)
                {
                    bindingContext.Result = ModelBindingResult.Success(actualValue);
                    return Task.CompletedTask;
                }
            }

            return _baseBinder.BindModelAsync(bindingContext);
        }
    }
}
