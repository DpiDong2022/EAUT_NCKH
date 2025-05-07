using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace EAUT_NCKH.Web.Repositories {
    public class StudentRepository: IStudentRepository {
        private readonly EntityDataContext _context;

        public StudentRepository(EntityDataContext context) {
            _context = context;
        }

        public async Task<double> GetCountDataTable(StudentIndexViewPage options, int senderId) {
            return await _context.Students.Where(c => (options.MajorId == -1 || options.MajorId == c.Majorid)
                && (options.TrainingProgramId == -1 || options.TrainingProgramId == c.Trainingprogramid)
                && (options.DepartmentId == -1 || options.DepartmentId == c.Departmentid)
                && (string.IsNullOrEmpty(options.Search)
                || EF.Functions.FreeText(c.Id, options.Search)
                || EF.Functions.FreeText(c.Email, options.Search)
                || EF.Functions.FreeText(c.Phonenumber, options.Search)
                || EF.Functions.FreeText(c.Classname, options.Search)
                || EF.Functions.FreeText(c.Fullname, options.Search))).CountAsync();
        }

        public async Task<List<Student>> GetDataTable(StudentIndexViewPage options, int senderId) {
            return await _context.Students
                .Include(c => c.Trainingprogram)
                .Include(c => c.Major)
                .Include(c => c.Department)
                .Where(c => (options.MajorId == -1 || options.MajorId == c.Majorid)
                && (options.TrainingProgramId == -1 || options.TrainingProgramId == c.Trainingprogramid)
                && (options.DepartmentId == -1 || options.DepartmentId == c.Departmentid)
                && (string.IsNullOrEmpty(options.Search)
                || EF.Functions.FreeText(c.Id, options.Search)
                || EF.Functions.FreeText(c.Email, options.Search)
                || EF.Functions.FreeText(c.Phonenumber, options.Search)
                || EF.Functions.FreeText(c.Classname, options.Search)
                || EF.Functions.FreeText(c.Fullname, options.Search)))
                .OrderByDescending(c => c.Createddate)
                .Skip(options.Pagination.GetStartRow())
                .Take(options.Pagination.PageLength)
                .ToListAsync();
        }
    }
}
