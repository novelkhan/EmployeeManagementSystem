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

                string fontPath = Path.Combine(_environment.WebRootPath, "fonts", "Ekusher-Alo.ttf");
                if (!File.Exists(fontPath))
                {
                    Console.WriteLine($"Font file not found at: {fontPath}");
                    throw new FileNotFoundException("Font file not found.", fontPath);
                }

                // Try loading the font with additional debugging
                BaseFont bf;
                try
                {
                    bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Console.WriteLine($"Font loaded successfully from: {fontPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading font from {fontPath}: {ex.Message}");
                    throw;
                }

                Font font = new Font(bf, 12);
                Font headerFont = new Font(bf, 12, Font.BOLD);

                Paragraph title = new Paragraph("কর্মচারী রিপোর্ট", new Font(bf, 20, Font.BOLD));
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1f, 3f, 3f, 2f, 2f });

                table.AddCell(new PdfPCell(new Phrase("ID", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("নাম", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("বিভাগ", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("বেতন", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("যোগদানের তারিখ", headerFont)) { BackgroundColor = new BaseColor(242, 242, 242), HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (DataRow row in dt.Rows)
                {
                    table.AddCell(new PdfPCell(new Phrase(row["Id"].ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(row["Name"].ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(row["Department"].ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(row["Salary"].ToString(), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(Convert.ToDateTime(row["JoiningDate"]).ToString("dd/MM/yyyy"), font)) { HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(table);

                Paragraph footer = new Paragraph($"জেনারেটেড হয়েছে: {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}", font);
                footer.Alignment = Element.ALIGN_CENTER;
                document.Add(footer);

                document.Close();
                return ms.ToArray();
            }
        }
    }
}