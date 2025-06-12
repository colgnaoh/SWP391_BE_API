namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class GetCoursesByPageResponse
    {
        public bool Success { get; set; }
        public List<CourseResponseModel> Data { get; set; }
    }
}
