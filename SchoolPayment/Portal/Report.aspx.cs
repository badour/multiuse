using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using SchoolPayment.Data;

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
            litMessage.Text = string.Empty;

            DateTime fromDate;
            DateTime toDate;
            var hasFrom = TryParseDate(txtFrom.Text, out fromDate);
            var hasTo = TryParseDate(txtTo.Text, out toDate);
            if (!string.IsNullOrWhiteSpace(txtFrom.Text) && !hasFrom)
            {
                ShowError("تاريخ البداية غير صحيح.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtTo.Text) && !hasTo)
            {
                ShowError("تاريخ النهاية غير صحيح.");
                return;
            }

            if (hasFrom && hasTo && fromDate > toDate)
            {
                ShowError("تاريخ البداية يجب أن يكون قبل تاريخ النهاية أو يساويه.");
                return;
            }

            int schoolId;
            int? schoolFilter = null;
            if (int.TryParse(ddlSchool.SelectedValue, out schoolId) && schoolId > 0)
            {
                schoolFilter = schoolId;
            }

            try
            {
                var rows = _payments.Search(
                    hasFrom ? (DateTime?)fromDate : null,
                    hasTo ? (DateTime?)toDate : null,
                    txtPincode.Text,
                    ddlStatus.SelectedValue,
                    schoolFilter);
                gvPayments.DataSource = rows;
                gvPayments.DataBind();
                litCount.Text = "عدد العمليات: " + rows.Count.ToString(CultureInfo.InvariantCulture);
            }
            catch (SqlException)
            {
                ShowError("تعذر قراءة التقرير من قاعدة البيانات.");
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
