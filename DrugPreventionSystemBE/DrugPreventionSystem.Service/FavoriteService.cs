using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AddFavoriteResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class FavoriteService : IFavoriteService
    {
        private readonly DrugPreventionDbContext _context;

        public FavoriteService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddToFavoriteAsync(Guid userId, AddFavoriteRequestModel model)
        {
            var exists = await _context.Favorites.AnyAsync(f =>
                f.UserId == userId && f.TargetId == model.TargetId &&
                f.TargetType == model.TargetType && !f.IsDeleted);

            if (exists) return false;

            var favorite = new Favorite
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TargetId = model.TargetId,
                TargetType = model.TargetType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return true;
        }

        public Task<List<FavoriteItemResponseModel>> GetFavoritesAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveFromFavoriteAsync(Guid userId, Guid targetId, FavoriteType targetType)
        {
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f =>
                f.UserId == userId && f.TargetId == targetId &&
                f.TargetType == targetType && !f.IsDeleted);

            if (favorite == null) return false;

            favorite.IsDeleted = true;
            favorite.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
