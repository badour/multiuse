using System;
using System.Web.Security;
using System.Web.UI;

namespace SchoolPayment.Portal
{
    public partial class PortalMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var authenticated = Request.IsAuthenticated;
            pnlNav.Visible = authenticated;
            if (authenticated)
            {
                litUser.Text = Server.HtmlEncode(Context.User.Identity.Name);
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Response.Redirect("~/Portal/Login.aspx");
        }
    }
}
