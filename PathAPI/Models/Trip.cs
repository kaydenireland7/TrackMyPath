using System;
using System.Collections.Generic;

namespace PathAPI.Models;

public partial class Trip
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? TripName { get; set; }
    public virtual User User { get; set; } = null!;
}
