namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel
{
    public class UserSearchModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? AgeGroup { get; set; }
        public bool? IsDeleted { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt"; //default
        public bool IsDescending { get; set; } = true;
    }
}