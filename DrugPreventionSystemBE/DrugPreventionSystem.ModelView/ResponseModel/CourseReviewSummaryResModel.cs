namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class CourseReviewSummaryResModel
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public List<ReviewResModel> Reviews { get; set; }
    }
}
