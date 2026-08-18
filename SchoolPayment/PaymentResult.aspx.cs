using System;
using System.Globalization;
using System.Web.UI;
using SchoolPayment.Data;
using SchoolPayment.Services;

namespace SchoolPayment
{
    public partial class PaymentResult : Page
    {
        private readonly PaymentRepository _payments = new PaymentRepository();
        private readonly AlqasehClient _alqaseh = new AlqasehClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            var orderId = Request.QueryString["order_id"];
            var paymentId = Request.QueryString["payment_id"];
            var redirectStatus = Request.QueryString["status"];

            if (string.IsNullOrWhiteSpace(orderId) && string.IsNullOrWhiteSpace(paymentId))
            {
                litStatus.Text = "<div class=\"status pending\">لم يتم العثور على بيانات الدفع.</div>";
                return;
            }

            var payment = string.IsNullOrWhiteSpace(orderId) ? null : _payments.GetByOrderId(orderId);
            var gatewayInfo = _alqaseh.GetPaymentById(paymentId);
            if (gatewayInfo == null && payment != null)
            {
                gatewayInfo = _alqaseh.GetPaymentByToken(payment.PaymentToken);
            }

            var gatewayStatus = gatewayInfo != null && !string.IsNullOrWhiteSpace(gatewayInfo.Status)
                ? gatewayInfo.Status
                : redirectStatus;
            var localStatus = PaymentStatusMapper.ToLocalStatus(gatewayStatus);

            if (!string.IsNullOrWhiteSpace(orderId))
            {
                _payments.UpdateStatus(
                    orderId,
                    localStatus,
                    gatewayStatus,
                    gatewayInfo != null ? gatewayInfo.ApprovalCode : null,
                    gatewayInfo != null ? gatewayInfo.Rrn : null);
                payment = _payments.GetByOrderId(orderId);
            }
            else if (!string.IsNullOrWhiteSpace(paymentId))
            {
                _payments.UpdateStatusByPaymentId(
                    paymentId,
                    gatewayInfo != null ? gatewayInfo.OrderId : null,
                    localStatus,
                    gatewayStatus,
                    gatewayInfo != null ? gatewayInfo.ApprovalCode : null,
                    gatewayInfo != null ? gatewayInfo.Rrn : null);
            }

            RenderStatus(localStatus);

            if (payment == null)
            {
                return;
            }

            pnlSummary.Visible = true;
            litOrderId.Text = Server.HtmlEncode(payment.OrderId);
            litSchool.Text = Server.HtmlEncode(payment.SchoolName);
            litStage.Text = Server.HtmlEncode(payment.StageName);
            litStudent.Text = Server.HtmlEncode(payment.StudentName);
            litPaymentType.Text = Server.HtmlEncode(payment.PaymentType);
            litAmount.Text = string.Format(new CultureInfo("ar-IQ"), "{0:N0} {1}", payment.Amount, payment.Currency);
            litApproval.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(payment.ApprovalCode) ? "—" : payment.ApprovalCode);
        }

        private void RenderStatus(string status)
        {
            string css;
            string text;
            switch ((status ?? string.Empty).ToLowerInvariant())
            {
                case "success":
                case "succeeded":
                    css = "success";
                    text = "تم الدفع بنجاح";
                    break;
                case "failed":
                case "declined":
                    css = "failed";
                    text = "فشلت عملية الدفع";
                    break;
                default:
                    css = "pending";
                    text = "عملية الدفع قيد المعالجة: " + status;
                    break;
            }

            litStatus.Text = "<div class=\"status " + css + "\">" + Server.HtmlEncode(text) + "</div>";
        }
    }
}
