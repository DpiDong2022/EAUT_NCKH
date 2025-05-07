using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Topicstudent
{
    public int Id { get; set; }

    public int Topicid { get; set; }

    public string Studentcode { get; set; } = null!;

    public DateTime Createddate { get; set; }

    public bool Role { get; set; }

    public virtual Student StudentcodeNavigation { get; set; } = null!;

    public virtual Topic Topic { get; set; } = null!;
}
