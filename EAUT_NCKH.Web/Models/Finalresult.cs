using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Finalresult
{
    public int Id { get; set; }

    public int Topicid { get; set; }

    public DateTime Submitdeadline { get; set; }

    public DateTime? Submitdate { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public string? Filepath { get; set; }

    public virtual ICollection<Finalresultevaluation> Finalresultevaluations { get; set; } = new List<Finalresultevaluation>();

    public virtual Topic Topic { get; set; } = null!;
}
