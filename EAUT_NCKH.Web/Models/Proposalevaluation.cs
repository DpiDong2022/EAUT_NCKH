using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Proposalevaluation
{
    public int Id { get; set; }

    public int Proposalid { get; set; }

    public int Approverid { get; set; }

    public byte Type { get; set; }

    public string? Feedback { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public int Topicid { get; set; }

    public int Statusid { get; set; }

    public virtual Account Approver { get; set; } = null!;

    public virtual Proposal Proposal { get; set; } = null!;

    public virtual Substatus Status { get; set; } = null!;

    public virtual Topic Topic { get; set; } = null!;
}
