using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Notificationtemplate { get; set; } = null!;

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public virtual ICollection<Notificationaccount> Notificationaccounts { get; set; } = new List<Notificationaccount>();
}
