using System;
using System.Collections.Generic;

namespace Racing.Models;

public partial class Race
{
    public int Id { get; set; }

    public int? MeetingId { get; set; }

    public string? Name { get; set; }

    public int? Number { get; set; }

    public int? Visible { get; set; }

    public string? AdvertisedStartTime { get; set; }
}
