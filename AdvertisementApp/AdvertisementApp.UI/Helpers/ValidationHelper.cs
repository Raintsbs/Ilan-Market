using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AdvertisementApp.UI.Helpers
{
    public static class ValidationHelper
    {
        public static void AddToModelState(this ValidationException exception, ModelStateDictionary modelState)
        {
            foreach (var error in exception.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
    }
}
