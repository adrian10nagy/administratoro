<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" MaintainScrollPositionOnPostback="true"  Inherits="Admin.Associations.Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <span class="associationsLabels">Denumire asociație:</span>
    <asp:TextBox runat="server" ID="txtAssociationName" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="bntAssociationNameChange" OnClick="bntAssociationNameChange_Click" Text="Modifică" />
    <br />

    <span class="associationsLabels">Adresa:</span>
    <asp:TextBox runat="server" ID="txtAssociationAddress" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="btnAssociationAddress" OnClick="btnAssociationAddress_Click" Text="Modifică" />
    <br />

    <span class="associationsLabels">Cod fiscal:</span>
    <asp:TextBox runat="server" ID="txtAssociationFiscalCode" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="btnAssociationFiscalCode" OnClick="btnAssociationFiscalCode_Click" Text="Modifică" />
    <br />
    
    <span class="associationsLabels">% Penelizarea pe zi de întârziere:</span>
    <asp:TextBox runat="server" ID="txtPenaltyRate" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="btnPenaltyRate" OnClick="btnPenaltyRate_OnClick" Text="Modifică" />
    <br />

    <span class="associationsLabels">Cont bancar:</span>
    <asp:TextBox runat="server" ID="txtAssociationBanckAccount" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="btnAssociationBanckAccount" OnClick="btnAssociationBanckAccount_Click" Text="Modifică" />
    <br />
    <div>
        <asp:RadioButtonList ID="drpAssociationEqualIndiviza" CssClass="AssociationsNewRadioBtns" runat="server" AutoPostBack="true" RepeatLayout="Flow" OnSelectedIndexChanged="associationEqualIndiviza_SelectedIndexChanged" RepeatDirection="Horizontal">
            <asp:ListItem Text="Nu" Value="0" Selected="True" />
            <asp:ListItem Text="Da" Value="1" />
        </asp:RadioButtonList>
        <span>Cotă de indiviză egală la toate apatamentele</span>
        <asp:TextBox runat="server" ID="txtAssociationCotaIndivizaApartments" Visible="false" placeholder="ex: 1,16" Enabled="false"></asp:TextBox>
        %
    <asp:Button runat="server" ID="btnAssociationEqualIndiviza" OnClick="btnAssociationEqualIndiviza_Click" Text="Modifică" Visible="false" />
    </div>
    <br />
    <div>
        <span class="associationsLabels">Coloana de rotunjiri vizibilă în lista de cheltuieli:</span>
        <asp:RadioButtonList ID="rbHasRoundup" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rbHasRoundup_SelectedIndexChanged"
            RepeatLayout="Flow" RepeatDirection="Horizontal">
            <asp:ListItem Text="Nu" Value="0" />
            <asp:ListItem Text="Da" Value="1" />
        </asp:RadioButtonList>
    </div>
    <div>
        <span class="associationsLabels">Bloc împărțit pe scări:</span>
        <asp:RadioButtonList ID="associationStairs" runat="server" AutoPostBack="true" OnSelectedIndexChanged="associationStairs_SelectedIndexChanged"
            RepeatLayout="Flow" RepeatDirection="Horizontal">
            <asp:ListItem Text="Nu" Value="0" />
            <asp:ListItem Text="Da" Value="1" />
        </asp:RadioButtonList>
    </div>
    <h3 runat="server" id="gvStaircasesMessage" class="textAlignCenter">Scările asociației</h3>
    <asp:GridView ID="gvStaircases" runat="server" AutoGenerateColumns="true"
        OnRowEditing="gvStaircases_RowEditing" OnRowUpdating="gvStaircases_RowUpdating" CausesValidation="false"
        OnRowCancelingEdit="gvStaircases_RowCancelingEdit" OnRowDataBound="gvStaircases_RowDataBound"
        CssClass="table table-striped table-bordered dt-responsive nowrap associationsIndexStairs">
        <Columns>
            <asp:CommandField ShowEditButton="True" EditText="Modifică"
                ItemStyle-Font-Size="8pt" ItemStyle-Width="30px" ButtonType="Link"></asp:CommandField>
        </Columns>
    </asp:GridView>

    <asp:Panel ID="newStairCasePanel" runat="server" Visible="false">
        <asp:Label runat="server" Text="Nume:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
        <asp:TextBox runat="server" ID="txtAssociationStairCaseName" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:TextBox>
        <asp:Label runat="server" Text="Cota indiviză:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
        <asp:TextBox runat="server" ID="txtAssociationStairCaseIndiviza" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:TextBox>
    </asp:Panel>
    <asp:Button runat="server" ID="btnAssociationStairCasesNew" Text="Adaugă scară" OnClick="btnAssociationstairCasesNew_Click" CssClass="col-md-2 col-xs-2 col-xs-offset-3" />
    <br />
    <br />
    <br />
    <br />

    <h3 class="textAlignCenter">Contoarele asociației</h3>
    <asp:GridView ID="gvCounters" runat="server" AutoGenerateColumns="true"
        OnRowEditing="gvCounters_RowEditing" OnRowUpdating="gvCounters_RowUpdating" CausesValidation="false"
        OnRowCancelingEdit="gvCounters_RowCancelingEdit" OnRowDataBound="gvCounters_RowDataBound" DataKeyNames="Id"
        CssClass="table table-striped table-bordered dt-responsive nowrap associationsIndexStairs">
        <Columns>
            <asp:CommandField ShowEditButton="True" EditText="Modifică"
                ItemStyle-Font-Size="8pt" ItemStyle-Width="30px" ButtonType="Link"></asp:CommandField>
            <asp:TemplateField HeaderText="Cheltuială" Visible="true">
                <ItemTemplate>
                    <asp:Label ID="lblExpense" runat="server" Text='<%# Eval("Id_Expense") %>' Visible="false" />
                    <asp:DropDownList ID="ddlExpense" runat="server" Enabled="false">
                    </asp:DropDownList>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField HeaderText="Serie Contor" DataField="Value" />
            <asp:BoundField HeaderText="Scară" DataField="AssociationCounterStairCaseIdsString" />
            <asp:TemplateField HeaderText="Scară">
                <ItemTemplate>
                    <asp:Label ID="lblStairCaseId" runat="server" Text='<%# Eval("AssociationCounterStairCaseIdsString") %>' Visible="false" />
                    <asp:CheckBoxList ID="chbStairCase"  runat="server" Enabled="true">
                    </asp:CheckBoxList>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <asp:Panel ID="newCounter" runat="server" Visible="false">
        <div class="col-md-12 col-cs-12">
            <asp:Label runat="server" Text="Cheltuială:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
            <asp:DropDownList runat="server" ID="drpAssociationCounterTypeNew" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:DropDownList>
        </div>
        <div class="col-md-12 col-cs-12">
            <asp:Label runat="server" Text="Serie contor:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
            <asp:TextBox runat="server" ID="txtAssociationCounterValueNew" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:TextBox>
        </div>
        <div class="col-md-12 col-cs-12">
            <asp:Label runat="server" Text="Scară:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
            <asp:CheckBoxList runat="server" ID="chbAssociationStairs" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:CheckBoxList>
        </div>
    </asp:Panel>
    <asp:Button runat="server" ID="btnAssociationCountersNew" Text="Adaugă contor" OnClick="btnAssociationCountersNew_Click" CssClass="col-md-2 col-xs-2 col-xs-offset-3" />
    <br /><br />

    <div class="row">
        <div class="col-md-6 col-xd-12">
           <span class="associationsLabels">Fond reparații:</span>
            <asp:TextBox runat="server" ID="tbFondReparatii" Enabled="false"></asp:TextBox>
            <%--<asp:Button runat="server" ID="Button1" OnClick="btnAssociationFiscalCode_Click" Text="Modifică" />--%>
            <br />
        </div>
        <div class="col-md-6 col-xd-12">
           <span class="associationsLabels">Fond rulment:</span>
            <asp:TextBox runat="server" ID="tbFondRulment" Enabled="false"></asp:TextBox>
            <%--<asp:Button runat="server" ID="Button1" OnClick="btnAssociationFiscalCode_Click" Text="Modifică" />--%>
            <br />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
