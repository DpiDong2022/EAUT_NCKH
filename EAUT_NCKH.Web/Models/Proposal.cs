using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Proposal
{
    public int Id { get; set; }

    public int Topicid { get; set; }

    public DateTime Submitdeadline { get; set; }

    public DateTime? Submitdate { get; set; }

    public string? Filepath { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<Proposalevaluation> Proposalevaluations { get; set; } = new List<Proposalevaluation>();

    public virtual Topic Topic { get; set; } = null!;
}
