using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Student
{
    public int Id { get; set; }

    public int Accountid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public int Trainingprogramid { get; set; }

    public int Majorid { get; set; }

    public string Classname { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual Major Major { get; set; } = null!;

    public virtual ICollection<Topicstudent> Topicstudents { get; set; } = new List<Topicstudent>();

    public virtual Trainingprogram Trainingprogram { get; set; } = null!;
}
