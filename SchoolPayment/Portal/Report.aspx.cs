using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using SchoolPayment.Data;
using SchoolPayment.Models;
using SchoolPayment.Services;

namespace SchoolPayment.Portal
{
    public partial class Report : Page
    {
        private readonly SchoolRepository _schools = new SchoolRepository();
        private readonly PaymentRepository _payments = new PaymentRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            BindSchools();
            BindGrid();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            gvPayments.PageIndex = 0;
            BindGrid();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            IList<PaymentRecord> rows;
            if (!TryLoadFilteredPayments(out rows))
            {
                return;
            }

            if (rows.Count == 0)
            {
                ShowError("لا توجد عمليات مطابقة للتصفية لتنزيلها.");
                return;
            }

            var headers = new[]
            {
                "التاريخ", "المدرسة", "الطالب", "الرمز", "نوع الدفع",
                "المبلغ", "العملة", "الحالة", "ولي الأمر", "الهاتف", "رقم الطلب"
            };
            var data = new List<IList<string>>(rows.Count);
            foreach (var row in rows)
            {
                data.Add(new[]
                {
                    row.CreatedAt.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture),
                    row.SchoolName ?? string.Empty,
                    row.StudentName ?? string.Empty,
                    row.Pincode ?? string.Empty,
                    row.PaymentType ?? string.Empty,
                    row.Amount.ToString("0.##", CultureInfo.InvariantCulture),
                    row.Currency ?? string.Empty,
                    row.StatusDisplay ?? row.Status ?? string.Empty,
                    row.PayerName ?? string.Empty,
                    row.PayerPhone ?? string.Empty,
                    row.OrderId ?? string.Empty
                });
            }

            var bytes = XlsxWriter.Write("الدفعات", headers, data);
            var fileName = "Payments_" + DateTime.Now.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture) + ".xlsx";

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void gvPayments_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPayments.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        private void BindSchools()
        {
            ddlSchool.Items.Clear();
            ddlSchool.Items.Add(new ListItem("الكل", ""));
            foreach (var school in _schools.GetAllSchools())
            {
                ddlSchool.Items.Add(new ListItem(school.Name, school.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private void BindGrid()
        {
            IList<PaymentRecord> rows;
            if (!TryLoadFilteredPayments(out rows))
            {
                return;
            }

            gvPayments.DataSource = rows;
            gvPayments.DataBind();
            litCount.Text = "عدد العمليات: " + rows.Count.ToString(CultureInfo.InvariantCulture);
        }

        private bool TryLoadFilteredPayments(out IList<PaymentRecord> rows)
        {
            rows = new List<PaymentRecord>();
            litMessage.Text = string.Empty;

            DateTime fromDate;
            DateTime toDate;
            var hasFrom = TryParseDate(txtFrom.Text, out fromDate);
            var hasTo = TryParseDate(txtTo.Text, out toDate);
            if (!string.IsNullOrWhiteSpace(txtFrom.Text) && !hasFrom)
            {
                ShowError("تاريخ البداية غير صحيح.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtTo.Text) && !hasTo)
            {
                ShowError("تاريخ النهاية غير صحيح.");
                return false;
            }

            if (hasFrom && hasTo && fromDate > toDate)
            {
                ShowError("تاريخ البداية يجب أن يكون قبل تاريخ النهاية أو يساويه.");
                return false;
            }

            int schoolId;
            int? schoolFilter = null;
            if (int.TryParse(ddlSchool.SelectedValue, out schoolId) && schoolId > 0)
            {
                schoolFilter = schoolId;
            }

            try
            {
                rows = _payments.Search(
                    hasFrom ? (DateTime?)fromDate : null,
                    hasTo ? (DateTime?)toDate : null,
                    txtPincode.Text,
                    ddlStatus.SelectedValue,
                    schoolFilter);
                return true;
            }
            catch (SqlException)
            {
                ShowError("تعذر قراءة التقرير من قاعدة البيانات.");
                return false;
            }
        }

        private static bool TryParseDate(string text, out DateTime date)
        {
            return DateTime.TryParseExact(
                (text ?? string.Empty).Trim(),
                new[] { "yyyy-MM-dd", "dd/MM/yyyy" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private void ShowError(string message)
        {
            litMessage.Text = "<div class=\"message error\">" + Server.HtmlEncode(message) + "</div>";
            gvPayments.DataSource = null;
            gvPayments.DataBind();
            litCount.Text = string.Empty;
        }
    }
}
