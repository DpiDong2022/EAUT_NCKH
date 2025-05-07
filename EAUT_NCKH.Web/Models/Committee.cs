using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Committee
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Typeid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public DateTime? Eventdate { get; set; }

    public int? Buildingid { get; set; }

    public int? Roomid { get; set; }

    public int Topicid { get; set; }

    public virtual Building? Building { get; set; }

    public virtual ICollection<Committeemember> Committeemembers { get; set; } = new List<Committeemember>();

    public virtual Room? Room { get; set; }

    public virtual Topic Topic { get; set; } = null!;

    public virtual Committeetype Type { get; set; } = null!;
}
