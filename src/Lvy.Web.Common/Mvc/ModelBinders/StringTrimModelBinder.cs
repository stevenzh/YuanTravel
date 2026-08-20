using System.Web.Mvc;

namespace Lvy.Web.Common.Mvc.ModelBinders
{
    public class StringTrimModelBinder : DefaultModelBinder
    {

        public StringTrimModelBinder()
        {

        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = base.BindModel(controllerContext, bindingContext);
            if (value is string) return (value as string).Trim();
            return value;
        }
    }
}
