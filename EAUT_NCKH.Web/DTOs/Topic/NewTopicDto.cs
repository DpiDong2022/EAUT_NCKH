namespace EAUT_NCKH.Web.DTOs {
    public class NewTopicDto {
        public string? Id { get; set; }
        public string Title { get; set; }
        public string? Note { get; set; }
        public int? SecondTeacherId { get; set; } = 0;
        public List<TopicStudentDto> Students { get; set; } = new List<TopicStudentDto>();
    }
}
