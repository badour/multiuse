using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using SchoolPayment.Data;
using SchoolPayment.Models;
using SchoolPayment.Services;

namespace SchoolPayment
{
    public partial class Default : Page
    {
        private readonly SchoolRepository _schools = new SchoolRepository();
        private readonly PaymentRepository _payments = new PaymentRepository();
        private readonly AlqasehClient _alqaseh = new AlqasehClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            try
            {
                BindSchools();
                BindStudentGrid(null);
            }
            catch (SqlException)
            {
                ShowError("تعذر الاتصال بقاعدة البيانات. تحقق من سلسلة الاتصال في Web.config ومن تنفيذ Database/SchoolPayment.sql.");
                btnPay.Enabled = false;
            }
        }

        protected void ddlSchool_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearStudentResult();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            litMessage.Text = string.Empty;
            ClearStudentResult();

            int schoolId;
            if (!int.TryParse(ddlSchool.SelectedValue, out schoolId) || schoolId <= 0)
            {
                ShowError("يرجى اختيار اسم المدرسة.");
                return;
            }

            var lookup = (txtStudentId.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(lookup))
            {
                ShowError("يرجى إدخال رقم الطالب ثم الضغط على بحث.");
                return;
            }

            try
            {
                var students = _schools.SearchStudents(schoolId, lookup);
                BindStudentGrid(students);

                if (students.Count == 0)
                {
                    ShowError("لم يتم العثور على طالب بهذا الرقم في المدرسة المحددة.");
                    return;
                }

                hfStudentId.Value = students[0].Id.ToString(CultureInfo.InvariantCulture);
                ShowPaymentPanel(true);
                ShowOk("تم العثور على الطالب: " + students[0].FullName);
            }
            catch (SqlException)
            {
                ShowError("تعذر البحث في قاعدة البيانات.");
            }
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            litMessage.Text = string.Empty;

            int schoolId;
            if (!int.TryParse(ddlSchool.SelectedValue, out schoolId) || schoolId <= 0)
            {
                ShowError("يرجى اختيار اسم المدرسة.");
                return;
            }

            int studentId;
            if (!int.TryParse(hfStudentId.Value, out studentId) || studentId <= 0)
            {
                ShowError("يرجى البحث عن الطالب أولاً حتى يظهر اسمه في الجدول.");
                return;
            }

            var paymentType = rblPaymentType.SelectedValue;
            if (paymentType != PaymentTypes.CurrentYearInstallment && paymentType != PaymentTypes.Debt)
            {
                ShowError("يرجى اختيار نوع الدفع.");
                return;
            }

            decimal amount;
            if (!TryParseAmount(txtAmount.Text, out amount))
            {
                ShowError("يرجى إدخال قيمة دفع صحيحة أكبر من صفر.");
                return;
            }

            var student = _schools.GetStudent(studentId);
            if (student == null || student.SchoolId != schoolId)
            {
                ShowError("بيانات الطالب غير متطابقة مع المدرسة المحددة. أعد البحث.");
                return;
            }

            var orderId = Guid.NewGuid().ToString("N");
            var payment = new PaymentRecord
            {
                OrderId = orderId,
                SchoolId = schoolId,
                StudentId = student.Id,
                PaymentType = paymentType,
                Amount = amount,
                Currency = "IQD",
                PayerName = NullIfEmpty(txtPayerName.Text),
                PayerPhone = NullIfEmpty(txtPhone.Text),
                Status = "Prepared"
            };

            _payments.Insert(payment);

            var description = string.Format(
                CultureInfo.InvariantCulture,
                "{0} - {1} - {2}",
                paymentType,
                student.FullName,
                student.StudentNumber);

            var result = _alqaseh.CreatePayment(
                amount,
                "IQD",
                orderId,
                description,
                AppUrls.GetRedirectUrl(Request),
                AppUrls.GetWebhookUrl(Request),
                null,
                "IQ",
                new Dictionary<string, object>
                {
                    { "schoolId", schoolId },
                    { "studentId", student.Id },
                    { "paymentType", paymentType }
                });

            if (!result.IsSuccess)
            {
                _payments.UpdateStatus(orderId, "Failed", result.ErrorMessage, null, null);
                ShowError(result.ErrorMessage);
                return;
            }

            _payments.UpdateGatewayIdentifiers(orderId, result.PaymentId, result.Token, "Prepared");
            Response.Redirect(result.PaymentUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void BindSchools()
        {
            ddlSchool.Items.Clear();
            ddlSchool.Items.Add(new ListItem("اختر المدرسة", ""));
            foreach (var school in _schools.GetSchools())
            {
                ddlSchool.Items.Add(new ListItem(school.Name, school.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private void BindStudentGrid(IList<Student> students)
        {
            gvStudent.DataSource = students ?? new List<Student>();
            gvStudent.DataBind();
        }

        private void ClearStudentResult()
        {
            hfStudentId.Value = string.Empty;
            ShowPaymentPanel(false);
            BindStudentGrid(null);
        }

        private void ShowPaymentPanel(bool visible)
        {
            pnlPayment.CssClass = visible ? string.Empty : "is-hidden";
        }

        private static bool TryParseAmount(string text, out decimal amount)
        {
            amount = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var raw = text.Trim().Replace(",", "").Replace("،", "").Replace(" ", "");
            return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)
                && amount > 0;
        }

        private void ShowError(string message)
        {
            litMessage.Text = "<div class=\"message error\">" + Server.HtmlEncode(message) + "</div>";
        }

        private void ShowOk(string message)
        {
            litMessage.Text = "<div class=\"message ok\">" + Server.HtmlEncode(message) + "</div>";
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
