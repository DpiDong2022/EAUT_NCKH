using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EAUT_NCKH.Web.Services {
    public class ReportService {
        private readonly EntityDataContext _context;
        private readonly ITopicRepository _topicRepository;

        public ReportService(EntityDataContext context, ITopicRepository topicRepository) {
            _context = context;
            _topicRepository = topicRepository;
        }

        public async Task<byte[]> GetTopicRegisterStudentList(TopicIndexViewPage options, int userId) {
            options.Pagination.PageNumber = 1;
            options.Pagination.PageLength = int.Parse((await _topicRepository.GetCountDataTable(options, userId)).ToString());
            var topicList = await _topicRepository.GetDataTable(options, userId);

            var keyValues = new Dictionary<string, string> {
                {"{DONVI}", "VIỆN ĐÀO TẠO VÀ HỢP TÁC QUỐC TẾ"},
                {"{YEAR}", DateTime.Now.ToString("yyyy")},
                {"day", DateTime.Now.ToString("dd")},
                {"month", DateTime.Now.ToString("MM")},
                {"year", DateTime.Now.ToString("yyyy")},
            };

            var dataList = new List<string[]>();
            int index = 1;

            foreach (var item in topicList) {
                var studentList = _context.Topicstudents
                                            .Include(c => c.StudentcodeNavigation)
                                            .Where(c => c.Topicid == item.Id)
                                            .OrderBy(c => c.Role);

                foreach (var student in studentList) {
                    dataList.Add(new string[]
                    {
                        index.ToString(),
                        item.Title,
                        student.StudentcodeNavigation.Fullname,
                        student.StudentcodeNavigation.Id,
                        student.StudentcodeNavigation.Classname,
                        student.Role ? "Trưởng nhóm" : "Thành viên",
                        item.CreatedbyNavigation.Fullname,
                        item.Secondteacher?.Fullname ?? "",
                        student.StudentcodeNavigation.Phonenumber,
                        student.StudentcodeNavigation.Email,
                        ""
                    });
                    index++;
                }
            }

            // Convert List<string[]> to string[,]
            string[,] studentDatas = new string[dataList.Count, 11];
            for (int i = 0; i < dataList.Count; i++) {
                for (int j = 0; j < 11; j++) {
                    studentDatas[i, j] = dataList[i][j];
                }
            }

            var data = await GenerateReport("Templates/danhsach_sinhvien_dangky_detai.docx", keyValues, studentDatas);
            return data;
        }
        private async Task<byte[]?> GenerateReport(string templatePath, Dictionary<string, string> keyValues, string[,] dataMatrix2 = null) {

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
        private void FillStudentTable(WordprocessingDocument doc, string[,] dataMatrix2) {
            var body = doc.MainDocumentPart.Document.Body;
            var table = body.Elements<Table>().ElementAtOrDefault(1);
            if (table == null)
                return;

            var rows = table.Elements<TableRow>().ToList();
            if (rows.Count < 2)
                return;

            var sampleRow = rows[2];

            int rowCount = dataMatrix2.GetLength(0);
            int colCount = dataMatrix2.GetLength(1);

            for (int i = 0; i < rowCount; i++) {
                var newRow = (TableRow)sampleRow.CloneNode(true);
                var cells = newRow.Elements<TableCell>().ToList();

                for (int j = 0; j < colCount && j < cells.Count; j++) {
                    string cellText = dataMatrix2[i, j];

                    var run = new Run(new Text(cellText));
                    var paragraph = new Paragraph(run);
                    var paragraphProps = new ParagraphProperties(
                new SpacingBetweenLines { Before = "60", After = "60" }
            );
                    paragraph.PrependChild(paragraphProps);

                    cells[j].RemoveAllChildren<Paragraph>();
                    cells[j].Append(paragraph);
                }

                table.AppendChild(newRow);
            }

            table.RemoveChild(sampleRow);
        }

    }
}
