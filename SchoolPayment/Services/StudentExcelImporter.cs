using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SchoolPayment.Data;
using SchoolPayment.Models;

namespace SchoolPayment.Services
{
    public class StudentImportResult
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public IList<string> Errors { get; set; }

        public StudentImportResult()
        {
            Errors = new List<string>();
        }
    }

    public class StudentExcelImporter
    {
        private const int MaxRows = 5000;
        private readonly SchoolRepository _schools = new SchoolRepository();

        public StudentImportResult Import(Stream excelStream)
        {
            var result = new StudentImportResult();
            IList<IDictionary<string, string>> rows;
            try
            {
                rows = XlsxReader.ReadFirstSheet(excelStream);
            }
            catch (InvalidDataException ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }

            if (rows.Count == 0)
            {
                result.Errors.Add("الملف لا يحتوي على صفوف بيانات بعد صف العناوين.");
                return result;
            }

            if (rows.Count > MaxRows)
            {
                result.Errors.Add("عدد الصفوف أكبر من " + MaxRows + ". قسّم الملف ثم أعد المحاولة.");
                return result;
            }

            var schoolLookup = BuildSchoolLookup();
            var rowNumber = 1;
            foreach (var row in rows)
            {
                rowNumber++;
                try
                {
                    var outcome = UpsertRow(row, schoolLookup);
                    if (outcome == "inserted")
                    {
                        result.Inserted++;
                    }
                    else
                    {
                        result.Updated++;
                    }
                }
                catch (Exception ex)
                {
                    if (result.Errors.Count < 50)
                    {
                        result.Errors.Add("الصف " + rowNumber + ": " + ex.Message);
                    }
                }
            }

            return result;
        }

        private string UpsertRow(IDictionary<string, string> row, IDictionary<string, School> schoolLookup)
        {
            var schoolValue = Get(row, "school");
            var fullName = Get(row, "fullname");
            if (string.IsNullOrWhiteSpace(schoolValue))
            {
                throw new InvalidOperationException("اسم المدرسة مطلوب.");
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new InvalidOperationException("اسم الطالب مطلوب.");
            }

            var school = FindSchool(schoolValue, schoolLookup);
            if (school == null)
            {
                throw new InvalidOperationException("المدرسة غير موجودة: " + schoolValue);
            }

            var student = new Student
            {
                SchoolId = school.Id,
                FullName = fullName.Trim(),
                StudentNumber = NullIfEmpty(Get(row, "studentnumber")),
                Pincode = NullIfEmpty(Get(row, "pincode")),
                TotalCost = ParseAmount(Get(row, "totalcost")),
                PaidCost = ParseAmount(Get(row, "paidcost")),
                RemainCost = ParseAmount(Get(row, "remaincost")),
                DebtCost = ParseAmount(Get(row, "debtcost")),
                DiscountCost = ParseAmount(Get(row, "discountcost"))
            };

            if (string.IsNullOrWhiteSpace(student.Pincode) && string.IsNullOrWhiteSpace(student.StudentNumber))
            {
                throw new InvalidOperationException("أدخل الرمز أو رقم الطالب.");
            }

            return _schools.UpsertStudent(student);
        }

        private IDictionary<string, School> BuildSchoolLookup()
        {
            var map = new Dictionary<string, School>(StringComparer.OrdinalIgnoreCase);
            foreach (var school in _schools.GetAllSchools())
            {
                if (!string.IsNullOrWhiteSpace(school.Name) && !map.ContainsKey(school.Name.Trim()))
                {
                    map[school.Name.Trim()] = school;
                }

                map[school.Id.ToString(CultureInfo.InvariantCulture)] = school;
            }

            return map;
        }

        private static School FindSchool(string value, IDictionary<string, School> lookup)
        {
            School school;
            if (lookup.TryGetValue(value.Trim(), out school))
            {
                return school;
            }

            return null;
        }

        private static string Get(IDictionary<string, string> row, string key)
        {
            string value;
            return row.TryGetValue(key, out value) ? value : null;
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static decimal ParseAmount(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            var raw = NormalizeDigits(value).Replace(",", "").Replace("،", "").Replace(" ", "");
            decimal amount;
            if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) || amount < 0)
            {
                throw new InvalidOperationException("قيمة رقمية غير صحيحة: " + value);
            }

            return amount;
        }

        private static string NormalizeDigits(string value)
        {
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] >= '\u0660' && chars[i] <= '\u0669')
                {
                    chars[i] = (char)('0' + (chars[i] - '\u0660'));
                }
                else if (chars[i] >= '\u06F0' && chars[i] <= '\u06F9')
                {
                    chars[i] = (char)('0' + (chars[i] - '\u06F0'));
                }
            }

            return new string(chars);
        }
    }
}
