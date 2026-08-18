using System;
using System.Configuration;
using System.Web;

namespace SchoolPayment.Services
{
    public static class AppUrls
    {
        public static string GetBaseUrl(HttpRequest request)
        {
            var configured = ConfigurationManager.AppSettings["AppBaseUrl"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured.TrimEnd('/');
            }

            if (request == null || request.Url == null)
            {
                return string.Empty;
            }

            var builder = new UriBuilder(request.Url)
            {
                Path = request.ApplicationPath == "/" ? string.Empty : request.ApplicationPath,
                Query = string.Empty,
                Fragment = string.Empty
            };

            return builder.Uri.ToString().TrimEnd('/');
        }

        public static string GetRedirectUrl(HttpRequest request)
        {
            return GetBaseUrl(request) + "/PaymentResult.aspx";
        }

        public static string GetWebhookUrl(HttpRequest request)
        {
            return GetBaseUrl(request) + "/Webhook.ashx";
        }
    }
}
