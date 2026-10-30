using IcfesApp.Application.Auth;
using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Domain.Constants;
using IcfesApp.Domain.Entities;
using IcfesApp.Infrastructure.Email;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Security;

public class RegistrationService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IAuthEmailSender authEmailSender) : IRegistrationService
{
    public async Task<AuthResult> RegisterStudentAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        InvitationCode? invitationCode = null;
        if (!string.IsNullOrWhiteSpace(request.InvitationCode))
        {
            var invitationCodeResult = await FindUsableInvitationCodeAsync(request.InvitationCode, cancellationToken);
            if (!invitationCodeResult.Succeeded)
            {
                return AuthResult.Failed(invitationCodeResult.Errors);
            }

            invitationCode = invitationCodeResult.Value;
        }

        var student = ApplicationUserFactory.CreateSelfRegisteredStudent(
            request.Email, request.FirstName, request.LastName, invitationCode?.InstitutionId);

        // Creating the user and spending one use of the code go together: if anything fails, everything is rolled back.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var createUserResult = await userManager.CreateAsync(student, request.Password);
            if (!createUserResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AuthResult.Failed(createUserResult.ErrorDescriptions());
            }

            await userManager.AddToRoleAsync(student, Roles.Student);

            if (invitationCode is not null)
            {
                invitationCode.Uses++;
                invitationCode.UpdatedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        await authEmailSender.SendEmailConfirmationAsync(student, cancellationToken);
        return AuthResult.RegistrationPending();
    }

    public async Task<Result<UserInfoDto>> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Role != Roles.Teacher && request.Role != Roles.Admin)
        {
            return Result<UserInfoDto>.Failed([$"El rol debe ser '{Roles.Teacher}' o '{Roles.Admin}'."]);
        }

        var staffUser = ApplicationUserFactory.CreateStaff(request.Email, request.FirstName, request.LastName);

        var createUserResult = await userManager.CreateAsync(staffUser, request.Password);
        if (!createUserResult.Succeeded)
        {
            return Result<UserInfoDto>.Failed(createUserResult.ErrorDescriptions());
        }

        await userManager.AddToRoleAsync(staffUser, request.Role);
        return Result<UserInfoDto>.Success(staffUser.ToUserInfoDto([request.Role]));
    }

    private async Task<Result<InvitationCode>> FindUsableInvitationCodeAsync(string typedCode, CancellationToken cancellationToken)
    {
        var normalizedCode = typedCode.Trim().ToUpperInvariant();
        var invitationCode = await dbContext.InvitationCodes
            .FirstOrDefaultAsync(code => code.Code == normalizedCode, cancellationToken);

        if (invitationCode is null)
        {
            return Result<InvitationCode>.Failed(["El código de invitación no es válido."]);
        }

        if (!invitationCode.IsUsable)
        {
            return Result<InvitationCode>.Failed(["El código de invitación ya expiró o alcanzó su límite de usos."]);
        }

        return Result<InvitationCode>.Success(invitationCode);
    }
}
