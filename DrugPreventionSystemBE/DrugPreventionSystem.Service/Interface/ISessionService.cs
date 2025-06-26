using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SessionReqModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ISessionService
    {
        Task<IEnumerable<SessionViewModel>> GetAllAsync();
        Task<SessionSingleResponse?> GetByIdAsync(Guid id);
        Task<SessionViewModel> CreateAsync(SessionCreateModelView request);
        Task<bool> UpdateAsync(Guid id, SessionUpdateModelView request);
        Task<bool> DeleteAsync(Guid id);
    }

}
