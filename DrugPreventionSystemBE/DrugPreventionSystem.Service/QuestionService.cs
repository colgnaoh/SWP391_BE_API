using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class QuestionService : IQuestionService
    {
        private readonly DrugPreventionDbContext _context;

        public QuestionService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreateQuestionAsync(QuestionCreateModel model)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                SurveyId = model.SurveyId,
                QuestionContent = model.QuestionContent,
                QuestionType = model.QuestionType,
                PositionOrder = model.PositionOrder
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Tạo câu hỏi thành công.");
        }


        public async Task<IActionResult> UpdateQuestionAsync(Guid id, QuestionUpdateModel model)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
            if (question == null)
                return new NotFoundObjectResult("Câu hỏi không tồn tại.");

            question.QuestionContent = model.QuestionContent;
            question.QuestionType = model.QuestionType;
            question.PositionOrder = model.PositionOrder;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Cập nhật câu hỏi thành công.");
        }

        public async Task<IActionResult> DeleteQuestionAsync(Guid id)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

            if (question == null)
                return new NotFoundObjectResult("Câu hỏi không tồn tại.");

            question.IsDeleted = true;

            foreach (var option in question.AnswerOptions)
            {
                option.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult("Xóa câu hỏi và các đáp án liên quan thành công.");
        }


        public async Task<List<QuestionResponseModel>> GetQuestionsBySurveyIdAsync(Guid surveyId)
        {
            return await _context.Questions
                .Where(q => q.SurveyId == surveyId && !q.IsDeleted)
                .OrderBy(q => q.PositionOrder)
                .Select(q => new QuestionResponseModel
                {
                    Id = q.Id,
                    SurveyId = q.SurveyId,
                    QuestionContent = q.QuestionContent,
                    QuestionType = q.QuestionType,
                    PositionOrder = q.PositionOrder
                }).ToListAsync();
        }

        public async Task<IActionResult> GetQuestionsByPageAsync(Guid? surveyId, int pageNumber, int pageSize, string? filter)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Questions
                .Where(q => q.SurveyId == surveyId && !q.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(q => EF.Functions.Like(q.QuestionContent, $"%{filter}%"));
            }

            var totalCount = await query.CountAsync();

            var questions = await query
                .OrderBy(q => q.PositionOrder)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            return new OkObjectResult(new GetQuestionsByPageResponse
            {
                Success = true,
                Data = questions.Select(q => new QuestionResponseModel
                {
                    Id = q.Id,
                    SurveyId = q.SurveyId,
                    QuestionContent = q.QuestionContent,
                    QuestionType = q.QuestionType,
                    PositionOrder = q.PositionOrder
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / safePageSize)
            });
        }

    }

}
