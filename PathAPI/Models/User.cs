using System;
using System.Collections.Generic;

namespace PathAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    //public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
