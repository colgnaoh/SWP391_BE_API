using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SessionReqModel;
using Microsoft.AspNetCore.Mvc;

public interface ISessionService
{
    Task<IActionResult> CreateAsync(SessionCreateModelView request);
    Task<IActionResult> GetAllAsync(string? name, Guid? courseId, int pageNumber = 1, int pageSize = 12);
    Task<IActionResult> GetByIdAsync(Guid id);
    Task<IActionResult> GetSessionByPageAsync(Guid sessionId, int pageNumber = 1, int pageSize = 12);
    Task<IActionResult> UpdateAsync(Guid id, SessionUpdateModelView request);
    Task<IActionResult> SoftDeleteAsync(Guid id);
}
