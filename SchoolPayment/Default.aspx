<%@ Page Title="دفع الرسوم المدرسية" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SchoolPayment.Default" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">دفع الرسوم المدرسية</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel">
        <h1>دفع الرسوم عبر فيزا أو ماستركارد</h1>
        <p class="lead">اختر المدرسة، أدخل رقم الطالب ثم اضغط بحث. إذا ظهر اسم الطالب يمكنك إكمال الدفع.</p>

        <asp:Literal ID="litMessage" runat="server" />

        <asp:UpdatePanel ID="upPayment" runat="server">
            <Triggers>
                <asp:PostBackTrigger ControlID="btnPay" />
            </Triggers>
            <ContentTemplate>
                <asp:HiddenField ID="hfStudentId" runat="server" />

                <div class="grid">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlSchool" Text="اسم المدرسة" />
                        <asp:DropDownList ID="ddlSchool" runat="server" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="ddlSchool_SelectedIndexChanged" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtStudentId" Text="رقم الطالب" />
                        <div class="search-input">
                            <asp:TextBox ID="txtStudentId" runat="server" MaxLength="50" />
                            <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-search" Text="بحث" CausesValidation="false" OnClick="btnSearch_Click" />
                        </div>
                    </div>
                </div>

                <div class="grid-wrap">
                    <asp:GridView ID="gvStudent" runat="server" CssClass="student-grid" AutoGenerateColumns="false"
                        EmptyDataText="لا توجد بيانات طالب. اختر المدرسة وأدخل رقم الطالب ثم اضغط بحث."
                        ShowHeaderWhenEmpty="true" DataKeyNames="Id" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="الرقم" />
                            <asp:BoundField DataField="FullName" HeaderText="الاسم" />
                            <asp:BoundField DataField="Pincode" HeaderText="الرمز" />
                            <asp:BoundField DataField="TotalCost" HeaderText="الكلفة الكلية" DataFormatString="{0:N0}" HtmlEncode="false" />
                            <asp:BoundField DataField="PaidCost" HeaderText="المدفوع" DataFormatString="{0:N0}" HtmlEncode="false" />
                            <asp:BoundField DataField="RemainCost" HeaderText="المتبقي" DataFormatString="{0:N0}" HtmlEncode="false" />
                            <asp:BoundField DataField="DebtCost" HeaderText="الديون" DataFormatString="{0:N0}" HtmlEncode="false" />
                            <asp:BoundField DataField="DiscountCost" HeaderText="الخصم" DataFormatString="{0:N0}" HtmlEncode="false" />
                        </Columns>
                    </asp:GridView>
                </div>

                <asp:Panel ID="pnlPayment" runat="server" CssClass="is-hidden">
                    <div class="grid">
                        <div class="field full">
                            <span class="label">نوع الدفع</span>
                            <asp:RadioButtonList ID="rblPaymentType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="pay-types" CausesValidation="false">
                                <asp:ListItem Text="اقساط عام حالي" Value="اقساط عام حالي" Selected="True" />
                                <asp:ListItem Text="ديون" Value="ديون" />
                            </asp:RadioButtonList>
                        </div>
                        <div class="field">
                            <asp:Label runat="server" AssociatedControlID="txtPayerName" Text="اسم ولي الأمر" />
                            <asp:TextBox ID="txtPayerName" runat="server" MaxLength="200" />
                        </div>
                        <div class="field">
                            <asp:Label runat="server" AssociatedControlID="txtPhone" Text="رقم الهاتف" />
                            <asp:TextBox ID="txtPhone" runat="server" MaxLength="30" />
                        </div>
                        <div class="field full">
                            <asp:Label runat="server" AssociatedControlID="txtAmount" Text="قيمة الدفع" />
                            <div class="amount-input-wrap">
                                <asp:TextBox ID="txtAmount" runat="server" CssClass="amount-input" MaxLength="18" />
                                <span class="amount-suffix">د.ع</span>
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <p class="note">بطاقة فيزا أو ماستركارد تُدخل في صفحة الدفع التابعة لشركة القصّة، وليست في هذا النموذج.</p>
                        <asp:Button ID="btnPay" runat="server" CssClass="btn btn-pay" Text="الدفع الآن" OnClick="btnPay_Click" />
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </section>
</asp:Content>
