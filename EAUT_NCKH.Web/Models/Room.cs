using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Room
{
    public int Id { get; set; }

    public int Buildingid { get; set; }

    public string Name { get; set; } = null!;

    public virtual Building Building { get; set; } = null!;

    public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
}
