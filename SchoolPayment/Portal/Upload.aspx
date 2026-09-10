<%@ Page Title="رفع بيانات الطلاب" Language="C#" MasterPageFile="~/Portal/Portal.Master" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="SchoolPayment.Portal.Upload" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">رفع بيانات الطلاب</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel">
        <h1>رفع ملف Excel إلى جدول الطلاب</h1>
        <p class="lead">اختر ملف <strong>.xlsx</strong> صفّه الأول عناوين الأعمدة. يتم تحديث الطالب إذا وُجد نفس الرمز أو رقم الطالب في نفس المدرسة، وإلا يُضاف صف جديد.</p>
        <asp:Literal ID="litMessage" runat="server" />

        <div class="field">
            <asp:Label runat="server" AssociatedControlID="fuExcel" Text="ملف Excel" />
            <asp:FileUpload ID="fuExcel" runat="server" Accept=".xlsx" />
        </div>
        <div class="actions">
            <asp:HyperLink ID="lnkTemplate" runat="server" CssClass="btn btn-search" NavigateUrl="~/Content/StudentsUploadTemplate.xlsx">تحميل النموذج</asp:HyperLink>
            <asp:Button ID="btnUpload" runat="server" CssClass="btn btn-pay" Text="رفع الملف" OnClick="btnUpload_Click" />
        </div>

        <div class="note upload-help">
            الأعمدة المدعومة: المدرسة، الاسم، رقم الطالب، الرمز، الكلفة الكلية، المدفوع، المتبقي، الديون، الخصم.
            يجب أن يطابق اسم المدرسة اسماً موجوداً في جدول المدارس. الرمز أو رقم الطالب مطلوب.
        </div>
    </section>
</asp:Content>
