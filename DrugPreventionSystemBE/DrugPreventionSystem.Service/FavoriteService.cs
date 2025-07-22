using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class FavoriteService : IFavoriteService
    {
        public Task<bool> AddToFavoriteAsync(Guid userId, AddFavoriteRequestModel model)
        {
            throw new NotImplementedException();
        }

        public Task<List<FavoriteItemResponseModel>> GetFavoritesAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveFromFavoriteAsync(Guid userId, Guid targetId, FavoriteType targetType)
        {
            throw new NotImplementedException();
        }
    }
}
