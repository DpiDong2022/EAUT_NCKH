using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Committeemember
{
    public int Id { get; set; }

    public int Committeeid { get; set; }

    public int Accountid { get; set; }

    public int Roleid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime? Updateddate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Committee Committee { get; set; } = null!;

    public virtual Committeerole Role { get; set; } = null!;
}
