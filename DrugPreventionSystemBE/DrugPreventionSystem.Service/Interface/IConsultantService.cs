using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IConsultantService
    {
        Task<ListConsultantResponse> GetAllConsultantsAsync();
        Task<IActionResult> GetConsultantByIdAsync(Guid id);
        Task<IActionResult> CreateConsultantAsync(ConsultantCreateRequest request);
        Task<IActionResult> UpdateConsultantAsync(Guid id, ConsultantUpdateRequest request);
        Task<IActionResult> DeleteConsultantAsync(Guid id);
        Task<bool> ConsultantExists(Guid id); 
    }
}
