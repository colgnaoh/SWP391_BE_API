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

        public async Task<IActionResult> CreateAnswerOptionAsync(AnswerOptionCreateModel model)
        {
            var answerOption = new AnswerOption
            {
                Id = Guid.NewGuid(),
                QuestionId = model.QuestionId,
                OptionContent = model.OptionContent,
                Score = model.Score,
                PositionOrder = model.PositionOrder
            };

            _context.AnswerOptions.Add(answerOption);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Tạo đáp án thành công.");
        }


        public async Task<IActionResult> UpdateAnswerOptionAsync(Guid id, AnswerOptionUpdateModel model)
        {
            var option = await _context.AnswerOptions.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
            if (option == null)
                return new NotFoundObjectResult("Đáp án không tồn tại.");

            option.OptionContent = model.OptionContent;
            option.Score = model.Score;
            option.PositionOrder = model.PositionOrder;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Cập nhật đáp án thành công.");
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

        public async Task<IActionResult> GetAnswerOptionsByPageAsync(Guid? questionId, int pageNumber, int pageSize, string? filter, int? filterByScore)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.AnswerOptions
                .Where(a => a.QuestionId == questionId && !a.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
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
