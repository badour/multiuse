<%@ Page Title="دفع الرسوم المدرسية" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SchoolPayment.Default" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">دفع الرسوم المدرسية</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel">
        <h1>دفع الرسوم عبر فيزا أو ماستركارد</h1>
        <p class="lead">اختر المدرسة والمرحلة واسم الطالب ونوع الدفع، ثم أكمل العملية في صفحة القصّة الآمنة.</p>

        <asp:Literal ID="litMessage" runat="server" />

        <asp:UpdatePanel ID="upPayment" runat="server">
            <Triggers>
                <asp:PostBackTrigger ControlID="btnPay" />
            </Triggers>
            <ContentTemplate>
                <div class="grid">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlSchool" Text="اسم المدرسة" />
                        <asp:DropDownList ID="ddlSchool" runat="server" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="ddlSchool_SelectedIndexChanged" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlStage" Text="المرحلة الدراسية" />
                        <asp:DropDownList ID="ddlStage" runat="server" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="ddlStage_SelectedIndexChanged" />
                    </div>
                    <div class="field full">
                        <asp:Label runat="server" AssociatedControlID="ddlStudent" Text="اسم الطالب" />
                        <asp:DropDownList ID="ddlStudent" runat="server" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="ddlStudent_SelectedIndexChanged" />
                    </div>
                    <div class="field full">
                        <span class="label">نوع الدفع</span>
                        <asp:RadioButtonList ID="rblPaymentType" runat="server" RepeatLayout="Flow" CssClass="pay-types" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="rblPaymentType_SelectedIndexChanged">
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
                        <asp:Label runat="server" AssociatedControlID="txtEmail" Text="البريد الإلكتروني (اختياري)" />
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="80" />
                    </div>
                    <div class="field full">
                        <div class="amount-box">
                            <span>المبلغ المستحق</span>
                            <strong>
                                <asp:Literal ID="litAmount" runat="server" Text="—" />
                            </strong>
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <p class="note">بطاقة فيزا أو ماستركارد تُدخل في صفحة الدفع التابعة لشركة القصّة، وليست في هذا النموذج.</p>
                    <asp:Button ID="btnPay" runat="server" CssClass="btn btn-pay" Text="الدفع الآن" OnClick="btnPay_Click" />
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </section>
</asp:Content>
