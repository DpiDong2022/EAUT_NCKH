using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Evaluationimage
{
    public int Id { get; set; }

    public int Defenseassignmentid { get; set; }

    public string Filepath { get; set; } = null!;

    public string Filename { get; set; } = null!;

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public virtual Defenseassignment Defenseassignment { get; set; } = null!;
}
