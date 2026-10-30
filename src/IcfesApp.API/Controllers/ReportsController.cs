using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/reports")]
[Authorize(Policy = Policies.RequireTeacher)]
public class ReportsController(IReportService reportService) : ApiControllerBase
{
    /// <summary>Performance of the students in the authenticated teacher's institution.</summary>
    [HttpGet("student-performance")]
    public async Task<IActionResult> GetStudentPerformance(CancellationToken cancellationToken)
        => Ok(await reportService.GetStudentsReportAsync(CurrentUserId, cancellationToken));
}
