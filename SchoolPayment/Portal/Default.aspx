<%@ Page Title="بوابة الإدارة" Language="C#" MasterPageFile="~/Portal/Portal.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SchoolPayment.Portal.Default" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">بوابة الإدارة</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel">
        <h1>مرحباً</h1>
        <p class="lead">اختر مهمة من بوابة الإدارة.</p>
        <div class="portal-cards">
            <a class="portal-card" href="Upload.aspx">
                <strong>رفع بيانات الطلاب</strong>
                <span>استيراد صفوف جدول الطلاب من ملف Excel.</span>
            </a>
            <a class="portal-card" href="Report.aspx">
                <strong>تقرير الدفعات</strong>
                <span>عرض عمليات الدفع الناجحة والفاشلة مع التصفية.</span>
            </a>
        </div>
    </section>
</asp:Content>
