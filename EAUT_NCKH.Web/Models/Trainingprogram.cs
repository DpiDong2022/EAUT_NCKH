using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Trainingprogram
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
