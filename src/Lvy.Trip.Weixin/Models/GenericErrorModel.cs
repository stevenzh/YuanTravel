using System.Collections.Generic;
using System.Web.Mvc;

namespace Lvy.Trip.Weixin.Models
{
    public class GenericErrorModel
    {
        private IList<string> _errors;
        public IEnumerable<string> Errors { get { return _errors; } }

        public GenericErrorModel SetModelErrors(ViewDataDictionary viewData)
        {
            foreach (ModelState modelState in viewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    AddMessage(error.Exception != null ? "Server error" : error.ErrorMessage);
                }
            }

            return this;
        }

        public void AddMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (_errors == null)
                _errors = new List<string>();

            _errors.Add(message);
        }
    }
}