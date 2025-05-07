using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Major> Majors { get; set; } = new List<Major>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
