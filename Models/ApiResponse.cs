using Microsoft.AspNetCore.Mvc;

namespace CapfortuneBE.Models
{
    public static class ApiResponse
    {
        private static JsonResult Build(
            int statusCode,
            bool success,
            string message,
            object? data = null,
            long? count = null,
            int? offset = null,
            int? limit = null)
        {
            return new JsonResult(new ResponseStatus
            {
                Code = statusCode,
                Success = success,
                Message = message,
                Data = data,
                Count = count,
                Offset = offset,
                Limit = limit
            })
            {
                StatusCode = statusCode
            };
        }

        // 200 OK
        public static JsonResult Ok(
            object? data = null,
            string message = "Success")
        {
            return Build(200, true, message, data);
        }

        // 201 Created
        public static JsonResult Created(
            object? data = null,
            string message = "Created successfully")
        {
            return Build(201, true, message, data);
        }

        // 400 Bad Request
        public static JsonResult BadRequest(
            string message,
            object? data = null)
        {
            return Build(400, false, message, data);
        }

        // 401 Unauthorized
        public static JsonResult Unauthorized(
            string message = "Unauthorized")
        {
            return Build(401, false, message);
        }

        // 403 Forbidden
        public static JsonResult Forbidden(
            string message = "Access denied")
        {
            return Build(403, false, message);
        }

        // 404 Not Found
        public static JsonResult NotFound(
            string message = "Resource not found")
        {
            return Build(404, false, message);
        }

        // 409 Conflict
        public static JsonResult Conflict(
            string message)
        {
            return Build(409, false, message);
        }

        // 500 Error
        public static JsonResult Error(
            Exception ex,
            ILogger? logger = null,
            string message = "An unexpected error occurred")
        {
            logger?.LogError(ex, ex.Message);
            return Build(500, false, message);
        }

        public static JsonResult Paged<T>(
            IEnumerable<T> items,
            long total,
            int page,
            int pageSize,
            string message = "Success")
        {
            return Build(
                200,
                true,
                message,
                items,
                total,
                (page - 1) * pageSize,
                pageSize
            );
        }
    }

    public class ResponseStatus
    {
        public int Code { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public long? Count { get; set; }
        public int? Offset { get; set; }
        public int? Limit { get; set; }
    }
}