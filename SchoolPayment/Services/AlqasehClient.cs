using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;

namespace SchoolPayment.Services
{
    public class AlqasehCreateResult
    {
        public bool IsSuccess { get; set; }
        public string PaymentId { get; set; }
        public string Token { get; set; }
        public string PaymentUrl { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AlqasehPaymentInfo
    {
        public string PaymentId { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string ApprovalCode { get; set; }
        public string Rrn { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
    }

    public class AlqasehClient
    {
        private static readonly HttpClient Http = CreateHttpClient();
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public AlqasehCreateResult CreatePayment(
            decimal amount,
            string currency,
            string orderId,
            string description,
            string redirectUrl,
            string webhookUrl,
            string email,
            string country,
            IDictionary<string, object> customData)
        {
            var payload = new Dictionary<string, object>
            {
                { "amount", amount },
                { "currency", currency },
                { "order_id", orderId },
                { "description", description },
                { "redirect_url", redirectUrl },
                { "transaction_type", ConfigurationManager.AppSettings["Alqaseh.TransactionType"] ?? "Retail" },
                { "country", string.IsNullOrWhiteSpace(country) ? "IQ" : country }
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                payload["email"] = email;
            }

            if (!string.IsNullOrWhiteSpace(webhookUrl))
            {
                payload["webhook_url"] = webhookUrl;
            }

            if (customData != null && customData.Count > 0)
            {
                payload["custom_data"] = customData;
            }

            var json = _serializer.Serialize(payload);
            try
            {
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = Send(Http.PostAsync(Combine(GetApiBaseUrl(), "/egw/payments/create"), content)))
                {
                    var body = ReadBody(response);
                    var data = DeserializeObject(body);
                    var nested = data.ContainsKey("data") ? data["data"] as Dictionary<string, object> : null;
                    if (!response.IsSuccessStatusCode)
                    {
                        return new AlqasehCreateResult
                        {
                            IsSuccess = false,
                            ErrorMessage = ReadError(nested ?? data, "تعذر إنشاء عملية الدفع. " + (int)response.StatusCode)
                        };
                    }

                    var token = ReadString(data, "token") ?? ReadString(nested, "token");
                    var paymentId = ReadString(data, "payment_id") ?? ReadString(nested, "payment_id");
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return new AlqasehCreateResult
                        {
                            IsSuccess = false,
                            ErrorMessage = ReadError(nested ?? data, "لم يتم استلام رمز الدفع من بوابة القصّة.")
                        };
                    }

                    return new AlqasehCreateResult
                    {
                        IsSuccess = true,
                        PaymentId = paymentId,
                        Token = token,
                        PaymentUrl = GetPaymentPageUrl(token)
                    };
                }
            }
            catch (Exception)
            {
                return new AlqasehCreateResult
                {
                    IsSuccess = false,
                    ErrorMessage = "تعذر الاتصال ببوابة الدفع. تحقق من الاتصال وإعدادات Alqaseh."
                };
            }
        }

        public AlqasehPaymentInfo GetPaymentById(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return null;
            }

            using (var response = Send(Http.GetAsync(Combine(GetApiBaseUrl(), "/egw/payments/" + Uri.EscapeDataString(paymentId)))))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return MapPaymentInfo(DeserializeObject(ReadBody(response)));
            }
        }

        public AlqasehPaymentInfo GetPaymentByToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            using (var response = Send(Http.GetAsync(Combine(GetApiBaseUrl(), "/egw/payments/info/" + Uri.EscapeDataString(token)))))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return MapPaymentInfo(DeserializeObject(ReadBody(response)));
            }
        }

        public static string GetPaymentPageUrl(string token)
        {
            var baseUrl = (ConfigurationManager.AppSettings["Alqaseh.PaymentPageBaseUrl"] ?? "https://pay-test.alqaseh.com").TrimEnd('/');
            return baseUrl + "/pay/" + Uri.EscapeDataString(token);
        }

        private AlqasehPaymentInfo MapPaymentInfo(Dictionary<string, object> data)
        {
            if (data == null)
            {
                return null;
            }

            var nested = data.ContainsKey("data") ? data["data"] as Dictionary<string, object> : null;
            var source = nested ?? data;
            return new AlqasehPaymentInfo
            {
                PaymentId = ReadString(source, "payment_id") ?? ReadString(data, "payment_id"),
                OrderId = ReadString(source, "order_id") ?? ReadString(data, "order_id"),
                Status = ReadString(source, "payment_status") ?? ReadString(source, "status") ?? ReadString(data, "payment_status"),
                ApprovalCode = ReadString(source, "approval") ?? ReadString(source, "approval_code"),
                Rrn = ReadString(source, "rrn"),
                Amount = ReadDecimal(source, "amount"),
                Currency = ReadString(source, "currency")
            };
        }

        private Dictionary<string, object> DeserializeObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, object>();
            }

            try
            {
                return _serializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch (ArgumentException)
            {
                return new Dictionary<string, object>();
            }
        }

        private static string ReadString(IDictionary<string, object> data, string key)
        {
            object value;
            if (data == null || !data.TryGetValue(key, out value) || value == null)
            {
                return null;
            }

            return Convert.ToString(value);
        }

        private static decimal? ReadDecimal(IDictionary<string, object> data, string key)
        {
            object value;
            if (data == null || !data.TryGetValue(key, out value) || value == null)
            {
                return null;
            }

            decimal amount;
            if (decimal.TryParse(Convert.ToString(value), out amount))
            {
                return amount;
            }

            return null;
        }

        private static string ReadError(IDictionary<string, object> data, string fallback)
        {
            var message = ReadString(data, "err") ?? ReadString(data, "error") ?? ReadString(data, "message");
            return string.IsNullOrWhiteSpace(message) ? fallback : message;
        }

        private static string GetApiBaseUrl()
        {
            return (ConfigurationManager.AppSettings["Alqaseh.ApiBaseUrl"] ?? "https://api-test.alqaseh.com/v1").TrimEnd('/');
        }

        private static string Combine(string baseUrl, string path)
        {
            return baseUrl + path;
        }

        private static HttpResponseMessage Send(System.Threading.Tasks.Task<HttpResponseMessage> request)
        {
            return request.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static string ReadBody(HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static HttpClient CreateHttpClient()
        {
            var clientId = ConfigurationManager.AppSettings["Alqaseh.ClientId"] ?? "public_test";
            var clientSecret = ConfigurationManager.AppSettings["Alqaseh.ClientSecret"] ?? string.Empty;
            var token = Convert.ToBase64String(Encoding.ASCII.GetBytes(clientId + ":" + clientSecret));
            var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(45);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return http;
        }
    }
}
