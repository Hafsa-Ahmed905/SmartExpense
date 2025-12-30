using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FP.Data;

namespace FP.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services, ILogger logger)
        {
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Check if FirstName and LastName columns exist, add them if they don't
                try
                {
                    // Test if columns exist by trying to query them
                    var testUser = await context.Users.FirstOrDefaultAsync();
                    if (testUser != null)
                    {
                        // Try to access FirstName property - if it fails, we need to add the column
                        var firstName = testUser.FirstName;
                    }
                }
                catch
                {
                    // Columns don't exist, add them manually
                    await context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE AspNetUsers ADD FirstName NVARCHAR(100) NOT NULL DEFAULT ''");
                    await context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE AspNetUsers ADD LastName NVARCHAR(100) NOT NULL DEFAULT ''");
                    logger.LogInformation("Added FirstName and LastName columns to AspNetUsers table");
                }

                // Create default user if none exists
                if (!await context.Users.AnyAsync())
                {
                    var user = new ApplicationUser
                    {
                        UserName = "free@test.com",
                        Email = "free@test.com",
                        FirstName = "Test",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Test123!");
                    
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Default user created successfully");
                    }
                    else
                    {
                        logger.LogError("Failed to create default user: {errors}", 
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }
    }
}
