﻿using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class CourseResponseModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public string? Content { get; set; }
        public CourseStatus? Status { get; set; }
        public targetAudience? TargetAudience { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? Slug { get; set; }
    }
}
