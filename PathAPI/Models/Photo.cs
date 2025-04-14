using System;
using System.Collections.Generic;

namespace PathAPI.Models;

public partial class Photo
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public string FileUrl { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string? Caption { get; set; }

    public virtual Trip Trip { get; set; } = null!;
}
