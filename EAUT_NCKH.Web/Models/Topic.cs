using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Topic
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int Departmentid { get; set; }

    public int? Studentid { get; set; }

    public int Createdby { get; set; }

    public int? Secondteacherid { get; set; }

    public int Year { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public int Status { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();

    public virtual Account CreatedbyNavigation { get; set; } = null!;

    public virtual ICollection<Defenseassignment> Defenseassignments { get; set; } = new List<Defenseassignment>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Finalresultevaluation> Finalresultevaluations { get; set; } = new List<Finalresultevaluation>();

    public virtual ICollection<Finalresult> Finalresults { get; set; } = new List<Finalresult>();

    public virtual ICollection<Proposalevaluation> Proposalevaluations { get; set; } = new List<Proposalevaluation>();

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual Account? Secondteacher { get; set; }

    public virtual Topicstatus StatusNavigation { get; set; } = null!;

    public virtual Account? Student { get; set; }

    public virtual ICollection<Topicstudent> Topicstudents { get; set; } = new List<Topicstudent>();
}
