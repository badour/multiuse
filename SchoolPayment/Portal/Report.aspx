<%@ Page Title="تقرير الدفعات" Language="C#" MasterPageFile="~/Portal/Portal.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="SchoolPayment.Portal.Report" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">تقرير الدفعات</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel">
        <h1>تقرير عمليات الدفع</h1>
        <p class="lead">كل عمليات الدفع المسجّلة للطلاب، مع إمكانية التصفية حسب التاريخ والرمز والحالة والمدرسة.</p>
        <asp:Literal ID="litMessage" runat="server" />

        <div class="report-filters">
            <div class="report-filter-row">
                <div class="field field-from">
                    <asp:Label runat="server" AssociatedControlID="txtFrom" Text="من تاريخ" />
                    <asp:TextBox ID="txtFrom" runat="server" TextMode="Date" CssClass="filter-date" />
                </div>
                <div class="field field-to">
                    <asp:Label runat="server" AssociatedControlID="txtTo" Text="إلى تاريخ" />
                    <asp:TextBox ID="txtTo" runat="server" TextMode="Date" CssClass="filter-date" />
                </div>
                <div class="field field-pincode">
                    <asp:Label runat="server" AssociatedControlID="txtPincode" Text="رمز الطالب" />
                    <asp:TextBox ID="txtPincode" runat="server" MaxLength="200" CssClass="filter-pincode" />
                </div>
            </div>
            <div class="report-filter-row">
                <div class="field field-status">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" Text="حالة العملية" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="filter-status">
                        <asp:ListItem Text="الكل" Value="" />
                        <asp:ListItem Text="نجاح" Value="Success" />
                        <asp:ListItem Text="فشل" Value="Failed" />
                    </asp:DropDownList>
                </div>
                <div class="field field-school">
                    <asp:Label runat="server" AssociatedControlID="ddlSchool" Text="اسم المدرسة" />
                    <asp:DropDownList ID="ddlSchool" runat="server" CssClass="filter-school" />
                </div>
                <div class="field report-actions">
                    <span class="label">&nbsp;</span>
                    <div class="report-action-buttons">
                        <asp:Button ID="btnFilter" runat="server" CssClass="btn btn-search" Text="عرض التقرير" OnClick="btnFilter_Click" />
                        <asp:Button ID="btnExport" runat="server" CssClass="btn btn-pay" Text="تنزيل Excel" OnClick="btnExport_Click" />
                    </div>
                </div>
            </div>
        </div>

        <p class="note"><asp:Literal ID="litCount" runat="server" /></p>

        <div class="grid-wrap">
            <asp:GridView ID="gvPayments" runat="server" CssClass="student-grid" AutoGenerateColumns="false"
                EmptyDataText="لا توجد عمليات دفع مطابقة للتصفية."
                ShowHeaderWhenEmpty="true" GridLines="None" AllowPaging="true" PageSize="25"
                OnPageIndexChanging="gvPayments_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="CreatedAt" HeaderText="التاريخ" DataFormatString="{0:yyyy/MM/dd HH:mm}" HtmlEncode="false" />
                    <asp:BoundField DataField="SchoolName" HeaderText="المدرسة" />
                    <asp:BoundField DataField="StudentName" HeaderText="الطالب" />
                    <asp:BoundField DataField="Pincode" HeaderText="الرمز" />
                    <asp:BoundField DataField="PaymentType" HeaderText="نوع الدفع" />
                    <asp:BoundField DataField="Amount" HeaderText="المبلغ" DataFormatString="{0:N0}" HtmlEncode="false" />
                    <asp:BoundField DataField="StatusDisplay" HeaderText="الحالة" />
                    <asp:BoundField DataField="PayerName" HeaderText="ولي الأمر" />
                    <asp:BoundField DataField="PayerPhone" HeaderText="الهاتف" />
                    <asp:BoundField DataField="OrderId" HeaderText="رقم الطلب" />
                </Columns>
            </asp:GridView>
        </div>
    </section>
</asp:Content>
