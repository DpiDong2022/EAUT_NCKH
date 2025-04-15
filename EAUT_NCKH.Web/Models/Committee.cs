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

    public virtual ICollection<Defenseassignment> Defenseassignments { get; set; } = new List<Defenseassignment>();

    public virtual ICollection<Finalresult> Finalresults { get; set; } = new List<Finalresult>();

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual Committeetype Type { get; set; } = null!;
}
