using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteResModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IFavoriteService
    {
        Task<bool> AddToFavoriteAsync(Guid userId, AddFavoriteRequestModel model);
        Task<bool> RemoveFromFavoriteAsync(Guid userId, Guid targetId, FavoriteType targetType);
        Task<List<FavoriteItemResponseModel>> GetFavoritesAsync(Guid userId);
    }
}
