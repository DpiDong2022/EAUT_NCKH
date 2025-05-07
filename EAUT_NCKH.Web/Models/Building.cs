using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Building
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
