using AspNetCore.Reporting;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

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
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

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

            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 20, 20, 20, 20);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Use default Helvetica font for English
                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font font = new Font(bf, 12);
                Font headerFont = new Font(bf, 12, Font.BOLD);

                Paragraph title = new Paragraph("Employee Report", new Font(bf, 20, Font.BOLD)); // Corrected spelling
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph("\n\n")); // Add two line breaks to move table down

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1f, 3f, 3f, 2f, 2f });
                table.DefaultCell.Padding = 10; // Add padding to cells
                table.HeaderRows = 1; // Ensure header row is distinct

                table.AddCell(new PdfPCell(new Phrase("ID", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                table.AddCell(new PdfPCell(new Phrase("Name", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                table.AddCell(new PdfPCell(new Phrase("Department", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                table.AddCell(new PdfPCell(new Phrase("Salary", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                table.AddCell(new PdfPCell(new Phrase("Joining Date", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });

                foreach (DataRow row in dt.Rows)
                {
                    string id = row["Id"].ToString();
                    string name = row["Name"].ToString();
                    string dept = row["Department"].ToString();
                    string salary = row["Salary"].ToString() + " USD";
                    string joinDate = Convert.ToDateTime(row["JoiningDate"]).ToString("dd/MM/yyyy");

                    table.AddCell(new PdfPCell(new Phrase(id, font)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                    table.AddCell(new PdfPCell(new Phrase(name, font)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                    table.AddCell(new PdfPCell(new Phrase(dept, font)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                    table.AddCell(new PdfPCell(new Phrase(salary, font)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                    table.AddCell(new PdfPCell(new Phrase(joinDate, font)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 10 });
                }

                document.Add(table);

                Paragraph footer = new Paragraph($"Generated on: {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}", font);
                footer.Alignment = Element.ALIGN_CENTER;
                document.Add(footer);

                document.Close();
                return ms.ToArray();
            }
        }
    }
}