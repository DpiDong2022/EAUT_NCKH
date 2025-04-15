using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Notificationaccount
{
    public int Id { get; set; }

    public int Receiverid { get; set; }

    public int Notificationid { get; set; }

    public string Notificationcontent { get; set; } = null!;

    public string Link { get; set; } = null!;

    public bool Isread { get; set; }

    public DateTime Createddate { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual Account Receiver { get; set; } = null!;
}
