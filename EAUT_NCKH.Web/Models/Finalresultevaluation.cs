using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Finalresultevaluation
{
    public int Id { get; set; }

    public int Finalresultid { get; set; }

    public int Criteriaid { get; set; }

    public string? Value { get; set; }

    public int Createdby { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public virtual Account CreatedbyNavigation { get; set; } = null!;

    public virtual Criteriaevaluationtype Criteria { get; set; } = null!;

    public virtual Finalresult Finalresult { get; set; } = null!;
}
