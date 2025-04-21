using System;
using System.Collections.Generic;

namespace PathAPI.Models;

public partial class Photo
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public virtual Location Location { get; set; } = null!;
}
