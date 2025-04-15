using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Topicstudent
{
    public int Id { get; set; }

    public int Topicid { get; set; }

    public int Studentcode { get; set; }

    public bool Role { get; set; }

    public DateTime Createddate { get; set; }

    public virtual Student StudentcodeNavigation { get; set; } = null!;

    public virtual Topic Topic { get; set; } = null!;
}
