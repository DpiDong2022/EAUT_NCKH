using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace EAUT_NCKH.Web.Repositories {
    public class CommitteeRepository: ICommitteeRepository {
        private readonly EntityDataContext _context;
        public CommitteeRepository(EntityDataContext context) {
            _context = context;
        }

        public async Task<List<Committee>> GetCommittees() {
            return await _context.Committees.ToListAsync();
        }

        public async Task<List<Committeetype>> GetCommitteeTypes() {
            var data = _context.Committeetypes;
            if (data!=null) {
                return await data.ToListAsync();
            }
            return new List<Committeetype>();
        }
    }
}
