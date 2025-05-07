namespace EAUT_NCKH.Web.DTOs.Options {
    public class FinalResultEvaluation {
        public string? TopicId { get; set; }
        public List<EvaluationItem> EvaluationItems { get; set; }
    }

    public class EvaluationItem {
        public int Id { get; set; }
        public string? Value { get; set; }
        public string? Content { get; set; }
        public bool IsScore { get; set; }
    }
}
