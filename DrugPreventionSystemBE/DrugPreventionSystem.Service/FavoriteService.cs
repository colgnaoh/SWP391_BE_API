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

        public async Task<List<FavoriteItemResponseModel>> GetFavoritesAsync(Guid userId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .ToListAsync();

            var result = new List<FavoriteItemResponseModel>();

            foreach (var favorite in favorites)
            {
                if (favorite.TargetType == FavoriteType.Program)
                {
                    var program = await _context.Programs
                        .Where(p => p.Id == favorite.TargetId && !p.IsDeleted)
                        .Select(p => new FavoriteItemResponseModel
                        {
                            TargetId = p.Id,
                            Name = p.Name,
                            Type = "Program",
                            ImgUrl = p.ProgramImgUrl,
                            Description = p.Description
                        })
                        .FirstOrDefaultAsync();

                    if (program != null) result.Add(program);
                }
                else if (favorite.TargetType == FavoriteType.Course)
                {
                    var course = await _context.Courses
                        .Where(c => c.Id == favorite.TargetId && !c.IsDeleted)
                        .Select(c => new FavoriteItemResponseModel
                        {
                            TargetId = c.Id,
                            Name = c.Name,
                            Type = "Course",
                            ImgUrl = c.ImageUrlsJson,
                            Description = c.Content
                        })
                        .FirstOrDefaultAsync();

                    if (course != null) result.Add(course);
                }
            }

            return result;
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
