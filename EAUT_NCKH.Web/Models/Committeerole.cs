using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Committeerole
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public virtual ICollection<Committeemember> Committeemembers { get; set; } = new List<Committeemember>();
}
