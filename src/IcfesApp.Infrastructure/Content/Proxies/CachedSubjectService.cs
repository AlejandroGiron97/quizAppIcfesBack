using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Subjects.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace IcfesApp.Infrastructure.Content.Proxies;

/// <summary>
/// Caching proxy (Proxy pattern) in front of the real <see cref="SubjectService"/>.
///
/// Subjects are read on almost every screen (dashboard, practice, admin) and change very rarely,
/// so reads are served from memory. Any write (create / update / delete) clears the cache,
/// so the next read always sees fresh data.
///
/// It implements the same <see cref="ISubjectService"/> interface: controllers do not know it exists.
/// Note: the cache lives in the memory of each API instance.
/// </summary>
public class CachedSubjectService(ISubjectService realSubjectService, IMemoryCache memoryCache) : ISubjectService
{
    private const string AllSubjectsCacheKey = "subjects:all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<IReadOnlyList<SubjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cachedSubjects = await memoryCache.GetOrCreateAsync(AllSubjectsCacheKey, cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return realSubjectService.GetAllAsync(cancellationToken);
        });

        return cachedSubjects ?? [];
    }

    /// <summary>Served from the cached list, so no extra cache entry per subject is needed.</summary>
    public async Task<SubjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var allSubjects = await GetAllAsync(cancellationToken);
        return allSubjects.FirstOrDefault(subject => subject.Id == id);
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var createdSubject = await realSubjectService.CreateAsync(request, cancellationToken);
        ClearCache();
        return createdSubject;
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var updateResult = await realSubjectService.UpdateAsync(id, request, cancellationToken);
        ClearCache();
        return updateResult;
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleteResult = await realSubjectService.DeleteAsync(id, cancellationToken);
        ClearCache();
        return deleteResult;
    }

    private void ClearCache() => memoryCache.Remove(AllSubjectsCacheKey);
}
