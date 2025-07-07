using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse; 
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ReviewReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class ReviewService : IReviewService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewService(DrugPreventionDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> GetAllReviewsAsync()
        {
            try
            {
                var reviews = await _context.Reviews
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

                if (!reviews.Any())
                {
                    return new OkObjectResult(new BaseResponse(true, "Không tìm thấy đánh giá nào.", null));
                }

                return new OkObjectResult(new BaseResponse(true, "Lấy tất cả đánh giá thành công.", reviews));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy tất cả đánh giá: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetReviewByIdAsync(Guid id)
        {
            try
            {
                var review = await _context.Reviews
                    .Where(r => !r.IsDeleted && r.Id == id)
                    .FirstOrDefaultAsync();

                if (review == null)
                {
                    // Trả về 404 Not Found
                    return new NotFoundObjectResult(new BaseResponse(false, "Không tìm thấy đánh giá.", null));
                }

                var reviewResModel = new ReviewResModel
                {
                    Id = review.Id,
                    CourseId = review.CourseId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                // Trả về 200 OK
                return new OkObjectResult(new BaseResponse(true, "Lấy đánh giá theo ID thành công.", reviewResModel));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy đánh giá theo ID: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetReviewsByCourseIdAsync(Guid courseId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => !r.IsDeleted && r.CourseId == courseId)
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

                if (!reviews.Any())
                {
                    return new OkObjectResult(new BaseResponse(true, $"Không tìm thấy đánh giá nào cho khóa học ID '{courseId}'.", null));
                }

                return new OkObjectResult(new BaseResponse(true, $"Lấy đánh giá cho khóa học ID '{courseId}' thành công.", reviews));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy đánh giá theo Course ID: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetReviewsByUserIdAsync(Guid userId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => !r.IsDeleted && r.UserId == userId)
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

                if (!reviews.Any())
                {
                    return new OkObjectResult(new BaseResponse(true, $"Không tìm thấy đánh giá nào từ người dùng ID '{userId}'.", null));
                }

                return new OkObjectResult(new BaseResponse(true, $"Lấy đánh giá từ người dùng ID '{userId}' thành công.", reviews));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy đánh giá theo User ID: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetReviewsByAppointmentIdAsync(Guid appointmentId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => !r.IsDeleted && r.AppointmentId == appointmentId)
                    .Select(r => new ReviewResModel
                    {
                        Id = r.Id,
                        CourseId = r.CourseId,
                        AppointmentId = r.AppointmentId,
                        UserId = r.UserId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return new OkObjectResult(new BaseResponse(true, $"Không tìm thấy đánh giá nào cho cuộc hẹn ID '{appointmentId}'.", null));
                }

                return new OkObjectResult(new BaseResponse(true, $"Lấy đánh giá cho cuộc hẹn ID '{appointmentId}' thành công.", reviews));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy đánh giá theo Appointment ID: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }


        public async Task<IActionResult> CreateReviewAsync(CreateReviewCourseReqModel request)
        {
            try
            {
                var hasPurchasedCourse = await _context.OrderLogs
                    .AnyAsync(ol => ol.UserId == request.UserId && 
                                    ol.CourseId == request.CourseId);

                if (!hasPurchasedCourse)
                {
                    return new BadRequestObjectResult(new BaseResponse(false, "Bạn chỉ có thể đánh giá các khóa học mà bạn đã mua thành công.", null));
                }

                var existingReview = await _context.Reviews
                    .AnyAsync(r => r.UserId == request.UserId && // Sử dụng UserId từ request
                                   r.CourseId == request.CourseId &&
                                   !r.IsDeleted);

                if (existingReview)
                {
                    return new BadRequestObjectResult(new BaseResponse(false, "Bạn đã đánh giá khóa học này rồi. Bạn chỉ có thể đánh giá một lần.", null));
                }

                // Bước 3: Tạo đánh giá
                var review = new Review
                {
                    Id = Guid.NewGuid(),
                    CourseId = request.CourseId,
                    UserId = request.UserId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                var reviewResModel = new ReviewResModel
                {
                    Id = review.Id,
                    CourseId = review.CourseId,
                    UserId = review.UserId, // Lấy UserId từ đối tượng review đã lưu
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                return new ObjectResult(new BaseResponse(true, "Tạo đánh giá thành công.", reviewResModel))
                { StatusCode = 201 }; // 201 Created
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi tạo đánh giá: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> CreateAppointmentReviewAsync(CreateAppointmentReviewReqModel request)
        {
            try
            {
                // 1. Kiểm tra xem người dùng có thực sự có cuộc hẹn đó và đã hoàn thành chưa
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == request.AppointmentId
                                           && a.UserId == request.UserId
                                           && a.Status == AppointmentStatus.Completed
                                           );

                if (appointment == null)
                {
                    return new BadRequestObjectResult(new BaseResponse(false, "Bạn chỉ có thể đánh giá cuộc hẹn đã hoàn thành.", null));
                }

                // 2. Kiểm tra đã đánh giá chưa
                var existingReview = await _context.Reviews
                    .AnyAsync(r => r.AppointmentId == request.AppointmentId
                                && r.UserId == request.UserId
                                && !r.IsDeleted);

                if (existingReview)
                {
                    return new BadRequestObjectResult(new BaseResponse(false, "Bạn đã đánh giá cuộc hẹn này rồi.", null));
                }

                // 3. Tạo review
                var review = new Review
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = request.AppointmentId,
                    UserId = request.UserId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                var response = new ReviewResModel
                {
                    Id = review.Id,
                    AppointmentId = review.AppointmentId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                return new ObjectResult(new BaseResponse(true, "Tạo đánh giá cho cuộc hẹn thành công.", response))
                {
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi tạo đánh giá cuộc hẹn: {ex.Message}", null))
                {
                    StatusCode = 500
                };
            }
        }


        public async Task<IActionResult> UpdateReviewAsync(Guid id, UpdateReviewReqModel request)
        {
            try
            {
                var review = await _context.Reviews
                                           .Where(r => !r.IsDeleted && r.Id == id)
                                           .FirstOrDefaultAsync();

                if (review == null)
                {
                    return new NotFoundObjectResult(new BaseResponse(false, "Không tìm thấy đánh giá để cập nhật hoặc đã bị xóa.", null));
                }

                review.Rating = request.Rating;
                review.Comment = request.Comment;
                review.UpdatedAt = DateTime.UtcNow;

                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new BaseResponse(true, "Cập nhật đánh giá thành công.", null));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi cập nhật đánh giá: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> DeleteReviewAsync(Guid id)
        {
            try
            {
                var review = await _context.Reviews
                                           .Where(r => !r.IsDeleted && r.Id == id)
                                           .FirstOrDefaultAsync();

                if (review == null)
                {
                    // Trả về 404 Not Found
                    return new NotFoundObjectResult(new BaseResponse(false, "Không tìm thấy đánh giá để xóa hoặc đã bị xóa.", null));
                }

                review.IsDeleted = true;
                review.UpdatedAt = DateTime.UtcNow;

                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();

                // Trả về 200 OK
                return new OkObjectResult(new BaseResponse(true, "Xóa đánh giá thành công.", null));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi xóa đánh giá: {ex.Message}", null))
                { StatusCode = 500 };
            }
        }
    }
}