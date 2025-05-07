using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Defenseassignment
{
    public int Id { get; set; }

    public int Topicid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public int? Finalscore { get; set; }

    public virtual ICollection<Evaluationimage> Evaluationimages { get; set; } = new List<Evaluationimage>();

    public virtual Topic Topic { get; set; } = null!;
}
