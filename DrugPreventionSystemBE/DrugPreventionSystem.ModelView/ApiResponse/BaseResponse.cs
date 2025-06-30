namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty; // Khởi tạo để tránh null reference

        // Trường Data này sẽ chứa dữ liệu cụ thể (ví dụ: UserResponse, OrderResponse, v.v.)
        // Nó là kiểu object để có thể chứa bất kỳ kiểu dữ liệu nào.
        public object? Data { get; set; }

        public BaseResponse() { } // Constructor mặc định

        public BaseResponse(bool success, string message, object? data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}
