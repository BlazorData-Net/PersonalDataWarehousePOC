using Microsoft.OData.ModelBuilder;
using static PersonalDataWarehousePOC.Program;
using System.Reflection;

public static class ODataSupport
{
    public static void AddDynamicEntitySets(ODataConventionModelBuilder modelBuilder)
    {
        // Assume you want to add all classes in the current assembly marked with a specific attribute
        var entityTypes = Assembly.GetExecutingAssembly()
                                  .GetTypes()
                                  .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ODataEntityAttribute>() != null);

        foreach (var type in entityTypes)
        {
            // Dynamically invoke the generic EntitySet<T>() method
            modelBuilder.GetType()
                .GetMethod("EntitySet", BindingFlags.Instance | BindingFlags.Public)
                .MakeGenericMethod(type)
                .Invoke(modelBuilder, new object[] { type.Name + "s" });
        }
    }
}