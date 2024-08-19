using Microsoft.EntityFrameworkCore;
using Models;
namespace BlueDevilBridges.Data;

public class BlueDevilBridgesContext : DbContext
{
	public BlueDevilBridgesContext(DbContextOptions<BlueDevilBridgesContext> options) : base(options)
	{
		Database.SetCommandTimeout(120);
	}

	public DbSet<TestRecord> TestRecords { get; set; }
	public DbSet<AlumniRecord> AlumniRecords { get; set; }
	public DbSet<StudentRecord> StudentRecords { get; set; }
	public DbSet<PastMatchesRecord> PastMatchesRecords {get; set;}
	public DbSet<PairingRecord> PairingRecords {get; set;}

	public static readonly ILoggerFactory CustomLoggerFactory = LoggerFactory.Create(builder =>
	{
		builder
		.AddFilter((category, level) =>
			category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)
		.AddConsole();
	});

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseLoggerFactory(CustomLoggerFactory);
	}

	//protected override void OnModelCreating(ModelBuilder modelBuilder)
	//{

	//}
}