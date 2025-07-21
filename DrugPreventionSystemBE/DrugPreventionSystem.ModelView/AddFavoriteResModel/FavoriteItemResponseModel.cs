namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteResModel
{
    public class FavoriteItemResponseModel
    {
        public Guid TargetId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; } // "Course" or "Program"
        public string? ImgUrl { get; set; }
        public string? Description { get; set; }
    }

}
