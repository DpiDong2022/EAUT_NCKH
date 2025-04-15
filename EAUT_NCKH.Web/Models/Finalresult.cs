using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Finalresult
{
    public int Id { get; set; }

    public int Topicid { get; set; }

    public DateTime Submitdeadline { get; set; }

    public int? Status { get; set; }

    public string? Wordfilepath { get; set; }

    public string? Pptfilepath { get; set; }

    public DateTime? Submitdate { get; set; }

    public int? Committeeid { get; set; }

    public DateTime? Datetime { get; set; }

    public int? Buildingid { get; set; }

    public int? Roomid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public virtual Committee? Committee { get; set; }

    public virtual ICollection<Finalresultevaluation> Finalresultevaluations { get; set; } = new List<Finalresultevaluation>();

    public virtual Topic Topic { get; set; } = null!;
}
