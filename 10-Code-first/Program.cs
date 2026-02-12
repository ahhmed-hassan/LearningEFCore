using _10_Code_first.Examples;
using Microsoft.Extensions.Configuration;

namespace _10_Code_first
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .Build();

            Console.WriteLine("=== One-to-Many Relationship Examples ===");
            Console.WriteLine("Demonstrating configuration consequences:\n");

            try
            {
                await RelationshipExamples.Example1_ShadowNavigation_CannotNavigateToParent();
                await RelationshipExamples.Example2_ParentToChild_NavigationWorks();
                await RelationshipExamples.Example3_RequiredRelationship_CascadeDelete();
                await RelationshipExamples.Example4_RequiredRelationship_CannotCreateWithoutParent();
                await RelationshipExamples.Example5_OptionalRelationship_SetNull();
                await RelationshipExamples.Example6_OptionalVsRequired_CreatingEntities();
                await RelationshipExamples.Example7_QueryingWithoutNavigation();
                await RelationshipExamples.Example8_ReassigningRelationships();

                Console.WriteLine("\n✅ All examples completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
