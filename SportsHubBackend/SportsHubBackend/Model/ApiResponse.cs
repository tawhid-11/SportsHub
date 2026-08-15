namespace SportsHubBackend.Model
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> Ok(string message, T data = default)
        {
            return new ApiResponse<T>(true, message, data);
        }

        public static ApiResponse<T> Error(string message)
        {
            return new ApiResponse<T>(false, message);
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() { }
        public ApiResponse(bool success, string message, object data = null) 
            : base(success, message, data) { }

        public static new ApiResponse Ok(string message, object data = null)
        {
            return new ApiResponse(true, message, data);
        }

        public static new ApiResponse Error(string message)
        {
            return new ApiResponse(false, message);
        }
    }
}
