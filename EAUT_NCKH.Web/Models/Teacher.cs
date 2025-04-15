using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Teacher
{
    public int Id { get; set; }

    public string Academictitle { get; set; } = null!;

    public int Accountid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public int Majorid { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Major Major { get; set; } = null!;
}
