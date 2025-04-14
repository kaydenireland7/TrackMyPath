using System;
using System.Collections.Generic;

namespace PathAPI.Models;

public partial class Location
{
    public string Id { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
