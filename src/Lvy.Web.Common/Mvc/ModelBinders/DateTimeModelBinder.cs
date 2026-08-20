using System;
using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.ModelBinders
{
    public class DateTimeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException("bindingContext");

            if (!bindingContext.ModelName.Contains("Date"))
            {
                return bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            }
            ValueProviderResult valueResult = null;
            valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var time = valueResult.ConvertTo(typeof(DateTime));
            return time.ToDateTime();
        }

        private Nullable<T> GetA<T>(ModelBindingContext bindingContext, string key) where T : struct
        {
            if (String.IsNullOrEmpty(key)) return null;
            ValueProviderResult valueResult = null;
            //Try it with the prefix...
            valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + key);
            //Didn't work? Try without the prefix if needed...
            if (valueResult == null && bindingContext.FallbackToEmptyPrefix)
            {
                bindingContext.ValueProvider.GetValue(key);
            }
            if (valueResult == null)
            {
                return null;
            }
            return (Nullable<T>)valueResult.ConvertTo(typeof(T));
        }
    }
}