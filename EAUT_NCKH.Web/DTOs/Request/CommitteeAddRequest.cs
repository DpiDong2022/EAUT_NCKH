namespace EAUT_NCKH.Web.DTOs.Request
{
    public class CommitteeAddRequest
    {
        public int id { get; set; }
        public string topicid { get; set; }
        public string committeename { get; set; } = "";
        public int buildingid { get; set; }
        public int roomid { get; set; }
        public DateTime datetime { get; set; }
        public List<CommitteeMemberViewModel> members { get; set; }
    }

   public class CommitteeMemberViewModel {
        public int accountid { get; set; }
        public string fullname { get; set; } = "";
        public string email { get; set; } = "";
        public string phonenumber { get; set; } = "";
        public int roleid { get; set; }
        
    }
}
