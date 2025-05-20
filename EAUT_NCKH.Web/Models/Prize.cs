using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Prize
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Defenseassignment> Defenseassignments { get; set; } = new List<Defenseassignment>();
}
