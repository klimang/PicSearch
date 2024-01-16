using PicSearchAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PicSearchAPI.db;

public partial class Picture
{
    public long ImageId { get; set; }

    public long Hash8 { get; set; }

    public byte[] Hash64 { get; set; } = null!;

    public string Filename { get; set; } = null!;

    public virtual ICollection<Link> Links { get; } = new List<Link>();

}
