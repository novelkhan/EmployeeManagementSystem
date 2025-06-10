using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("employee-report")]
        public IActionResult GetEmployeeReport()
        {
            var reportBytes = _reportService.GenerateEmployeeReport();
            return File(reportBytes, "application/pdf", "EmployeeReport.pdf");
        }
    }
}