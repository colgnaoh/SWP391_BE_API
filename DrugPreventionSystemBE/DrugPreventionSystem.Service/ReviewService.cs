using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ReviewReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class ReviewService : IReviewService
    {
        private readonly DrugPreventionDbContext _context;

        public ReviewService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewResModel>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Where(r => !r.IsDeleted)
                .Select(r => new ReviewResModel
                {
                    Id = r.Id,
                    CourseId = r.CourseId,
                    UserId = r.UserId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ReviewResModel?> GetReviewByIdAsync(Guid id)
        {
            var review = await _context.Reviews
                .Where(r => !r.IsDeleted && r.Id == id)
                .FirstOrDefaultAsync();

            if (review == null) return null;

            return new ReviewResModel
            {
                Id = review.Id,
                CourseId = review.CourseId,
                UserId = review.UserId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }


        public async Task<ReviewResModel> CreateReviewAsync(CreateReviewReqModel request)
        {
            var review = new Review
            {
                Id = Guid.NewGuid(),
                CourseId = request.CourseId,
                UserId = request.UserId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return new ReviewResModel
            {
                Id = review.Id,
                CourseId = review.CourseId,
                UserId = review.UserId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<bool> UpdateReviewAsync(Guid id, UpdateReviewReqModel request)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteReviewAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null || review.IsDeleted) return false;

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
