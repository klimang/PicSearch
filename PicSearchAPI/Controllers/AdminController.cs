using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PicSearchAPI.db;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Linq.Expressions;
using System.Net;
using Npgsql.Replication.PgOutput.Messages;
using Microsoft.EntityFrameworkCore;
using PicSearchAPI.Models;

namespace PicSearchAPI.Controllers
{
	[Route("admin/")]
	[ApiController]
	public class AdminController : ControllerBase
	{
		db.PicSearchContext dbContext;
		public AdminController(db.PicSearchContext db)
		{
			this.dbContext = db;
		}

		[HttpPost]
		public async Task<IActionResult> AddImage(string resourceUrl,string imageUrl)
		{
			Stream stream = await GetImage(imageUrl);
			AddPictureToDb(resourceUrl,imageUrl,stream);
			return Ok();
		}


		//0-добавлено
		//1-изображение и url существует
		//2-добавлено url
		private int AddPictureToDb(string resourceUrl, string imageUrl, Stream stream)
		{
			if (stream == null) { throw new Exception(); }
			Dictionary<int, byte[]> hashes = AHash.generateHashes(stream, new List<int> { 8, 64 });
			Picture? p = FindPictureInDb(hashes[8], hashes[64]);
			if (p!= null){
				if (p.Links.Where(x => x.Url == resourceUrl).Count()!=0)
				{
					return 1;
				}
				Link link=new Link() { Url = resourceUrl,ImageUrl=imageUrl,Domain=new Uri(resourceUrl).Host };
				p.Links.Add(link);
				dbContext.SaveChanges();
				return 2;
			}
			db.Picture picture = new Picture();
			picture.Hash8 = BitConverter.ToInt64(hashes[8]);
			picture.Hash64 = hashes[64];
			Link lnk = new Link() { Url = resourceUrl, ImageUrl = imageUrl, Domain = new Uri(resourceUrl).Host };
			picture.Links.Add(lnk);
			
			Stream thumbnailStream = CreateThumbnail(stream, 256);
			//picture.Thumbnail=new byte[thumbnailStream.Length];
				//thumbnailStream.Read(picture.Thumbnail);
			var guid=Guid.NewGuid().ToString("N");
			string imagePath= Path.Combine(guid.Substring(0, 2), guid.Substring(2, 2), guid + ".jpg");
			string filePath = Path.Combine("\\app\\thumbnails", imagePath);
			Directory.CreateDirectory(new FileInfo(filePath).DirectoryName);
			using(var FileStream=new FileStream(filePath, FileMode.Create))
			{
				thumbnailStream.CopyTo(FileStream);
			}
			picture.Filename = imagePath;
			dbContext.Pictures.Add(picture);
			dbContext.SaveChanges();
			return 0;
		}

		private Picture? FindPictureInDb(byte[] hash8, byte[] hash64)
		{
			db.Picture? picture = dbContext.Pictures.Include(x => x.Links)
				.Where(x => x.Hash8 == BitConverter.ToInt64(hash8))
				.Where(x=>x.Hash64.SequenceEqual(hash64))
				.Include(x => x.Links)
				.FirstOrDefault();
			return picture;
		}
		private async Task<Stream> GetImage(string imageUrl)
		{
			try
			{
				using (HttpClient client = new HttpClient())
				{
					var response = await client.GetAsync(imageUrl);
					if (response.IsSuccessStatusCode)
					{
						return await response.Content.ReadAsStreamAsync();
					}
				}
			}catch (Exception ex){}
			return null;
		}
		private Stream CreateThumbnail(Stream imageStream,int size)
		{
			MemoryStream stream = new MemoryStream();
			Bitmap image= new Bitmap(imageStream);	
			Bitmap new_image = new Bitmap(size, size);
			Graphics g = Graphics.FromImage((System.Drawing.Image)new_image);
			g.InterpolationMode = InterpolationMode.High;
			g.DrawImage(image, 0, 0, size, size);
			new_image.Save(stream,System.Drawing.Imaging.ImageFormat.Jpeg);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;	
		}
	}
}
