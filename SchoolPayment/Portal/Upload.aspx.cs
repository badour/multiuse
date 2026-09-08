using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.UI;
using SchoolPayment.Services;

namespace SchoolPayment.Portal
{
    public partial class Upload : Page
    {
        private const int MaxBytes = 10 * 1024 * 1024;
        private readonly StudentExcelImporter _importer = new StudentExcelImporter();

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            litMessage.Text = string.Empty;

            if (!fuExcel.HasFile)
            {
                ShowError("يرجى اختيار ملف Excel أولاً.");
                return;
            }

            var fileName = fuExcel.FileName ?? string.Empty;
            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("يُقبل ملف Excel بصيغة .xlsx فقط.");
                return;
            }

            if (fuExcel.PostedFile.ContentLength <= 0 || fuExcel.PostedFile.ContentLength > MaxBytes)
            {
                ShowError("حجم الملف غير صالح. الحد الأقصى 10 ميغابايت.");
                return;
            }

            try
            {
                using (var stream = fuExcel.PostedFile.InputStream)
                {
                    var result = _importer.Import(stream);
                    var parts = new[]
                    {
                        "أُضيف " + result.Inserted + " طالباً.",
                        "حُدّث " + result.Updated + " طالباً."
                    };

                    if (result.Errors.Count == 0 && result.Inserted == 0 && result.Updated == 0)
                    {
                        ShowError("لم يتم استيراد أي صف.");
                        return;
                    }

                    if (result.Errors.Count == 0)
                    {
                        ShowOk(string.Join(" ", parts));
                        return;
                    }

                    var errors = string.Join("<br />", result.Errors.Select(Server.HtmlEncode));
                    litMessage.Text = "<div class=\"message error\">"
                        + Server.HtmlEncode(string.Join(" ", parts))
                        + "<br />" + errors
                        + "</div>";
                }
            }
            catch (SqlException)
            {
                ShowError("تعذر حفظ البيانات في قاعدة البيانات.");
            }
            catch (InvalidDataException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            litMessage.Text = "<div class=\"message error\">" + Server.HtmlEncode(message) + "</div>";
        }

        private void ShowOk(string message)
        {
            litMessage.Text = "<div class=\"message ok\">" + Server.HtmlEncode(message) + "</div>";
        }
    }
}
