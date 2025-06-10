using AspNetCore.Reporting;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagementSystem.Services
{
    public interface IReportService
    {
        byte[] GenerateEmployeeReport();
    }

    public class ReportService : IReportService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ReportService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public byte[] GenerateEmployeeReport()
        {
            // RDLC ফাইলের পাথ
            string reportPath = Path.Combine(_environment.ContentRootPath, "Reports", "EmployeeReport.rdlc");

            // connectionString appsettings.json থেকে নেওয়া
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // ডাটা ফেচ করা
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Employees";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            // RDLC রিপোর্ট জেনারেট
            LocalReport localReport = new LocalReport(reportPath);
            localReport.AddDataSource("EmployeeDataSet", dt);

            var result = localReport.Execute(RenderType.Pdf, 1, null, "");
            return result.MainStream;
        }
    }
}