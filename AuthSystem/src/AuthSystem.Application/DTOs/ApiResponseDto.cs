using System.Collections.Generic;

namespace AuthSystem.Application.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ApiResponseDto<T> Success(T data, string? message = null)
        {
            return new ApiResponseDto<T>
            {
                Succeeded = true,
                Data = data,
                Message = message ?? string.Empty
            };
        }

        public static ApiResponseDto<T> Failure(string errorMessage, string? errorCode = null)
        {
            return new ApiResponseDto<T>
            {
                Succeeded = false,
                Message = errorMessage ?? string.Empty,
                ErrorCode = errorCode ?? string.Empty
            };
        }

        public static ApiResponseDto<T> Failure(List<string> errors, string? errorCode = null)
        {
            return new ApiResponseDto<T>
            {
                Succeeded = false,
                Errors = errors ?? new List<string>(),
                ErrorCode = errorCode ?? string.Empty
            };
        }
    }
}