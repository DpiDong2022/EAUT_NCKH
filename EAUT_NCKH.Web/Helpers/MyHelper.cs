using EAUT_NCKH.Web.Enums;
using HashidsNet;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace EAUT_NCKH.Web.Helpers {
    public static class MyHelper {
        public static string GenerateRandomPassword(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";
            var random = new Random();
            var password = new StringBuilder();

            for (int i = 0; i < length; i++) {
                var index = random.Next(chars.Length);
                password.Append(chars[index]);
            }

            return password.ToString();
        }

        public static string FormatDateTimeVietnameseString(DateTime dateTime) {
            return $"{dateTime:HH} giờ {dateTime:mm} phút, ngày {dateTime:dd} tháng {dateTime:MM}, {dateTime:yyyy}";
        }

        public static string RendProposalStatus(int currentStage, int? status) {
            if (currentStage == -1)
                return "Đang xử lý";
            if (status == (int)ProposalStatusId.REJECTED) {
                return ProposalStatusEnums.REJECTED;
            }
            string result = "";
            if (status == (int)ProposalStatusId.NEED_REVISION) {
                result = ProposalStatusEnums.NEED_REVISION;
            } else if(status == (int)ProposalStatusId.APPROVED) {
                result = ProposalStatusEnums.APPROVED;
            }
            switch (currentStage) {
                case 0:
                    result += " - Cấp Khoa";
                    break;
            }

            return result;
        }

        public static string GetTimeAgo(DateTime dateTime) {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalSeconds < 60) {
                return "Vài giây trước";
            } else if (timeSpan.TotalMinutes < 60) {
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            } else if (timeSpan.TotalHours < 24) {
                return $"{(int)timeSpan.TotalHours} giờ trước";
            } else if (timeSpan.TotalDays < 30) {
                return $"{(int)timeSpan.TotalDays} ngày trước";
            } else if (timeSpan.TotalDays < 365) {
                return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";
            } else {
                return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
            }
        }

        public static bool ValidateZipContents(string zipPath, string[] allowedExtensions) {
            try {
                using (var archive = ZipFile.OpenRead(zipPath)) {
                    foreach (var entry in archive.Entries) {
                        // Ignore folders
                        if (string.IsNullOrWhiteSpace(entry.Name))
                            continue;

                        var ext = Path.GetExtension(entry.FullName).ToLowerInvariant();
                        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                            return false;
                    }
                }
                return true;
            } catch {
                return false;
            }
        }


    }
}
