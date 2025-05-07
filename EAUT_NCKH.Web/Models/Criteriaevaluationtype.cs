using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Criteriaevaluationtype
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public bool Type { get; set; }

    public DateTime Createddate { get; set; }

    public int? Maxscore { get; set; }

    public virtual ICollection<Finalresultevaluation> Finalresultevaluations { get; set; } = new List<Finalresultevaluation>();
}
