
using Microsoft.EntityFrameworkCore;
using PicSearchAPI.db;

namespace PicSearchAPI
{
    public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			builder.Services.AddDbContext<PicSearchContext>(options=>options.UseNpgsql(builder.Configuration.GetConnectionString("PicSearchDb")),ServiceLifetime.Singleton);
			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.MapControllers();

			app.Run();
		}
	}
}