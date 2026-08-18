using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using SchoolPayment.Data;
using SchoolPayment.Services;

namespace SchoolPayment
{
    public class WebhookHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405;
                context.Response.Write("Method Not Allowed");
                return;
            }

            string json;
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                json = reader.ReadToEnd();
            }

            var serializer = new JavaScriptSerializer();
            Dictionary<string, object> payload;
            try
            {
                payload = serializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch (ArgumentException)
            {
                context.Response.StatusCode = 400;
                context.Response.Write("Invalid JSON");
                return;
            }

            var orderId = Read(payload, "order_id");
            var paymentId = Read(payload, "payment_id");
            var gatewayStatus = Read(payload, "payment_status") ?? Read(payload, "status");
            var approval = Read(payload, "approval") ?? Read(payload, "approval_code");
            var rrn = Read(payload, "rrn");
            var localStatus = PaymentStatusMapper.ToLocalStatus(gatewayStatus);

            var repository = new PaymentRepository();
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                repository.UpdateStatus(orderId, localStatus, gatewayStatus, approval, rrn);
                if (!string.IsNullOrWhiteSpace(paymentId))
                {
                    var existing = repository.GetByOrderId(orderId);
                    if (existing != null && string.IsNullOrWhiteSpace(existing.AlqasehPaymentId))
                    {
                        repository.UpdateGatewayIdentifiers(orderId, paymentId, existing.PaymentToken, localStatus);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(paymentId))
            {
                repository.UpdateStatusByPaymentId(paymentId, orderId, localStatus, gatewayStatus, approval, rrn);
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Write("{\"received\":true}");
        }

        public bool IsReusable
        {
            get { return false; }
        }

        private static string Read(IDictionary<string, object> payload, string key)
        {
            object value;
            if (payload == null || !payload.TryGetValue(key, out value) || value == null)
            {
                return null;
            }

            var text = Convert.ToString(value);
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
    }
}
