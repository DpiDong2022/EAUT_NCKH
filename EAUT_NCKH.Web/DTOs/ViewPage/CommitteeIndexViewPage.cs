using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs
{
    public class CommitteeIndexViewPage : IndexViewPage<Committee> {
        public int DepartmentId { get; set; } = -1;
        public string Search { get; set; } = "";
        public int TypeId { get; set; } = -1;
        public int RoomId { get; set; } = -1;
        public int BuildingId { get; set; } = -1;
        public DateTime From { get; set; } = new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);
        public DateTime To { get; set; } = DateTime.Today.AddMonths(2);
        public List<Department> Departments { get; set; }
        public List<Committeetype> CommitteeTypes { get; set; }
        public List<Building> Buildings { get; set; }
        public List<Room> Rooms { get; set; }
    }
}
