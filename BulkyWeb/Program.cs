using Bulky.DataAccess.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BulkyWeb;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Host.UseSerilog((context, configuration) =>
			configuration.ReadFrom.Configuration(context.Configuration)
		);

		builder.Services.AddControllersWithViews();

		builder.Services.AddDbContext<ApplicationDbContext>(o =>
			o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

		var app = builder.Build();

		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
			app.UseHsts();
		}

		app.UseHttpsRedirection();

		app.UseStaticFiles();

		app.UseRouting();

		app.UseSerilogRequestLogging();

		app.UseAuthorization();

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");

		app.Run();
	}
}
