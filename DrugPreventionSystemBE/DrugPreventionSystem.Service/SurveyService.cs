using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
            EstimateTime = model.EstimateTime,
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
        survey.EstimateTime = model.EstimateTime;
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
            EstimateTime = survey.EstimateTime,
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
    Guid? userId,
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

        // If userId is not provided, return basic survey list
        if (userId == null || userId == Guid.Empty)
        {
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
                    EstimateTime = s.EstimateTime,
                    CreatedAt = s.CreatedAt,
                    IsCompleted = false // or you can use `null` if IsCompleted is nullable
                }).ToList()
            };

            return new OkObjectResult(result);
        }

        // If userId exists, fetch user role
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return new NotFoundObjectResult(new
            {
                Success = false,
                Message = "User not found"
            });
        }

        var role = user.Role.ToString().ToLower();

        if (role == "customer")
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
                    EstimateTime = s.EstimateTime,
                    CreatedAt = s.CreatedAt,
                    IsCompleted = completedSurveyIds.Contains(s.Id)
                }).ToList()
            };

            return new OkObjectResult(result);
        }

        // If not customer
        var generalResult = new SurveyPagedResultModelWithStatus
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
                EstimateTime = s.EstimateTime,
                CreatedAt = s.CreatedAt,
                IsCompleted = null 
            }).ToList()
        };

        return new OkObjectResult(generalResult);
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

    public async Task<SurveyResultDetailResponseModel?> GetSurveyResultAsync(Guid surveyResultId)
    {
        var surveyResult = await _context.SurveyResults
            .Where(sr => sr.Id == surveyResultId)
            .FirstOrDefaultAsync();

        if (surveyResult == null)
            return null;

        var answers = await _context.UserAnswerLogs
            .Where(log => log.SurveyResultId == surveyResultId)
            .Select(log => new UserAnswerDetail
            {
                QuestionId = log.QuestionId,
                AnswerOptionId = log.AnswerOptionId,
                QuestionContent = _context.Questions
                    .Where(q => q.Id == log.QuestionId)
                    .Select(q => q.QuestionContent)
                    .FirstOrDefault(),
                AnswerOptionContent = log.AnswerOptionId != null
                    ? _context.AnswerOptions
                        .Where(a => a.Id == log.AnswerOptionId)
                        .Select(a => a.OptionContent)
                        .FirstOrDefault()
                    : null,
                Score = log.Score
            }).ToListAsync();

        return new SurveyResultDetailResponseModel
        {
            SurveyResultId = surveyResult.Id,
            SurveyId = surveyResult.SurveyId,
            UserId = surveyResult.UserId,
            TotalScore = surveyResult.TotalScore,
            RiskLevel = surveyResult.RiskLevel,
            Answers = answers
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
