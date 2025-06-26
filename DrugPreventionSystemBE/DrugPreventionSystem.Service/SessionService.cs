using CloudinaryDotNet;
using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SessionReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class SessionService : ISessionService
    {
        private readonly DrugPreventionDbContext _context;

        public SessionService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SessionViewModel>> GetAllAsync()
        {
            return await _context.Sessions
                .AsNoTracking()
                .Select(s => new SessionViewModel
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    Name = s.Name,
                    UserId = s.UserId,
                    Slug = s.Slug,
                    Content = s.Content,
                    PositionOrder = s.PositionOrder
                }).ToListAsync();
        }

        public async Task<SessionSingleResponse?> GetByIdAsync(Guid id)
        {
            var s = await _context.Sessions.FindAsync(id);
            if (s == null) return null;

            return new SessionSingleResponse
            {
                Success = true,
                Data = new SessionViewModel
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    Name = s.Name,
                    UserId = s.UserId,
                    Slug = s.Slug,
                    Content = s.Content,
                    PositionOrder = s.PositionOrder
                }
            };
        }

        public async Task<SessionViewModel> CreateAsync(SessionCreateModelView request)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                CourseId = request.CourseId,
                Name = request.Name,
                UserId = request.UserId,
                Slug = request.Slug,
                Content = request.Content,
                PositionOrder = request.PositionOrder,
                CreatedAt = DateTime.UtcNow
            };
            var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? SlugGeneratorHelper.GenerateSlug(request.Name)
                : SlugGeneratorHelper.GenerateSlug(request.Slug);

            session.Slug = await GenerateUniqueSlugAsync(baseSlug);

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return new SessionViewModel
            {
                Id = session.Id,
                CourseId = session.CourseId,
                Name = session.Name,
                UserId = session.UserId,
                Slug = session.Slug,
                Content = session.Content,
                PositionOrder = session.PositionOrder
            };
        }

        public async Task<bool> UpdateAsync(Guid id, SessionUpdateModelView request)
        {
            var s = await _context.Sessions.FindAsync(id);
            if (s == null) return false;

            s.Name = request.Name ?? s.Name;
            s.Slug = request.Slug ?? s.Slug;
            s.Content = request.Content ?? s.Content;
            s.PositionOrder = request.PositionOrder ?? s.PositionOrder;
            s.UpdatedAt = DateTime.UtcNow;

            _context.Sessions.Update(s);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var s = await _context.Sessions.FindAsync(id);
            if (s == null || s.IsDeleted) return false;

            s.IsDeleted = true;
            s.UpdatedAt = DateTime.UtcNow;
            _context.Sessions.Update(s);
            await _context.SaveChangesAsync();
            return true;
        }


        private async Task<string> GenerateUniqueSlugAsync(string baseSlug, Guid? excludeId = null)
        {
            var slug = baseSlug;
            var suffix = 1;

            while (await _context.Courses.AnyAsync(c =>
                c.Slug == slug &&
                !c.IsDeleted &&
                (excludeId == null || c.Id != excludeId)))
            {
                slug = $"{baseSlug}-{suffix++}";
            }

            return slug;
        }
    }

}
