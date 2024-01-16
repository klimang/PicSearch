using System;
using System.Collections.Generic;

namespace PicSearchAPI.db;

public partial class Link
{
    public long PictureId { get; set; }

    public long LinkId { get; set; }

    public string Domain { get; set; }

    public string Url { get; set; }

    public string ImageUrl { get; set; }

    public virtual Picture Picture { get; set; } = null!;
}
