using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Drawing.Drawing2D;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PicSearchAPI
{
	public class AHash
	{
		public static Dictionary<int,byte[]> generateHashes(Stream file,List<int> sizes)
		{
			Dictionary<int,byte[]> hashes= new Dictionary<int,byte[]>();	
			var image = new Bitmap(file);
			sizes.Sort();
			for (int i=sizes.Count-1; i>=0; i--)
			{
				if (sizes[i] >= image.Width && sizes[i] >= image.Height)
					return null;
				Bitmap new_image = new Bitmap(sizes[i], sizes[i]);
				Graphics g = Graphics.FromImage((System.Drawing.Image)new_image);
				g.InterpolationMode = InterpolationMode.High;
				g.DrawImage(image, 0, 0, sizes[i], sizes[i]);
				image= new_image;
				hashes.Add(sizes[i],createHash(toGrayscale(new_image), sizes[i]));
			}
			gcCollect();
			return hashes;
		}
		private static byte[,] toGrayscale(Bitmap map)
		{
			byte[,] gray = new byte[map.Width, map.Height];
			for (int y = 0; y < map.Height; y++)
			{
				for (int x = 0; x < map.Width; x++)
				{
					Color color = map.GetPixel(x, y);
					gray[x, y] = (byte)(((int)color.R + (int)color.G + (int)color.B) / 3);
				}
			}
			return gray;
		}
		private static byte[] createHash(byte[,] map, int size)
		{
			long common = 0;
			for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) common += map[x, y];
			common /= size * size;
			for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) if (map[x, y] > common) map[x, y] = 1; else map[x, y] = 0;
			byte[] hashBits = new byte[size * size];
			for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) hashBits[x * size + y] = map[x, y];
			byte[] hash = new byte[hashBits.Length / 8];
			for (int x = 0; x < hashBits.Length; x++)
			{
				hash[x / 8] += (byte)(hashBits[x] << (7 - x % 8));
			}
			return hash;
		}

		private static void gcCollect()
		{
				GC.Collect();
				GC.WaitForPendingFinalizers();
		}

	}
}
