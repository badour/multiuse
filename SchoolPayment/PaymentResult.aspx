<%@ Page Title="نتيجة الدفع" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PaymentResult.aspx.cs" Inherits="SchoolPayment.PaymentResult" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">نتيجة الدفع</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel result">
        <h1>نتيجة عملية الدفع</h1>
        <asp:Literal ID="litStatus" runat="server" />
        <asp:Panel ID="pnlSummary" runat="server" CssClass="summary" Visible="false">
            <div><span>رقم الطلب</span><strong><asp:Literal ID="litOrderId" runat="server" /></strong></div>
            <div><span>المدرسة</span><strong><asp:Literal ID="litSchool" runat="server" /></strong></div>
            <div><span>الطالب</span><strong><asp:Literal ID="litStudent" runat="server" /></strong></div>
            <div><span>نوع الدفع</span><strong><asp:Literal ID="litPaymentType" runat="server" /></strong></div>
            <div><span>المبلغ</span><strong><asp:Literal ID="litAmount" runat="server" /></strong></div>
            <div><span>رمز الموافقة</span><strong><asp:Literal ID="litApproval" runat="server" /></strong></div>
        </asp:Panel>
        <asp:HyperLink ID="lnkHome" runat="server" NavigateUrl="~/Default.aspx" CssClass="btn btn-pay">عودة إلى صفحة الدفع</asp:HyperLink>
    </section>
</asp:Content>
