using System.Security.Cryptography;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.InvitationCodes;
using IcfesApp.Application.InvitationCodes.Dtos;
using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class InvitationCodeService(IApplicationDbContext dbContext) : IInvitationCodeService
{
    /// <summary>Alphabet without characters that are easy to confuse when read (no I, O, 0, 1).</summary>
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int RandomSuffixLength = 6;
    private const int MaxPrefixLength = 6;
    private const int MinPrefixLength = 3;
    private const string FallbackPrefix = "CODE";
    private const int UnlimitedUses = -1;

    public async Task<IReadOnlyList<InvitationCodeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invitationCodes = await dbContext.InvitationCodes
            .AsNoTracking()
            .Include(invitationCode => invitationCode.Institution)
            .OrderByDescending(invitationCode => invitationCode.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return invitationCodes.Select(invitationCode => invitationCode.ToDto()).ToList();
    }

    public async Task<Result<InvitationCodeDto>> CreateAsync(
        CreateInvitationCodeRequest request,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var institution = await dbContext.Schools
            .FirstOrDefaultAsync(school => school.Id == request.InstitutionId, cancellationToken);

        if (institution is null)
        {
            return Result<InvitationCodeDto>.Failed(["La institución especificada no existe."]);
        }

        if (request.MaxUses != UnlimitedUses && request.MaxUses < 1)
        {
            return Result<InvitationCodeDto>.Failed(["MaxUses debe ser -1 (ilimitado) o mayor a 0."]);
        }

        if (request.ExpiresAtUtc.HasValue && request.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result<InvitationCodeDto>.Failed(["La fecha de expiración debe ser futura."]);
        }

        var codeValue = string.IsNullOrWhiteSpace(request.Code)
            ? GenerateCodeValue(institution.Name)
            : request.Code.Trim().ToUpperInvariant();

        var codeValueAlreadyExists = await dbContext.InvitationCodes
            .AnyAsync(invitationCode => invitationCode.Code == codeValue, cancellationToken);

        if (codeValueAlreadyExists)
        {
            return Result<InvitationCodeDto>.Failed(["Ya existe un código con ese valor."]);
        }

        var newInvitationCode = new InvitationCode
        {
            Code = codeValue,
            InstitutionId = request.InstitutionId,
            Institution = institution,
            MaxUses = request.MaxUses,
            Uses = 0,
            ExpiresAtUtc = request.ExpiresAtUtc,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };

        dbContext.InvitationCodes.Add(newInvitationCode);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<InvitationCodeDto>.Success(newInvitationCode.ToDto());
    }

    public async Task<Result<InvitationCodeDto>> UpdateAsync(Guid id, UpdateInvitationCodeRequest request, CancellationToken cancellationToken = default)
    {
        var invitationCode = await dbContext.InvitationCodes
            .Include(invitationCode => invitationCode.Institution)
            .FirstOrDefaultAsync(invitationCode => invitationCode.Id == id, cancellationToken);

        if (invitationCode is null)
        {
            return Result<InvitationCodeDto>.NotFound();
        }

        if (request.MaxUses != UnlimitedUses && request.MaxUses < invitationCode.Uses)
        {
            return Result<InvitationCodeDto>.Failed([$"MaxUses no puede ser menor a los usos ya realizados ({invitationCode.Uses})."]);
        }

        // Only a NEW expiration date must be in the future. Keeping the current one is always allowed,
        // so an already expired code can still be revoked (IsActive = false) without touching its date.
        var expirationChanged = request.ExpiresAtUtc != invitationCode.ExpiresAtUtc;
        if (expirationChanged && request.ExpiresAtUtc.HasValue && request.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result<InvitationCodeDto>.Failed(["La fecha de expiración debe ser futura."]);
        }

        invitationCode.MaxUses = request.MaxUses;
        invitationCode.ExpiresAtUtc = request.ExpiresAtUtc;
        invitationCode.IsActive = request.IsActive;
        invitationCode.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<InvitationCodeDto>.Success(invitationCode.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invitationCode = await dbContext.InvitationCodes
            .FirstOrDefaultAsync(invitationCode => invitationCode.Id == id, cancellationToken);

        if (invitationCode is null)
        {
            return Result.NotFound();
        }

        dbContext.InvitationCodes.Remove(invitationCode);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Generates a readable code: a prefix with the first letters of the institution
    /// and a cryptographically secure random suffix. E.g. "COLEGI-7KQ2MX".
    /// </summary>
    private static string GenerateCodeValue(string institutionName)
    {
        var prefix = new string(institutionName
                .Where(char.IsLetter)
                .Take(MaxPrefixLength)
                .ToArray())
            .ToUpperInvariant();

        if (prefix.Length < MinPrefixLength)
        {
            prefix = FallbackPrefix;
        }

        var randomSuffix = new char[RandomSuffixLength];
        for (var characterIndex = 0; characterIndex < randomSuffix.Length; characterIndex++)
        {
            randomSuffix[characterIndex] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        }

        return $"{prefix}-{new string(randomSuffix)}";
    }
}
