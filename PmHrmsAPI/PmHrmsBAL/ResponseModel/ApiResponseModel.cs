namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class ApiResponseModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponseModel(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}

