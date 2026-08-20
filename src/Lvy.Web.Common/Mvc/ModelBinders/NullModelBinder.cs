using System;
using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.ModelBinders
{
    public class NullModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            return null;
        }
    }
}
