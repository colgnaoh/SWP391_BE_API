namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class BlogResponseModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
        public string BlogImgUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        }
}
