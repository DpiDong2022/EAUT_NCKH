using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace EAUT_NCKH.Web.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly EntityDataContext _context;

        public TopicRepository(EntityDataContext context) {
            _context = context;
        }

        public async Task<double> GetCountDataTable(TopicDataTableOptions options, int userId) {
            var senderAccount = _context.Accounts.FirstOrDefault(a => a.Id == userId);
            if (senderAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM
                || senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                options.DepartmentId = senderAccount.Departmentid ?? 0;
            }

            var topics = _context.Topics
                .Where(c =>
                    (options.DepartmentId == -1 || c.Departmentid == options.DepartmentId)
                    && (options.Year == -1 || c.Year == options.Year)
                    && (options.Status == -1 || c.Status == options.Status)
                    && (options.SubStatus == "-1" || c.Substatus == options.SubStatus)
                    && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search)));

            if (senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                topics = topics.Where(c => c.Createdby == senderAccount.Id
                || (c.Secondteacher != null && c.Secondteacher.Id == senderAccount.Id));
            }
            var count = await topics.CountAsync();
            return count;
        }

        public async Task<List<Topic>>? GetDataTable(TopicDataTableOptions options, int userId) {
            var senderAccount = _context.Accounts.FirstOrDefault(a => a.Id == userId);
            if (senderAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM
                || senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                options.DepartmentId = senderAccount.Departmentid ?? 0;
            }

            var topics = _context.Topics
                .Include(t => t.Department)
                .Include(t => t.CreatedbyNavigation)
                .Include(t => t.SubstatusNavigation)
                .Include(t => t.StatusNavigation)
                .Include(t => t.Defenseassignments)
                .Where(c =>
                    (options.DepartmentId == -1 || c.Departmentid == options.DepartmentId)
                    && (options.Year == -1 || c.Year == options.Year)
                    && (options.Status == -1 || c.Status == options.Status)
                    && (options.SubStatus == "-1" || c.Substatus == options.SubStatus)
                    && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search)))
                .OrderByDescending(t => t.Createddate)
                .AsQueryable();
            if (senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                topics = topics.Where(c => c.Createdby == senderAccount.Id 
                || (c.Secondteacher!=null && c.Secondteacher.Id == senderAccount.Id));
            }

            return await topics.Skip(options.GetStartRow())
                .Take(options.PageLength).ToListAsync();
        }

        // Implement methods for topic management here
    }
}
