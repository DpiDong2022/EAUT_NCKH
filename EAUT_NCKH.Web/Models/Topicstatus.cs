using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Topicstatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public string? Description { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
