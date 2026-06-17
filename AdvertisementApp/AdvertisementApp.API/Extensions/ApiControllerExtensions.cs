using AdvertisementApp.Common.Models;
using AdvertisementApp.Common.Result;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Extensions
{
    public static class ApiControllerExtensions
    {
        public static IActionResult ToActionResult<T>(this ControllerBase controller, IDataResult<T> result, int notFoundStatus = 404)
        {
            if (result.Success)
                return controller.Ok(ApiResponse<T>.Ok(result.Data!, result.Message));

            return notFoundStatus == 404
                ? controller.NotFound(ApiResponse<T>.Fail(result.Message))
                : controller.BadRequest(ApiResponse<T>.Fail(result.Message));
        }

        public static IActionResult ToActionResult(this ControllerBase controller, Common.Result.IResult result, int notFoundStatus = 404)
        {
            if (result.Success)
                return controller.Ok(ApiResponse.Ok(result.Message));

            return notFoundStatus == 404
                ? controller.NotFound(ApiResponse.Fail(result.Message))
                : controller.BadRequest(ApiResponse.Fail(result.Message));
        }
    }
}
