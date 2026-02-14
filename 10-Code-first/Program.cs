using _10_Code_first.Examples;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace _10_Code_first
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .Build();

            // Set up logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("=== One-to-Many Relationship Examples ===");
            logger.LogInformation("Demonstrating configuration consequences:\n");

            try
            {
                //await RelationshipExamples.Example1_ShadowNavigation_CannotNavigateToParent(logger);
                //await RelationshipExamples.Example2_ParentToChild_NavigationWorks(logger);
                //await RelationshipExamples.Example3_RequiredRelationship_CascadeDelete(logger);
                //await RelationshipExamples.Example4_RequiredRelationship_CannotCreateWithoutParent(logger);
                //await RelationshipExamples.Example5_OptionalRelationship_SetNull(logger);
                //await RelationshipExamples.Example6_OptionalVsRequired_CreatingEntities(logger);
                //await RelationshipExamples.Example7_QueryingWithoutNavigation(logger);
                //await RelationshipExamples.Example8_ReassigningRelationships(logger);

                logger.LogInformation("\n=== Value Object Examples ===");
                logger.LogInformation("Demonstrating owned entities and queryability:\n");

                await ValueObjectExamples.Example1_OwnedEntity_QueriesTranslateToSql(logger);
                await ValueObjectExamples.Example2_OwnedEntity_AutomaticallyLoaded(logger);

                logger.LogInformation("\n✅ All examples completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error occurred while running examples");
            }
        }
    }
}
