﻿using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class GetBlogsByPageResponse
    {
        public bool Success { get; set; }
        public List<BlogResponseModel> Data { get; set; }
        public int? TotalCount { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
    }
}
