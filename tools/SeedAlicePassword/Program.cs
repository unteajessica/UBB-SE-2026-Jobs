using Microsoft.AspNetCore.Identity;

const string defaultPassword = "Password123!";
string password = args.Length > 0 ? args[0] : defaultPassword;

var hasher = new PasswordHasher<object>();
string hash = hasher.HashPassword(null!, password);

Console.WriteLine("-- Run this against your ISS-921-1 database");
Console.WriteLine("-- Sets alice.smith@example.com password to: " + password);
Console.WriteLine();
Console.WriteLine($"UPDATE Users SET PasswordHash = '{hash}' WHERE Email = 'alice.smith@example.com';");
Console.WriteLine();
Console.WriteLine("-- Done. Alice can now log in at /Account/Login");
