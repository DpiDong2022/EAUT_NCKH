using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Committeetype
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public virtual ICollection<Committeerole> Committeeroles { get; set; } = new List<Committeerole>();

    public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
}
