using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace SchoolPayment.Services
{
    public static class XlsxReader
    {
        private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PkgRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static IList<IDictionary<string, string>> ReadFirstSheet(Stream input)
        {
            using (var zip = new ZipArchive(input, ZipArchiveMode.Read, true))
            {
                var workbookEntry = FindEntry(zip, "xl/workbook.xml");
                if (workbookEntry == null)
                {
                    throw new InvalidDataException("ملف Excel غير صالح.");
                }

                var relsEntry = FindEntry(zip, "xl/_rels/workbook.xml.rels");
                if (relsEntry == null)
                {
                    throw new InvalidDataException("ملف Excel غير صالح.");
                }

                var sheetPath = ResolveFirstWorksheetPath(ReadXml(workbookEntry), ReadXml(relsEntry));
                var sheetEntry = FindEntry(zip, sheetPath);
                if (sheetEntry == null)
                {
                    throw new InvalidDataException("تعذر قراءة ورقة العمل في ملف Excel.");
                }

                var sharedStrings = ReadSharedStrings(zip);
                return ParseSheet(ReadXml(sheetEntry), sharedStrings);
            }
        }

        private static IList<IDictionary<string, string>> ParseSheet(XDocument sheet, IList<string> sharedStrings)
        {
            var rows = new List<IDictionary<string, string>>();
            IDictionary<int, string> headers = null;
            var sheetData = sheet.Root == null ? null : sheet.Root.Element(Ns + "sheetData");
            if (sheetData == null)
            {
                return rows;
            }

            foreach (var row in sheetData.Elements(Ns + "row"))
            {
                var cells = ReadRowCells(row, sharedStrings);
                if (cells.Count == 0)
                {
                    continue;
                }

                if (headers == null)
                {
                    headers = cells;
                    continue;
                }

                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in cells)
                {
                    string header;
                    if (!headers.TryGetValue(cell.Key, out header) || string.IsNullOrWhiteSpace(header))
                    {
                        continue;
                    }

                    var canonical = CanonicalHeader(header);
                    if (string.IsNullOrEmpty(canonical))
                    {
                        continue;
                    }

                    values[canonical] = cell.Value;
                }

                if (values.Count > 0)
                {
                    rows.Add(values);
                }
            }

            return rows;
        }

        private static IDictionary<int, string> ReadRowCells(XElement row, IList<string> sharedStrings)
        {
            var cells = new Dictionary<int, string>();
            foreach (var cell in row.Elements(Ns + "c"))
            {
                var reference = (string)cell.Attribute("r");
                var index = ColumnIndex(reference);
                if (index <= 0)
                {
                    continue;
                }

                var value = CellText(cell, sharedStrings);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    cells[index] = value.Trim();
                }
            }

            return cells;
        }

        private static string CellText(XElement cell, IList<string> sharedStrings)
        {
            var type = (string)cell.Attribute("t");
            if (string.Equals(type, "s", StringComparison.OrdinalIgnoreCase))
            {
                int index;
                var raw = ValueText(cell);
                if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
                    && index >= 0 && index < sharedStrings.Count)
                {
                    return sharedStrings[index];
                }

                return null;
            }

            if (string.Equals(type, "inlineStr", StringComparison.OrdinalIgnoreCase))
            {
                var inline = cell.Element(Ns + "is");
                return inline == null ? null : string.Concat(inline.Descendants(Ns + "t").Select(t => (string)t));
            }

            return ValueText(cell);
        }

        private static string ValueText(XElement cell)
        {
            var value = cell.Element(Ns + "v");
            return value == null ? null : (string)value;
        }

        private static IList<string> ReadSharedStrings(ZipArchive zip)
        {
            var entry = FindEntry(zip, "xl/sharedStrings.xml");
            if (entry == null)
            {
                return new List<string>();
            }

            var document = ReadXml(entry);
            var list = new List<string>();
            if (document.Root == null)
            {
                return list;
            }

            foreach (var item in document.Root.Elements(Ns + "si"))
            {
                list.Add(string.Concat(item.Descendants(Ns + "t").Select(t => (string)t)));
            }

            return list;
        }

        private static string ResolveFirstWorksheetPath(XDocument workbook, XDocument rels)
        {
            if (workbook.Root == null)
            {
                throw new InvalidDataException("ملف Excel غير صالح.");
            }

            var sheets = workbook.Root.Element(Ns + "sheets");
            var first = sheets == null ? null : sheets.Elements(Ns + "sheet").FirstOrDefault();
            var relId = first == null ? null : (string)first.Attribute(RelNs + "id");
            if (string.IsNullOrEmpty(relId) || rels.Root == null)
            {
                throw new InvalidDataException("تعذر تحديد ورقة العمل.");
            }

            foreach (var relationship in rels.Root.Elements(PkgRelNs + "Relationship"))
            {
                if (!string.Equals((string)relationship.Attribute("Id"), relId, StringComparison.Ordinal))
                {
                    continue;
                }

                var target = (string)relationship.Attribute("Target");
                if (string.IsNullOrWhiteSpace(target))
                {
                    break;
                }

                target = target.Replace('\\', '/').TrimStart('/');
                if (target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
                {
                    return target;
                }

                return "xl/" + target;
            }

            throw new InvalidDataException("تعذر تحديد ورقة العمل.");
        }

        public static string CanonicalHeader(string header)
        {
            var raw = (header ?? string.Empty).Trim();
            var compact = raw.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
            switch (compact)
            {
                case "school":
                case "schoolname":
                case "schoolid":
                case "المدرسة":
                case "اسمالمدرسة":
                    return "school";
                case "fullname":
                case "name":
                case "studentname":
                case "الاسم":
                case "اسمالطالب":
                    return "fullname";
                case "studentnumber":
                case "studentno":
                case "studentid":
                case "رقمالطالب":
                    return "studentnumber";
                case "pincode":
                case "pin":
                case "الرمز":
                case "البينكود":
                    return "pincode";
                case "totalcost":
                case "total":
                case "الكلفةالكلية":
                    return "totalcost";
                case "paidcost":
                case "paid":
                case "المدفوع":
                    return "paidcost";
                case "remaincost":
                case "remain":
                case "المتبقي":
                    return "remaincost";
                case "debtcost":
                case "debt":
                case "الديون":
                    return "debtcost";
                case "discountcost":
                case "discount":
                case "الخصم":
                    return "discountcost";
                default:
                    return compact;
            }
        }

        private static int ColumnIndex(string cellRef)
        {
            if (string.IsNullOrEmpty(cellRef))
            {
                return 0;
            }

            var index = 0;
            foreach (var ch in cellRef)
            {
                if (ch >= 'A' && ch <= 'Z')
                {
                    index = (index * 26) + (ch - 'A' + 1);
                }
                else if (ch >= 'a' && ch <= 'z')
                {
                    index = (index * 26) + (ch - 'a' + 1);
                }
                else
                {
                    break;
                }
            }

            return index;
        }

        private static ZipArchiveEntry FindEntry(ZipArchive zip, string name)
        {
            return zip.Entries.FirstOrDefault(e =>
                string.Equals(e.FullName.Replace('\\', '/'), name, StringComparison.OrdinalIgnoreCase));
        }

        private static XDocument ReadXml(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }
    }
}
