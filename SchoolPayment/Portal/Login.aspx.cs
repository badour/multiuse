using System;
using System.Data.SqlClient;
using System.Web.Security;
using System.Web.UI;
using SchoolPayment.Data;
using SchoolPayment.Services;

namespace SchoolPayment.Portal
{
    public partial class Login : Page
    {
        private readonly UserRepository _users = new UserRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Request.IsAuthenticated)
            {
                Response.Redirect("~/Portal/Default.aspx");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            litMessage.Text = string.Empty;
            var username = (txtUsername.Text ?? string.Empty).Trim();
            var password = txtPassword.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                ShowError("يرجى إدخال اسم المستخدم وكلمة المرور.");
                return;
            }

            try
            {
                var user = _users.GetActiveByUsername(username);
                if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                {
                    ShowError("اسم المستخدم أو كلمة المرور غير صحيحة.");
                    return;
                }

                FormsAuthentication.RedirectFromLoginPage(user.Username, chkRemember.Checked);
            }
            catch (SqlException)
            {
                ShowError("تعذر الاتصال بقاعدة البيانات. تحقق من سلسلة الاتصال ومن تنفيذ سكربت البوابة.");
            }
        }

        private void ShowError(string message)
        {
            litMessage.Text = "<div class=\"message error\">" + Server.HtmlEncode(message) + "</div>";
        }
    }
}
