using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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

                // Apply any pending migrations and create database
                await context.Database.MigrateAsync();

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
                
                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }
    }
}
