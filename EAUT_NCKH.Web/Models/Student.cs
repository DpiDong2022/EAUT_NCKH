using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Student
{
    public string Id { get; set; } = null!;

    public int? Accountid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public int Trainingprogramid { get; set; }

    public int Majorid { get; set; }

    public string Classname { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public int? Departmentid { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Department? Department { get; set; }

    public virtual Major Major { get; set; } = null!;

    public virtual ICollection<Topicstudent> Topicstudents { get; set; } = new List<Topicstudent>();

    public virtual Trainingprogram Trainingprogram { get; set; } = null!;
}
