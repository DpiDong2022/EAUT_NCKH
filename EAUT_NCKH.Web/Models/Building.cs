using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Building
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
