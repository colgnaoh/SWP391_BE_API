using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class SurveyService : ISurveyService
    {
        private readonly DrugPreventionDbContext _context;

        public SurveyService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<List<Survey>> GetAllSurveysAsync()
        {
            return await _context.Surveys.Where(s => !s.IsDeleted).ToListAsync();
        }

        public async Task<Guid> CreateSurveyAsync(SurveyCreateModel model)
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

            foreach (var questionModel in model.Questions)
            {
                var questionId = Guid.NewGuid();

                var question = new Question
                {
                    Id = questionId,
                    SurveyId = surveyId,
                    QuestionContent = questionModel.QuestionContent,
                    QuestionType = questionModel.QuestionType,
                    PositionOrder = questionModel.PositionOrder
                };
                _context.Questions.Add(question);

                foreach (var optionModel in questionModel.AnswerOptions ?? new List<AnswerOptionCreateModel>())
                {
                    var option = new AnswerOption
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = questionId,
                        OptionContent = optionModel.OptionContent,
                        Score = optionModel.Score,
                        PositionOrder = optionModel.PositionOrder
                    };
                    _context.AnswerOptions.Add(option);
                }
            }

            await _context.SaveChangesAsync();
            return surveyId;
        }


        public async Task<SurveyDetailModel> GetSurveyDetailAsync(Guid surveyId)
        {
            var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == surveyId);
            var questions = await _context.Questions
                .Where(q => q.SurveyId == surveyId)
                .OrderBy(q => q.PositionOrder)
                .ToListAsync();

            var questionModels = new List<QuestionModel>();

            foreach (var q in questions)
            {
                var options = await _context.AnswerOptions
                    .Where(o => o.QuestionId == q.Id)
                    .OrderBy(o => o.PositionOrder)
                    .ToListAsync();

                questionModels.Add(new QuestionModel
                {
                    Id = q.Id,
                    Content = q.QuestionContent,
                    QuestionType = q.QuestionType,
                    PositionOrder = q.PositionOrder,
                    Options = options.Select(o => new AnswerOptionModel
                    {
                        Id = o.Id,
                        Content = o.OptionContent,
                        Score = o.Score,
                        PositionOrder = o.PositionOrder
                    }).ToList()
                });
            }

            return new SurveyDetailModel
            {
                Id = survey.Id,
                Name = survey.Name,
                Description = survey.Description,
                Type = survey.Type,
                Questions = questionModels
            };
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
                    Score = score
                });
            }

            var riskLevel = GetRiskLevel(totalScore);
            //var programId = GetProgramId(model.SurveyId, riskLevel);

            var surveyResult = new SurveyResult
            {
                Id = surveyResultId,
                UserId = model.UserId,
                SurveyId = model.SurveyId,
                TotalScore = totalScore,
                RiskLevel = riskLevel,
                //ProgramId = programId,
                CompletedAt = DateTime.UtcNow
            };

            _context.SurveyResults.Add(surveyResult);
            await _context.SaveChangesAsync();

            return new SurveyResultResponseModel
            {
                TotalScore = totalScore,
                RiskLevel = riskLevel,
                //ProgramId = programId,
            };
        }

        private RiskLevel GetRiskLevel(int score)
        {
            return score switch
            {
                < 10 => RiskLevel.Low,
                < 20 => RiskLevel.Moderate,
                _ => RiskLevel.High
            };
        }

    }

}
