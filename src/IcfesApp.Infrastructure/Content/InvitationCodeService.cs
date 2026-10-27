using System.Security.Cryptography;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.InvitationCodes.Dtos;
using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class InvitationCodeService(IApplicationDbContext dbContext) : IInvitationCodeService
{
    // Alfabeto sin caracteres confundibles (sin I, O, 0, 1)
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public async Task<IReadOnlyList<InvitationCodeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var codes = await dbContext.InvitationCodes
            .Include(ic => ic.Institution)
            .OrderByDescending(ic => ic.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return codes.Select(MapToDto).ToList();
    }

    public async Task<InvitationCodeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var code = await dbContext.InvitationCodes
            .Include(ic => ic.Institution)
            .FirstOrDefaultAsync(ic => ic.Id == id, cancellationToken);

        return code is null ? null : MapToDto(code);
    }

    public async Task<Result<InvitationCodeDto>> CreateAsync(
        CreateInvitationCodeRequest request,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var institution = await dbContext.Schools
            .FirstOrDefaultAsync(s => s.Id == request.InstitutionId, cancellationToken);

        if (institution is null)
        {
            return Result<InvitationCodeDto>.Failed(["La institución especificada no existe."]);
        }

        if (request.MaxUses != -1 && request.MaxUses < 1)
        {
            return Result<InvitationCodeDto>.Failed(["MaxUses debe ser -1 (ilimitado) o mayor a 0."]);
        }

        if (request.ExpiresAtUtc.HasValue && request.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result<InvitationCodeDto>.Failed(["La fecha de expiración debe ser futura."]);
        }

        var code = string.IsNullOrWhiteSpace(request.Code)
            ? GenerateCode(institution.Name)
            : request.Code.Trim().ToUpperInvariant();

        var alreadyExists = await dbContext.InvitationCodes
            .AnyAsync(ic => ic.Code == code, cancellationToken);

        if (alreadyExists)
        {
            return Result<InvitationCodeDto>.Failed(["Ya existe un código con ese valor."]);
        }

        var invitationCode = new InvitationCode
        {
            Code = code,
            InstitutionId = request.InstitutionId,
            MaxUses = request.MaxUses,
            Uses = 0,
            ExpiresAtUtc = request.ExpiresAtUtc,
            IsActive = true,
            CreatedByUserId = createdByUserId
        };

        dbContext.InvitationCodes.Add(invitationCode);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Recargar con la institución para el DTO
        invitationCode.Institution = institution;

        return Result<InvitationCodeDto>.Success(MapToDto(invitationCode));
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateInvitationCodeRequest request, CancellationToken cancellationToken = default)
    {
        var code = await dbContext.InvitationCodes
            .FirstOrDefaultAsync(ic => ic.Id == id, cancellationToken);

        if (code is null)
        {
            return Result.NotFound();
        }

        if (request.MaxUses != -1 && request.MaxUses < code.Uses)
        {
            return Result.Failed([$"MaxUses no puede ser menor a los usos ya realizados ({code.Uses})."]);
        }

        if (request.ExpiresAtUtc.HasValue && request.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result.Failed(["La fecha de expiración debe ser futura."]);
        }

        code.MaxUses = request.MaxUses;
        code.ExpiresAtUtc = request.ExpiresAtUtc;
        code.IsActive = request.IsActive;
        code.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RevokeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var code = await dbContext.InvitationCodes
            .FirstOrDefaultAsync(ic => ic.Id == id, cancellationToken);

        if (code is null)
        {
            return Result.NotFound();
        }

        code.IsActive = false;
        code.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var code = await dbContext.InvitationCodes
            .FirstOrDefaultAsync(ic => ic.Id == id, cancellationToken);

        if (code is null)
        {
            return Result.NotFound();
        }

        dbContext.InvitationCodes.Remove(code);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string GenerateCode(string institutionName)
    {
        // Prefijo: primeras letras del nombre de la institución (solo letras, hasta 6)
        var prefix = new string(institutionName
            .Where(char.IsLetter)
            .Take(6)
            .ToArray())
            .ToUpperInvariant();

        if (prefix.Length < 3)
        {
            prefix = "CODE";
        }

        // Sufijo: 6 caracteres aleatorios criptográficamente seguros
        var suffix = new char[6];
        for (int i = 0; i < suffix.Length; i++)
        {
            suffix[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        }

        return $"{prefix}-{new string(suffix)}";
    }

    private static InvitationCodeDto MapToDto(InvitationCode code) => new()
    {
        Id = code.Id,
        Code = code.Code,
        InstitutionId = code.InstitutionId,
        InstitutionName = code.Institution?.Name ?? string.Empty,
        MaxUses = code.MaxUses,
        Uses = code.Uses,
        ExpiresAtUtc = code.ExpiresAtUtc,
        IsActive = code.IsActive,
        IsUsable = code.IsActive
            && (code.MaxUses < 0 || code.Uses < code.MaxUses)
            && (code.ExpiresAtUtc == null || code.ExpiresAtUtc > DateTime.UtcNow),
        CreatedAtUtc = code.CreatedAtUtc,
        UpdatedAtUtc = code.UpdatedAtUtc
    };
}