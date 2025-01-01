namespace PersonalDataWarehousePOC.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Collections.Generic;

    public static class DynamicRdlBuilder
    {
        // Minimal dictionary for mapping C# types to RDL-friendly data types
        private static readonly Dictionary<Type, string> RdlTypeNameMap = new Dictionary<Type, string>
    {
        { typeof(string), "System.String" },
        { typeof(int), "System.Int32" },
        { typeof(int?), "System.Int32" },
        { typeof(double), "System.Double" },
        { typeof(double?), "System.Double" },
        { typeof(decimal), "System.Decimal" },
        { typeof(decimal?), "System.Decimal" },
        { typeof(DateTime), "System.DateTime" },
        { typeof(DateTime?), "System.DateTime" },
        // Add more if needed, otherwise default to System.String
    };

        /// <summary>
        /// Generates an RDL (as a string) based on the public properties of T.
        /// This version creates:
        /// - One DataSet with a field for each property
        /// - A Tablix with a column for each property, plus a header row and detail row
        /// </summary>
        /// <typeparam name="T">The class type whose public properties you want to reflect.</typeparam>
        /// <param name="reportTitle">Used by the sample Title parameter in the RDL.</param>
        /// <param name="schemaPath">If you have an .xsd location, you can pass it here. Otherwise it’s optional.</param>
        /// <returns>A string containing valid RDL XML.</returns>
        public static string GenerateDynamicRdl(
            Type type,
            string reportTitle = "Dynamic Report",
            string schemaPath = @"C:\MySchemas\DynamicDataSchema.xsd")
        {
            // 1) Use reflection to get public properties of T
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .Where(p => p.CanRead)
                                     .ToArray();

            // 2) Build the <Fields> block
            var fieldsBuilder = new StringBuilder();
            foreach (var prop in properties)
            {
                string fieldName = prop.Name;
                string rdTypeName = GetRdlTypeName(prop.PropertyType);

                fieldsBuilder.AppendLine($@"
        <Field Name=""{fieldName}"">
          <DataField>{fieldName}</DataField>
          <rd:TypeName>{rdTypeName}</rd:TypeName>
        </Field>");
            }

            // 3) Build the Tablix columns, header row, and detail row
            var tablixColumnsBuilder = new StringBuilder();
            var tablixHeaderCellsBuilder = new StringBuilder();
            var tablixDetailCellsBuilder = new StringBuilder();

            // Get all properties except the Id property
            foreach (var prop in properties.Where(x => x.Name.ToLower() != "id"))
            {
                // Each property -> one column
                tablixColumnsBuilder.AppendLine(@"
                <TablixColumn>
                  <Width>2.54cm</Width>
                </TablixColumn>");

                // Header cell
                tablixHeaderCellsBuilder.AppendLine($@"
                    <TablixCell>
                      <CellContents>
                        <Textbox Name=""Header_{prop.Name}"">
                          <CanGrow>true</CanGrow>
                          <KeepTogether>true</KeepTogether>
                          <Paragraphs>
                            <Paragraph>
                              <TextRuns>
                                <TextRun>
                                  <Value>{prop.Name.ToUpper()}</Value>
                                  <Style>
                                    <FontFamily>Cambria</FontFamily>
                                    <FontWeight>Bold</FontWeight>
                                  </Style>
                                </TextRun>
                              </TextRuns>
                              <Style>
                                <TextAlign>Center</TextAlign>
                              </Style>
                            </Paragraph>
                          </Paragraphs>
                          <rd:DefaultName>Header_{prop.Name}</rd:DefaultName>
                          <Style>
                            <Border>
                              <Style>Solid</Style>
                            </Border>
                            <PaddingLeft>2pt</PaddingLeft>
                            <PaddingRight>2pt</PaddingRight>
                            <PaddingTop>2pt</PaddingTop>
                            <PaddingBottom>2pt</PaddingBottom>
                          </Style>
                        </Textbox>
                      </CellContents>
                    </TablixCell>");

                // Detail cell
                tablixDetailCellsBuilder.AppendLine($@"
                    <TablixCell>
                      <CellContents>
                        <Textbox Name=""Detail_{prop.Name}"">
                          <CanGrow>true</CanGrow>
                          <KeepTogether>true</KeepTogether>
                          <Paragraphs>
                            <Paragraph>
                              <TextRuns>
                                <TextRun>
                                  <Value>=Fields!{prop.Name}.Value</Value>
                                  <Style>
                                    <FontFamily>Cambria</FontFamily>
                                  </Style>
                                </TextRun>
                              </TextRuns>
                              <Style>
                                <TextAlign>Right</TextAlign>
                              </Style>
                            </Paragraph>
                          </Paragraphs>
                          <rd:DefaultName>Detail_{prop.Name}</rd:DefaultName>
                          <Style>
                            <Border>
                              <Style>Solid</Style>
                            </Border>
                            <PaddingLeft>2pt</PaddingLeft>
                            <PaddingRight>2pt</PaddingRight>
                            <PaddingTop>2pt</PaddingTop>
                            <PaddingBottom>2pt</PaddingBottom>
                          </Style>
                        </Textbox>
                      </CellContents>
                    </TablixCell>");
            }

            // 4) Put it all together as a valid RDL
            //    - We’ll embed the above pieces into an RDL “template.”
            string rdl = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Report xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition"" 
        xmlns:rd=""http://schemas.microsoft.com/SQLServer/reporting/reportdesigner"">
  <AutoRefresh>0</AutoRefresh>
  <DataSources>
    <DataSource Name=""ReportItemSchemas"">
      <ConnectionProperties>
        <DataProvider>System.Data.DataSet</DataProvider>
        <ConnectString>/* Local Connection */</ConnectString>
      </ConnectionProperties>
      <rd:DataSourceID>{Guid.NewGuid()}</rd:DataSourceID>
    </DataSource>
  </DataSources>
  <DataSets>
    <DataSet Name=""DataSet1"">
      <Query>
        <DataSourceName>ReportItemSchemas</DataSourceName>
        <CommandText>/* Local Query */</CommandText>
      </Query>
      <Fields>
{fieldsBuilder}
      </Fields>
      <rd:DataSetInfo>
        <rd:DataSetName>ReportItemSchemas</rd:DataSetName>
        <rd:SchemaPath>{EscapeXml(schemaPath)}</rd:SchemaPath>
        <rd:TableName>{type.Name}</rd:TableName>
      </rd:DataSetInfo>
    </DataSet>
  </DataSets>
  <ReportSections>
    <ReportSection>
      <Body>
        <ReportItems>
          <Tablix Name=""Tablix1"">
            <TablixBody>
              <TablixColumns>
{tablixColumnsBuilder}
              </TablixColumns>
              <TablixRows>
                <!-- Header row -->
                <TablixRow>
                  <Height>0.67938cm</Height>
                  <TablixCells>
{tablixHeaderCellsBuilder}
                  </TablixCells>
                </TablixRow>

                <!-- Detail row -->
                <TablixRow>
                  <Height>0.67938cm</Height>
                  <TablixCells>
{tablixDetailCellsBuilder}
                  </TablixCells>
                </TablixRow>
              </TablixRows>
            </TablixBody>
            <TablixColumnHierarchy>
              <TablixMembers>
{string.Join("", properties.Select(_ => "                <TablixMember />" + Environment.NewLine))}
              </TablixMembers>
            </TablixColumnHierarchy>
            <TablixRowHierarchy>
              <TablixMembers>
                <TablixMember>
                  <KeepWithGroup>After</KeepWithGroup>
                </TablixMember>
                <TablixMember>
                  <Group Name=""Details"" />
                </TablixMember>
              </TablixMembers>
            </TablixRowHierarchy>
            <DataSetName>DataSet1</DataSetName>
            <Top>2.85433cm</Top>
            <Left>0.3175cm</Left>
            <Height>1.35876cm</Height>
            <Width>7.62cm</Width>
            <Style>
              <Border>
                <Style>None</Style>
              </Border>
            </Style>
          </Tablix>

          <!-- Title Textbox -->
          <Textbox Name=""TextboxTitle"">
            <CanGrow>true</CanGrow>
            <KeepTogether>true</KeepTogether>
            <Paragraphs>
              <Paragraph>
                <TextRuns>
                  <TextRun>
                    <Value>=Parameters!Title.Value</Value>
                    <Style>
                      <FontFamily>Cambria</FontFamily>
                      <FontSize>26pt</FontSize>
                    </Style>
                  </TextRun>
                </TextRuns>
                <Style>
                  <TextAlign>Center</TextAlign>
                </Style>
              </Paragraph>
            </Paragraphs>
            <rd:DefaultName>TextboxTitle</rd:DefaultName>
            <Top>0.26141cm</Top>
            <Left>0.3175cm</Left>
            <Height>1.79062cm</Height>
            <Width>15.875cm</Width>
            <ZIndex>1</ZIndex>
            <Style>
              <Border>
                <Style>None</Style>
              </Border>
              <PaddingLeft>2pt</PaddingLeft>
              <PaddingRight>2pt</PaddingRight>
              <PaddingTop>2pt</PaddingTop>
              <PaddingBottom>2pt</PaddingBottom>
            </Style>
          </Textbox>

        </ReportItems>
        <Height>2in</Height>
        <Style />
      </Body>
      <Width>6.5in</Width>
      <Page>
        <LeftMargin>0.7874in</LeftMargin>
        <RightMargin>0.7874in</RightMargin>
        <TopMargin>0.7874in</TopMargin>
        <BottomMargin>0.7874in</BottomMargin>
        <ColumnSpacing>0.05118in</ColumnSpacing>
        <Style />
      </Page>
    </ReportSection>
  </ReportSections>

  <!-- Report Parameter: Title -->
  <ReportParameters>
    <ReportParameter Name=""Title"">
      <DataType>String</DataType>
      <Nullable>true</Nullable>
      <AllowBlank>true</AllowBlank>
      <Prompt>Title</Prompt>
      <DefaultValue>
        <Values>
          <Value>{EscapeXml(reportTitle)}</Value>
        </Values>
      </DefaultValue>
    </ReportParameter>
  </ReportParameters>

  <ReportParametersLayout>
    <GridLayoutDefinition>
      <NumberOfColumns>4</NumberOfColumns>
      <NumberOfRows>2</NumberOfRows>
      <CellDefinitions>
        <CellDefinition>
          <ColumnIndex>0</ColumnIndex>
          <RowIndex>0</RowIndex>
          <ParameterName>Title</ParameterName>
        </CellDefinition>
      </CellDefinitions>
    </GridLayoutDefinition>
  </ReportParametersLayout>

  <rd:ReportUnitType>Inch</rd:ReportUnitType>
  <rd:ReportID>{Guid.NewGuid()}</rd:ReportID>
</Report>
";
            return rdl;
        }

        private static string GetRdlTypeName(Type propType)
        {
            // If we have a known mapping, return it; otherwise default to System.String
            return RdlTypeNameMap.TryGetValue(propType, out string rdlType)
                ? rdlType
                : "System.String";
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            // minimal XML escaping
            return value
                    .Replace("&", "&amp;")
                    .Replace("\"", "&quot;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
        }
    }
}
