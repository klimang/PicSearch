
namespace PicSearchAPI.Models
{
    public class ResultPicture
    {
		public class Link
		{
			public string ImageUrl { get; set; }
			public string Url { get; set; }
			public string Domain { get; set; }
			public Link(db.Link link)
			{
				this.Domain = link.Domain;
				this.ImageUrl = link.ImageUrl;
				this.Url = link.Url;
			}
		}
		public string Thumbnail { get; set; }
		public virtual ICollection<Link> Links { get; set; }

		public ResultPicture(db.Picture picture)
		{
			this.Thumbnail = picture.Filename;
			this.Links=new List<Link>();
            foreach (var item in picture.Links)
            {
				this.Links.Add(new Link(item));
            }
        }
	}
}

