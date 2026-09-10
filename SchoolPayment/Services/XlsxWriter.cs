using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace SchoolPayment.Services
{
    public static class XlsxWriter
    {
        private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        public static byte[] Write(string sheetName, IList<string> headers, IList<IList<string>> rows)
        {
            if (headers == null || headers.Count == 0)
            {
                throw new ArgumentException("Headers are required.", "headers");
            }

            var shared = new List<string>();
            var index = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var header in headers)
            {
                AddShared(shared, index, header ?? string.Empty);
            }

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    if (row == null)
                    {
                        continue;
                    }

                    foreach (var cell in row)
                    {
                        AddShared(shared, index, cell ?? string.Empty);
                    }
                }
            }

            var sheetXml = BuildSheet(headers, rows ?? new List<IList<string>>(), index);
            var sharedXml = BuildSharedStrings(shared);
            var safeSheet = string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName.Trim();
            if (safeSheet.Length > 31)
            {
                safeSheet = safeSheet.Substring(0, 31);
            }

            using (var output = new MemoryStream())
            {
                using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
                {
                    WriteEntry(zip, "[Content_Types].xml", ContentTypes);
                    WriteEntry(zip, "_rels/.rels", RootRels);
                    WriteEntry(zip, "xl/workbook.xml", BuildWorkbook(safeSheet));
                    WriteEntry(zip, "xl/_rels/workbook.xml.rels", WorkbookRels);
                    WriteEntry(zip, "xl/sharedStrings.xml", sharedXml);
                    WriteEntry(zip, "xl/worksheets/sheet1.xml", sheetXml);
                }

                return output.ToArray();
            }
        }

        private static void AddShared(IList<string> shared, IDictionary<string, int> index, string value)
        {
            if (!index.ContainsKey(value))
            {
                index[value] = shared.Count;
                shared.Add(value);
            }
        }

        private static string BuildSheet(IList<string> headers, IList<IList<string>> rows, IDictionary<string, int> index)
        {
            var sheetData = new XElement(Ns + "sheetData");
            sheetData.Add(BuildRow(1, headers, index));
            for (var i = 0; i < rows.Count; i++)
            {
                sheetData.Add(BuildRow(i + 2, rows[i] ?? new string[0], index));
            }

            var document = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Ns + "worksheet", sheetData));
            return document.ToString(SaveOptions.DisableFormatting);
        }

        private static XElement BuildRow(int rowNumber, IList<string> values, IDictionary<string, int> index)
        {
            var row = new XElement(Ns + "row", new XAttribute("r", rowNumber));
            var count = values.Count;
            for (var i = 0; i < count; i++)
            {
                var text = values[i] ?? string.Empty;
                row.Add(new XElement(
                    Ns + "c",
                    new XAttribute("r", ColumnName(i + 1) + rowNumber),
                    new XAttribute("t", "s"),
                    new XElement(Ns + "v", index[text])));
            }

            return row;
        }

        private static string BuildSharedStrings(IList<string> shared)
        {
            var root = new XElement(
                Ns + "sst",
                new XAttribute("count", shared.Count),
                new XAttribute("uniqueCount", shared.Count));
            foreach (var value in shared)
            {
                root.Add(new XElement(Ns + "si", new XElement(Ns + "t", value ?? string.Empty)));
            }

            var document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
            return document.ToString(SaveOptions.DisableFormatting);
        }

        private static string BuildWorkbook(string sheetName)
        {
            XNamespace rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            var document = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(
                    Ns + "workbook",
                    new XAttribute(XNamespace.Xmlns + "r", rel),
                    new XElement(
                        Ns + "sheets",
                        new XElement(
                            Ns + "sheet",
                            new XAttribute("name", sheetName),
                            new XAttribute("sheetId", "1"),
                            new XAttribute(rel + "id", "rId1")))));
            return document.ToString(SaveOptions.DisableFormatting);
        }

        private static void WriteEntry(ZipArchive zip, string name, string xml)
        {
            var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(xml);
            }
        }

        private static string ColumnName(int index)
        {
            var name = string.Empty;
            var current = index;
            while (current > 0)
            {
                current--;
                name = ((char)('A' + (current % 26))) + name;
                current /= 26;
            }

            return name;
        }

        private const string ContentTypes = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/><Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/></Types>";
        private const string RootRels = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>";
        private const string WorkbookRels = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/><Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/></Relationships>";
    }
}
