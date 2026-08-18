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
                BindStages();
                BindStudents();
            }
            catch (SqlException)
            {
                ShowError("تعذر الاتصال بقاعدة البيانات. تحقق من سلسلة الاتصال في Web.config ومن تنفيذ Database/SchoolPayment.sql.");
                btnPay.Enabled = false;
            }
        }

        protected void ddlSchool_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindStages();
            BindStudents();
        }

        protected void ddlStage_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindStudents();
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            litMessage.Text = string.Empty;

            int schoolId;
            int stageId;
            int studentId;
            if (!TryGetSelectedIds(out schoolId, out stageId, out studentId))
            {
                ShowError("يرجى اختيار المدرسة والمرحلة واسم الطالب.");
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
            if (student == null || student.SchoolId != schoolId || student.StageId != stageId)
            {
                ShowError("بيانات الطالب غير متطابقة مع المدرسة والمرحلة المحددتين.");
                return;
            }

            var orderId = Guid.NewGuid().ToString("N");
            var payment = new PaymentRecord
            {
                OrderId = orderId,
                SchoolId = schoolId,
                StageId = stageId,
                StudentId = studentId,
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
                    { "stageId", stageId },
                    { "studentId", studentId },
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

        private void BindStages()
        {
            ddlStage.Items.Clear();
            ddlStage.Items.Add(new ListItem("اختر المرحلة", ""));

            int schoolId;
            if (!int.TryParse(ddlSchool.SelectedValue, out schoolId) || schoolId <= 0)
            {
                return;
            }

            foreach (var stage in _schools.GetStagesBySchool(schoolId))
            {
                ddlStage.Items.Add(new ListItem(stage.Name, stage.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private void BindStudents()
        {
            ddlStudent.Items.Clear();
            ddlStudent.Items.Add(new ListItem("اختر الطالب", ""));

            int schoolId;
            int stageId;
            if (!int.TryParse(ddlSchool.SelectedValue, out schoolId) || !int.TryParse(ddlStage.SelectedValue, out stageId))
            {
                return;
            }

            foreach (var student in _schools.GetStudents(schoolId, stageId))
            {
                var text = string.IsNullOrWhiteSpace(student.StudentNumber)
                    ? student.FullName
                    : student.FullName + " — " + student.StudentNumber;
                ddlStudent.Items.Add(new ListItem(text, student.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private bool TryGetSelectedIds(out int schoolId, out int stageId, out int studentId)
        {
            schoolId = 0;
            stageId = 0;
            studentId = 0;

            var hasSchool = int.TryParse(ddlSchool.SelectedValue, out schoolId);
            var hasStage = int.TryParse(ddlStage.SelectedValue, out stageId);
            var hasStudent = int.TryParse(ddlStudent.SelectedValue, out studentId);

            return hasSchool && hasStage && hasStudent
                && schoolId > 0 && stageId > 0 && studentId > 0;
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

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
