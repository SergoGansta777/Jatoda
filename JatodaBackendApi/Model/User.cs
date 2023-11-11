using System;
using System.Collections.Generic;

namespace JatodaBackendApi.Model;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Passwordsalt { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Todonote> Todonotes { get; set; } = new List<Todonote>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
