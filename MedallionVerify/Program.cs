// See https://aka.ms/new-console-template for more information
using Medallion.Threading.Postgres;

Console.WriteLine("Hello, World!");
string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=yourpassword;";
string lockname = Console.ReadLine();
var @lock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(lockname, allowHashing: true), connectionString);
await using (await @lock.AcquireAsync())
{
    Console.WriteLine($"lock Entered:{DateTime.Now}");
    Thread.Sleep(10000);
    Console.WriteLine($"lock released:{DateTime.Now}");
}
Console.ReadLine();

