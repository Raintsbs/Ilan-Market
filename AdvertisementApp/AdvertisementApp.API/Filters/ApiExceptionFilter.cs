using AdvertisementApp.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdvertisementApp.API.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is InvalidOperationException op)
            {
                context.Result = new ObjectResult(ApiResponse.Fail(op.Message))
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
                context.ExceptionHandled = true;
                return;
            }

            _logger.LogError(context.Exception, "Unhandled API exception");

            context.Result = new ObjectResult(ApiResponse.Fail("Beklenmeyen bir hata oluştu."))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
        }
    }
}
