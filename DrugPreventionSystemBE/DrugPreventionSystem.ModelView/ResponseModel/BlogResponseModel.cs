namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class BlogResponseModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }

        public string Content { get; set; }
        public string BlogImgUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string FullName { get; set; } // Tên đầy đủ của người dùng
        public string UserAvatar { get; set; }
    }
}
