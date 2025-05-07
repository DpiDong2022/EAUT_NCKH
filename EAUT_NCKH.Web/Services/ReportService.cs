using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;
using System.IO;

namespace EAUT_NCKH.Web.Services {
    public class ReportService {
        private readonly EntityDataContext _context;

        public ReportService(EntityDataContext context) {
            _context = context;
        }

        public async Task<byte[]> GetTopicRegisterStudentList(TopicIndexViewPage options = null) {
            var keyValues = new Dictionary<string, string>{
                {"{DONVI}", "VIỆN ĐÀO TẠO VÀ HỢP TÁC QUỐC TẾ"},
                {"{YEAR}", DateTime.Now.ToString("yyyy") },
                {"day", DateTime.Now.ToString("dd") },
                {"month", DateTime.Now.ToString("MM") },
                {"year", DateTime.Now.ToString("yyyy") },
            };

            string[][] studentData = new[]
            {
                new[]{ "1", "Chuyển đổi số trong Marketing: Nghiên cứu trường hợp một thương hiệu bán lẻ tại Việt Nam", "Huỳnh Gia Dũng", "20215405", "DC.MKTEN.13.1", "Trưởng nhóm", "TS. Gian vien hai", "GS. Hồ Thị Lan", "0550004027", "huynhgiadung20215405@gmail.com", "" },
                new[] { "2", "Topic B", "Alice Nguyen", "654321", "CSE102", "Member", "Dr. Max", "", "0987654321", "alice@example.com", "" }
            };

            var data = await GenerateReport("Templates/danhsach_sinhvien_dangky_detai.docx", keyValues, studentData);
            return data;
        }
        private async Task<byte[]?> GenerateReport(string templatePath, Dictionary<string, string> keyValues, string[][] dataMatrix2 = null) {

            try {
                using var memoryStream = new MemoryStream();
                using (var fileStream = File.OpenRead(templatePath)) {
                    fileStream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                using (var wordDoc = WordprocessingDocument.Open(memoryStream, true)) {
                    ReplacePlaceholders(wordDoc, keyValues);

                    // Fill table
                    if (keyValues != null && keyValues.Count > 0) {
                        FillStudentTable(wordDoc, dataMatrix2);
                    }
                    // Save is optional; Dispose() already saves changes
                }

                // At this point, memoryStream contains the updated file
                return memoryStream.ToArray();
            } catch (Exception ex) {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error generating report: {ex.Message}");
                return null;
            }
        }
        private void ReplacePlaceholders(WordprocessingDocument doc, Dictionary<string, string> keyValues) {
            foreach (var text in doc.MainDocumentPart.Document.Descendants<Text>()) {
                foreach (var kv in keyValues) {
                    if (!string.IsNullOrEmpty(text.Text) && text.Text.Contains(kv.Key) ) {
                        text.Text = text.Text.Replace(kv.Key, kv.Value);
                    }
                }
            }
        }
        private void FillStudentTable(WordprocessingDocument doc, string[][] dataMatrix2) {
            var body = doc.MainDocumentPart.Document.Body;
            var table = body.Elements<Table>().ElementAt(1);
            if (table == null)
                return;

            var rows = table.Elements<TableRow>().ToList();
            if (rows.Count < 2)
                return;

            var sampleRow = rows[2]; // Use second row as the sample to clone

            for (int i = 0; i < dataMatrix2.Length; i++) {
                var rowData = dataMatrix2[i];
                var newRow = (TableRow)sampleRow.CloneNode(true);
                var cells = newRow.Elements<TableCell>().ToList();

                for (int j = 0; j < cells.Count && j < rowData.Length; j++) {
                    // Tạo nội dung văn bản
                    var run = new Run(new Text(j == 0 ? (i + 1).ToString() : rowData[j]));

                    // Tạo Paragraph và thêm spacing
                    var paragraph = new Paragraph(run);
                    var paragraphProps = new ParagraphProperties();
                    paragraphProps.Append(new SpacingBetweenLines {
                        Before = "60", // 3pt
                        After = "60"   // 3pt
                    });
                    paragraph.PrependChild(paragraphProps);

                    // Gán vào ô
                    cells[j].RemoveAllChildren<Paragraph>();
                    cells[j].Append(paragraph);
                }

                table.AppendChild(newRow);
            }

            table.RemoveChild(sampleRow); // Clean up the sample/template row
        }
    }
}
