using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SurveyService : ISurveyService
{
    private readonly DrugPreventionDbContext _context;

    public SurveyService(DrugPreventionDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> CreateSurveyAsync(SurveyCreateModel model)
    {
        var surveyId = Guid.NewGuid();

        var survey = new Survey
        {
            Id = surveyId,
            Name = model.Name,
            Description = model.Description,
            Type = model.SurveyType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync();

        return new OkObjectResult("Tạo survey thành công.");
    }


    public async Task<IActionResult> UpdateSurveyAsync(Guid id, SurveyUpdateModel model)
    {
        var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (survey == null)
            return new NotFoundObjectResult("Survey không tồn tại.");

        survey.Name = model.Name;
        survey.Description = model.Description;
        survey.Type = model.SurveyType;
        survey.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OkObjectResult("Cập nhật survey thành công.");
    }

    public async Task<IActionResult> DeleteSurveyAsync(Guid id)
    {
        var survey = await _context.Surveys
            .Include(s => s.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (survey == null)
            return new NotFoundObjectResult("Survey không tồn tại.");

        survey.IsDeleted = true;

        foreach (var question in survey.Questions)
        {
            question.IsDeleted = true;

            foreach (var option in question.AnswerOptions)
            {
                option.IsDeleted = true;
            }
        }

        await _context.SaveChangesAsync();
        return new OkObjectResult("Xóa survey và các câu hỏi liên quan thành công.");
    }



    public async Task<SurveyDetailModel?> GetSurveyDetailAsync(Guid id)
    {
        var survey = await _context.Surveys
            .Include(s => s.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (survey == null) return null;

        return new SurveyDetailModel
        {
            Id = survey.Id,
            Name = survey.Name,
            Description = survey.Description,
            Type = survey.Type,
            CreatedAt = survey.CreatedAt,
            Questions = survey.Questions.OrderBy(q => q.PositionOrder).Select(q => new QuestionModel
            {
                Id = q.Id,
                QuestionContent = q.QuestionContent,
                QuestionType = q.QuestionType,
                PositionOrder = q.PositionOrder,
                AnswerOptions = q.AnswerOptions.OrderBy(a => a.PositionOrder).Select(a => new AnswerOptionResponseModel
                {
                    Id = a.Id,
                    OptionContent = a.OptionContent,
                    Score = a.Score,
                    PositionOrder = a.PositionOrder
                }).ToList()
            }).ToList()
        };
    }

    public async Task<IActionResult> GetSurveysByPageWithStatusAsync(
        Guid userId,
        string? role,
        int pageNumber,
        int pageSize,
        string? filterByName)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize < 1 ? 10 : pageSize;

        var query = _context.Surveys
            .Where(s => !s.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filterByName))
        {
            query = query.Where(s => s.Name.Contains(filterByName));
        }

        var totalCount = await query.CountAsync();

        var surveys = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

        // For customers: check if user completed the surveys
        if (role?.ToLower() == "customer")
        {
            var surveyIds = surveys.Select(s => s.Id).ToList();

            var completedSurveyIds = await _context.SurveyResults
                .Where(r => r.UserId == userId && surveyIds.Contains(r.SurveyId))
                .Select(r => r.SurveyId)
                .Distinct()
                .ToListAsync();

            var result = new SurveyPagedResultModelWithStatus
            {
                Success = true,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Data = surveys.Select(s => new SurveyResponseModelWithStatus
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    SurveyType = s.Type,
                    CreatedAt = s.CreatedAt,
                    IsCompleted = completedSurveyIds.Contains(s.Id)
                }).ToList()
            };

            return new OkObjectResult(result);
        }

        // Other roles: return without IsCompleted
        var adminResult = new SurveyPagedResultModel
        {
            Success = true,
            PageNumber = safePageNumber,
            PageSize = safePageSize,
            TotalPages = totalPages,
            TotalCount = totalCount,
            Data = surveys.Select(s => new SurveyResponseModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                SurveyType = s.Type,
                CreatedAt = s.CreatedAt
            }).ToList()
        };

        return new OkObjectResult(adminResult);
    }


    public async Task<SurveyResultResponseModel> SubmitSurveyAsync(SurveySubmitRequestModel model)
    {
        var totalScore = 0;
        var surveyResultId = Guid.NewGuid();

        foreach (var answer in model.Answers)
        {
            var option = answer.AnswerOptionId.HasValue
                ? await _context.AnswerOptions.FirstOrDefaultAsync(x => x.Id == answer.AnswerOptionId)
                : null;

            var score = option?.Score ?? 0;
            totalScore += score;

            _context.UserAnswerLogs.Add(new UserAnswerLog
            {
                Id = Guid.NewGuid(),
                SurveyResultId = surveyResultId,
                QuestionId = answer.QuestionId,
                AnswerOptionId = answer.AnswerOptionId,
                Score = score,
                UserId = model.UserId
            });
        }

        var riskLevel = GetRiskLevel(totalScore);

        var surveyResult = new SurveyResult
        {
            Id = surveyResultId,
            UserId = model.UserId,
            SurveyId = model.SurveyId,
            TotalScore = totalScore,
            RiskLevel = riskLevel,
            CompletedAt = DateTime.UtcNow
        };

        _context.SurveyResults.Add(surveyResult);
        await _context.SaveChangesAsync();

        return new SurveyResultResponseModel
        {
            TotalScore = totalScore,
            RiskLevel = riskLevel
        };
    }

    private RiskLevel GetRiskLevel(int score)
    {
        return score switch
        {
            < 10 => RiskLevel.Low,
            < 20 => RiskLevel.Medium,
            _ => RiskLevel.High
        };
    }
}
