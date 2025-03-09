using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Data;

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

    public static string GenerateXmlForType(Type type, DataTable dt)
    {
        // Use the type name and a simple plural form for the container element.
        var className = type.Name;           // e.g. "Customer"
        var pluralName = className + "s";      // e.g. "Customers"

        var sb = new StringBuilder();

        // Outer XML document structure.
        sb.AppendLine("<Query>");
        sb.AppendLine("  <XmlData>");
        sb.AppendLine($"    <{pluralName} xmlns=\"http://www.BlazorData.net\">");

        // Iterate through each row in the DataTable.
        foreach (DataRow row in dt.Rows)
        {
            // Start the element for this instance.
            sb.Append($"      <{className}");
            // If the DataTable contains an "ID" column, output it as an attribute.
            if (dt.Columns.Contains("ID"))
            {
                var idValue = row["ID"];
                if (idValue != DBNull.Value)
                {
                    sb.Append($" ID=\"{idValue}\"");
                }
            }
            sb.AppendLine(">");

            // Iterate over each column (except "ID") to output its value.
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.Equals("ID", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Use the value from the row (or an empty string if the value is null)
                var value = row[column] != DBNull.Value ? row[column].ToString() : string.Empty;
                // Escape the value to ensure it is valid XML.
                sb.AppendLine($"        <{column.ColumnName}>{System.Security.SecurityElement.Escape(value)}</{column.ColumnName}>");
            }

            sb.AppendLine($"      </{className}>");
        }

        // Close the outer XML elements.
        sb.AppendLine($"    </{pluralName}>");
        sb.AppendLine("  </XmlData>");
        sb.AppendLine("</Query>");

        return sb.ToString();
    }

    // Utility

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

    public static Type GetTypeFromCode(string code, string ClassName)
    {
        // 1. Parse the syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // 2. Set up references (you’ll need references to System, netstandard, etc.)
        var references = AppDomain.CurrentDomain
                                 .GetAssemblies()
                                 .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                 .Select(a => MetadataReference.CreateFromFile(a.Location))
                                 .Cast<MetadataReference>()
                                 .ToList();

        // 3. Create the Roslyn compilation
        var compilation = CSharpCompilation.Create(
            assemblyName: "DynamicAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // 4. Emit to an in-memory stream
        using var ms = new MemoryStream();
        EmitResult emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
        {
            foreach (var diagnostic in emitResult.Diagnostics)
            {
                Console.WriteLine(diagnostic.ToString());
            }
            throw new InvalidOperationException("Compilation failed!");
        }

        ms.Seek(0, SeekOrigin.Begin);

        // 5. Load the assembly from the stream
        Assembly assembly = Assembly.Load(ms.ToArray());

        // 6. Get our newly compiled type
        return assembly.GetType($"Controllers.{ClassName}");       
    }

    public static string GetTableName(string rdlcFilePath)
    {
        // Load the RDLC file as an XDocument
        XDocument document = XDocument.Load(rdlcFilePath);

        // The 'rd' namespace is commonly used for Reporting Designer elements
        XNamespace rdNamespace = "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner";

        // Attempt to find an element named "TableName" within that namespace
        XElement schemaPathElement = document
            .Descendants(rdNamespace + "TableName")
            .FirstOrDefault();

        return schemaPathElement?.Value;
    }
}