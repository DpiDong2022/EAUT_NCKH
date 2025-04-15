using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.Repositories.IRepositories;

namespace EAUT_NCKH.Web.Repositories {
    public class AuthRepository : IAuthRepository {
        private readonly EntityDataContext _context;

        public AuthRepository(EntityDataContext context) {
            _context = context;
        }
    }
}
