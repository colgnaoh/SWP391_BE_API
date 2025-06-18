namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; } // Tùy chọn: Thêm message để mô tả lỗi/thành công
    }
}
