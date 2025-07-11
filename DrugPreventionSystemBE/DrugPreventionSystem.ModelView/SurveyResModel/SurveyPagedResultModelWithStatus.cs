﻿namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class SurveyPagedResultModelWithStatus
    {
        public bool Success { get; set; }
        public List<SurveyResponseModelWithStatus> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

}
