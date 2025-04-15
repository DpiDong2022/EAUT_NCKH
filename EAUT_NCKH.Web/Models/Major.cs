using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Major
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int Displayorder { get; set; }

    public int Departmentid { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
