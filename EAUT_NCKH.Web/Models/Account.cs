using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Fullname { get; set; } = null!;

    public bool Sex { get; set; }

    public string Email { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public string? Avatar { get; set; }

    public string Password { get; set; } = null!;

    public int? Departmentid { get; set; }

    public int Roleid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public string? Otp { get; set; }

    public DateTime? Otpgeneratedat { get; set; }

    public virtual ICollection<Committeemember> Committeemembers { get; set; } = new List<Committeemember>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Finalresultevaluation> Finalresultevaluations { get; set; } = new List<Finalresultevaluation>();

    public virtual ICollection<Notificationaccount> Notificationaccounts { get; set; } = new List<Notificationaccount>();

    public virtual ICollection<Proposalevaluation> Proposalevaluations { get; set; } = new List<Proposalevaluation>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public virtual ICollection<Topic> TopicCreatedbyNavigations { get; set; } = new List<Topic>();

    public virtual ICollection<Topic> TopicSecondteachers { get; set; } = new List<Topic>();

    public virtual ICollection<Topic> TopicStudents { get; set; } = new List<Topic>();
}
