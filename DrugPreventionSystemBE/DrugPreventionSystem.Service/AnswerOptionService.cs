using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class AnswerOptionService : IAnswerOptionService
    {
        private readonly DrugPreventionDbContext _context;

        public AnswerOptionService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreateAnswerOptionsAsync(MultipleAnswerOptionCreateModel model)
        {
            if (model.Options == null || !model.Options.Any())
            {
                return new BadRequestObjectResult("Danh sách đáp án không được để trống.");
            }

            var options = model.Options.Select(opt => new AnswerOption
            {
                Id = Guid.NewGuid(),
                QuestionId = model.QuestionId,
                OptionContent = opt.OptionContent,
                Score = opt.Score,
                PositionOrder = opt.PositionOrder
            }).ToList();

            _context.AnswerOptions.AddRange(options);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Tạo nhiều đáp án thành công.");
        }



        public async Task<IActionResult> BulkUpdateAnswerOptionsAsync(BulkUpdateAnswerOptionModel model)
        {
            var optionIds = model.Options
                .Where(o => o.Id != Guid.Empty)
                .Select(o => o.Id)
                .ToList();

            var existingOptions = await _context.AnswerOptions
                .Where(o => optionIds.Contains(o.Id) && !o.IsDeleted)
                .ToListAsync();

            foreach (var updateItem in model.Options)
            {
                // Update existing
                var option = existingOptions.FirstOrDefault(o => o.Id == updateItem.Id);
                if (option != null)
                {
                    option.OptionContent = updateItem.OptionContent;
                    option.Score = updateItem.Score;
                    option.PositionOrder = updateItem.PositionOrder;
                }
                else
                {
                    // Add new (Id should be empty or a new Guid)
                    var newOption = new AnswerOption
                    {
                        Id = updateItem.Id == Guid.Empty ? Guid.NewGuid() : updateItem.Id,
                        QuestionId = updateItem.QuestionId, // Ensure this is included in your request model
                        OptionContent = updateItem.OptionContent,
                        Score = updateItem.Score,
                        PositionOrder = updateItem.PositionOrder,
                        IsDeleted = false
                    };
                    _context.AnswerOptions.Add(newOption);
                }
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult("Cập nhật và thêm đáp án hàng loạt thành công.");
        }



        public async Task<IActionResult> DeleteAnswerOptionAsync(Guid id)
        {
            var option = await _context.AnswerOptions
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (option == null)
                return new NotFoundObjectResult("Đáp án không tồn tại.");

            option.IsDeleted = true;
            await _context.SaveChangesAsync();

            return new OkObjectResult("Xóa đáp án thành công.");
        }


        public async Task<List<AnswerOptionResponseModel>> GetAnswerOptionsByQuestionIdAsync(Guid questionId)
        {
            return await _context.AnswerOptions
                .Where(o => o.QuestionId == questionId && !o.IsDeleted)
                .OrderBy(o => o.PositionOrder)
                .Select(o => new AnswerOptionResponseModel
                {
                    Id = o.Id,
                    QuestionId = o.QuestionId,
                    OptionContent = o.OptionContent,
                    Score = o.Score,
                    PositionOrder = o.PositionOrder
                }).ToListAsync();
        }

        public async Task<IActionResult> GetAnswerOptionsByPageAsync(
    Guid? questionId,
    int pageNumber,
    int pageSize,
    string? filter,
    int? filterByScore)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.AnswerOptions
                .Where(a => !a.IsDeleted)
                .Include(a => a.Question)
                .AsQueryable();

            if (questionId.HasValue)
            {
                query = query.Where(a => a.QuestionId == questionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(a => EF.Functions.Like(a.OptionContent, $"%{filter}%"));
            }

            if (filterByScore.HasValue)
            {
                query = query.Where(a => a.Score == filterByScore.Value);
            }

            var totalCount = await query.CountAsync();

            var options = await query
                .OrderBy(a => a.PositionOrder)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            return new OkObjectResult(new GetAnswerOptionsByPageResponse
            {
                Success = true,
                Data = options.Select(a => new AnswerOptionResponseModel
                {
                    Id = a.Id,
                    QuestionId = a.QuestionId,
                    QuestionContent = a.Question != null ? a.Question.QuestionContent : "",
                    OptionContent = a.OptionContent,
                    Score = a.Score,
                    PositionOrder = a.PositionOrder
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / safePageSize)
            });
        }


    }

}
