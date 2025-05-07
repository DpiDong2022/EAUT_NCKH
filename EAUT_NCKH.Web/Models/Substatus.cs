using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Substatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Displayorder { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<Proposalevaluation> Proposalevaluations { get; set; } = new List<Proposalevaluation>();
}
