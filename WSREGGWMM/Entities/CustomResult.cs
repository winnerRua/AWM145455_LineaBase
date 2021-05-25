using Microsoft.AspNetCore.Mvc;

namespace WSREGGWMM.Entities
{
    public class CustomResult : JsonResult
    {
        public CustomResult(dynamic response, int statusCode)
            : base(new CustomError(response))
        {
            StatusCode = statusCode;
        }
    }

    public class CustomError
    {
        public dynamic response { get; }

        public CustomError(dynamic _response)
        {
            response = _response;
        }
    }
}
