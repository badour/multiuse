<%@ Page Title="تسجيل الدخول" Language="C#" MasterPageFile="~/Portal/Portal.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SchoolPayment.Portal.Login" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">تسجيل الدخول</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel login-panel">
        <h1>تسجيل الدخول</h1>
        <p class="lead">أدخل بيانات المستخدم المخزّنة في قاعدة بيانات الدفع.</p>
        <asp:Literal ID="litMessage" runat="server" />
        <div class="field">
            <asp:Label runat="server" AssociatedControlID="txtUsername" Text="اسم المستخدم" />
            <asp:TextBox ID="txtUsername" runat="server" MaxLength="80" />
        </div>
        <div class="field">
            <asp:Label runat="server" AssociatedControlID="txtPassword" Text="كلمة المرور" />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" MaxLength="200" />
        </div>
        <div class="field remember">
            <asp:CheckBox ID="chkRemember" runat="server" Text="تذكرني على هذا الجهاز" />
        </div>
        <div class="actions">
            <asp:Button ID="btnLogin" runat="server" CssClass="btn btn-pay" Text="دخول" OnClick="btnLogin_Click" />
        </div>
    </section>
</asp:Content>
