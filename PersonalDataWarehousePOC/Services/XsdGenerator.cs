using System;
using System.IO;
using System.Linq;
using System.Text;

public static class XsdGenerator
{
    public static string GenerateSchemaForType(Type type)
    {
        var className = type.Name;

        var sb = new StringBuilder();

        // XML declaration
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");

        // Open xs:schema
        sb.AppendLine(@"<xs:schema elementFormDefault=""qualified"" id=""ReportItemSchemas"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">");

        // Top-level element
        sb.AppendLine($"  <xs:element name=\"{className}\" nillable=\"true\" type=\"{className}\" />");

        // Complex type definition
        sb.AppendLine($"  <xs:complexType name=\"{className}\">");
        sb.AppendLine("    <xs:sequence>");

        // Get only writeable (non-read-only) properties
        var properties = type.GetProperties()
            .Where(p => p.CanWrite)
            .ToArray();

        foreach (var prop in properties)
        {
            var name = prop.Name;
            var propType = prop.PropertyType;
            var xsType = MapToXsdType(propType);

            // Determine minOccurs
            bool isNullable = Nullable.GetUnderlyingType(propType) != null;
            bool isReferenceType = !propType.IsValueType || isNullable;
            int minOccurs = isReferenceType ? 0 : 1;

            sb.AppendLine(
                $"      <xs:element minOccurs=\"{minOccurs}\" maxOccurs=\"1\" name=\"{name}\" type=\"{xsType}\" />"
            );
        }

        sb.AppendLine("    </xs:sequence>");
        sb.AppendLine("  </xs:complexType>");

        // Close xs:schema
        sb.AppendLine("</xs:schema>");

        return sb.ToString();
    }

    private static string MapToXsdType(Type type)
    {
        // If it's a Nullable<T>, extract T
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
        {
            type = underlyingType;
        }

        if (type == typeof(string))
            return "xs:string";
        if (type == typeof(decimal))
            return "xs:decimal";
        if (type == typeof(int))
            return "xs:int";
        if (type == typeof(double))
            return "xs:double";
        if (type == typeof(DateTime))
            return "xs:dateTime";

        // Fallback
        return "xs:anyType";
    }

    public static Type GetTypeFromCurrentDomain(string fullyQualifiedClassName)
    {
        // e.g. "MyNamespace.ReportItem"
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullyQualifiedClassName))
            .FirstOrDefault(t => t != null);
    }
}