using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Runtime.InteropServices;

namespace EAUT_NCKH.Web.Repositories {
    public class CategoryRepository<T>: ICategoryRepository<T> where T : class{
        private readonly EntityDataContext _context;
        public CategoryRepository(EntityDataContext eautNckhContext) {
            _context = eautNckhContext;
        }

        public async Task<List<Department>> GetDepartmentListRoleBase(int userId) {
            var userAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == userId);
            if (userAccount?.Roleid != (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE) {
                return await _context.Departments
                    .Where(d => d.Id == userAccount.Departmentid)
                    .ToListAsync();
            }
            return await _context.Departments.ToListAsync();
        }

        public async Task<List<Major>> GetMajorListRoleBase(int userId) {
            var userAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == userId);
            if (userAccount.Roleid == (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE) { 
                return await _context.Majors.ToListAsync();
            } else {
                return await _context.Majors.Where(c => c.Departmentid == userAccount.Departmentid).ToListAsync();
            }
        }

        public async Task<List<Role>> GetRoleListRoleBase(int userId) {
            var userAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == userId);
            var roles = await _context.Roles.ToListAsync();
            roles.Remove(roles.First(c => c.Id == (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE));
            if (userAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM) {
                roles.Remove(roles.First(c => c.Id == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM));
            }
            return roles;
        }

        public async Task<List<T>> GetValueList() {
            return await _context.Set<T>().ToListAsync();
        }
    }
}
