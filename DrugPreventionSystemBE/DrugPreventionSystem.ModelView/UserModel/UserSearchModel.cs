namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel
{
    public class UserSearchModel
    {
        // Pagination
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search filters
        public string? Keyword { get; set; }
        public List<string>? Role { get; set; }
        public bool? IsVerified { get; set; }
        public bool? Status { get; set; }
        public bool? IsDeleted { get; set; }
    }
}