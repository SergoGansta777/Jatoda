using System;
using System.Collections.Generic;

namespace JatodaBackendApi.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Todonote> Todonotes { get; set; } = new List<Todonote>();
}
