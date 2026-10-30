using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/reports")]
[Authorize(Policy = Policies.RequireTeacher)]
public class ReportsController(IReportService reportService) : ApiControllerBase
{
    /// <summary>Desempeño de los estudiantes de la institución del profesor autenticado.</summary>
    [HttpGet("students")]
    public async Task<IActionResult> GetStudentsReport(CancellationToken cancellationToken)
        => Ok(await reportService.GetStudentsReportAsync(CurrentUserId, cancellationToken));
}
