using System.Security.Claims;
using IcfesApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Teacher,Admin")] // Solo profesores y admins
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("students")]
    public async Task<IActionResult> GetStudentsReport(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var report = await reportService.GetStudentsReportAsync(userId, cancellationToken);
        return Ok(report);
    }
}