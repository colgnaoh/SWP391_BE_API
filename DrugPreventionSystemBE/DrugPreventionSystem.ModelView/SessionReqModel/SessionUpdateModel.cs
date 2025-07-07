namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SessionReqModel
{
    public class SessionUpdateModelView
    {
        public Guid? CourseId { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string? PositionOrder { get; set; }
    }

}
