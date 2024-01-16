using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PicSearchAPI.Models;
using System.IO;

namespace PicSearchAPI.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        db.PicSearchContext dbContext;
        public HomeController(db.PicSearchContext db)
        {
            this.dbContext = db;
        }

        [HttpPost("find")]
        [Produces("application/json")]
		[RequestSizeLimit(10_485_760)]
		public async Task<IResult> SearchAsync([FromForm]FileUploadModel model)
        {
            if (model == null || model.File.Length <= 0)
            {
                return Results.Json(new { message = "File lenght equals 0" }, statusCode: 422) ;
            }
            try
            {
                List<ResultPicture> pictures=SearchImage(model.File.OpenReadStream());
				return Results.Json(new { result = pictures}, statusCode: 200);
			}
            catch (Exception ex)
            {
				return Results.Json(new { message = ex.Message }, statusCode: 422);
			}
        }

        [HttpGet("thumbnail/{f1}/{f2}/{thumbnail}")]
        public async Task<IActionResult> GetThumbnail([FromRoute] string f1,[FromRoute] string f2,[FromRoute] string thumbnail)
        {
            string path= Path.Combine("\\app\\thumbnails\\",f1,f2,thumbnail);
			try
            {
                return File(new FileStream(path, FileMode.Open), "image/jpeg", thumbnail);
            }catch (Exception ex)
            {
                return NotFound();
            }
        }

        List<ResultPicture> SearchImage(Stream file)
        {
            Dictionary<int,byte[]> hashes=AHash.generateHashes(file, new List<int> { 8,64 });
            List<db.Picture> h8Pictures=dbContext.Pictures.Include(x=>x.Links).Where(x => x.Hash8 == BitConverter.ToInt64(hashes[8])).ToList();
			List<ResultPicture> result = new List<ResultPicture>();
			if (h8Pictures.Count != 0)
            {
                file.Seek(0,SeekOrigin.Begin);  
                byte[] hash64 = AHash.generateHashes(file,new List<int> { 64})[64];
                foreach (db.Picture p in h8Pictures)
                {
                    if (hash64.SequenceEqual(p.Hash64)) result.Add(new ResultPicture(p));
                }
            }
            return result;
        }

	}

}
