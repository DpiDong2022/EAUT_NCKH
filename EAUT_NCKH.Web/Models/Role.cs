using System;
using System.Collections.Generic;

namespace EAUT_NCKH.Web.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Displayorder { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
