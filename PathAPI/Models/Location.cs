using System;
using System.Collections.Generic;

namespace PathAPI.Models;

public partial class Location
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public DateTime Timestamp { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public float? Accuracy { get; set; }
    public decimal? Speed { get; set; }

    //public virtual Photo Photo { get; set; } = null!;
    // public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
