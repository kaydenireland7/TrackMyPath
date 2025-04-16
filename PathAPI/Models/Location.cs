using System;
using System.Collections.Generic;

namespace PathAPI.Models;

public partial class Location
{
    public string LocationId { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
