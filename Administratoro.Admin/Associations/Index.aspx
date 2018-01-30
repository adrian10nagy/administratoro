<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="Admin.Associations.Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <span class="associationsLabels">Denumire asociație:</span>
    <asp:TextBox runat="server" ID="txtEstateName" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="bntEstateNameChange" OnClick="bntEstateNameChange_Click" Text="Modifică" />
    <br />

    <span class="associationsLabels">Adresa:</span>
    <asp:TextBox runat="server" ID="txtEstateAddress" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="btnEstateAddress" OnClick="btnEstateAddress_Click" Text="Modifică" />
    <br />

    <span class="associationsLabels">Cod fiscal:</span>
    <asp:TextBox runat="server" ID="txtEstateFiscalCode" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="btnEstateFiscalCode" OnClick="btnEstateFiscalCode_Click" Text="Modifică" />
    <br />
    <span class="associationsLabels">Cont bancar:</span>
    <asp:TextBox runat="server" ID="txtEstateBanckAccount" Enabled="false"></asp:TextBox>
    <asp:Button runat="server" ID="btnEstateBanckAccount" OnClick="btnEstateBanckAccount_Click" Text="Modifică" />
    <br />
    <div>
        <asp:RadioButtonList ID="drpEstateEqualIndiviza" CssClass="AssociationsNewRadioBtns" runat="server" AutoPostBack="true" RepeatLayout="Flow" OnSelectedIndexChanged="estateEqualIndiviza_SelectedIndexChanged" RepeatDirection="Horizontal">
            <asp:ListItem Text="Nu" Value="0" Selected="True" />
            <asp:ListItem Text="Da" Value="1" />
        </asp:RadioButtonList>
        <span>Cotă de indiviză egală la toate apatamentele</span>
        <asp:TextBox runat="server" ID="txtEstateCotaIndivizaApartments" Visible="false" placeholder="ex: 1,16" Enabled="false"></asp:TextBox>
        %
    <asp:Button runat="server" ID="btnEstateEqualIndiviza" OnClick="btnEstateEqualIndiviza_Click" Text="Modifică" Visible="false" />
    </div>
    <br />

    <div>
        <span class="associationsLabels">Bloc împărțit pe scări:</span>
        <asp:RadioButtonList ID="estateStairs" runat="server" AutoPostBack="true" OnSelectedIndexChanged="estateStairs_SelectedIndexChanged"
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
        <asp:TextBox runat="server" ID="txtEstateStairCaseName" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:TextBox>
        <asp:Label runat="server" Text="Serie contor:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
        <asp:TextBox runat="server" ID="txtEstateStairCaseIndiviza" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:TextBox>
    </asp:Panel>
    <asp:Button runat="server" ID="btneStatestairCasesNew" Text="Adaugă scară" OnClick="btneStatestairCasesNew_Click" CssClass="col-md-2 col-xs-2 col-xs-offset-3" />
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
            <asp:BoundField HeaderText="Scară" DataField="Id_StairCase" />
            <asp:TemplateField HeaderText="Scară">
                <ItemTemplate>
                    <asp:Label ID="lblStairCaseId" runat="server" Text='<%# Eval("Id_StairCase") %>' Visible="false" />
                    <asp:DropDownList ID="ddlStairCase" runat="server" Enabled="false">
                    </asp:DropDownList>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <asp:Panel ID="newCounter" runat="server" Visible="false">
        <div class="col-md-12 col-cs-12">
            <asp:Label runat="server" Text="Cheltuială:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
            <asp:DropDownList runat="server" ID="drpEstateCounterTypeNew" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:DropDownList>
        </div>
        <div class="col-md-12 col-cs-12">
            <asp:Label runat="server" Text="Scară:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
            <asp:DropDownList runat="server" ID="drpEstateStairs" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:DropDownList>
        </div>
        <div class="col-md-12 col-cs-12">
            <asp:Label runat="server" Text="Serie contor:" CssClass="col-md-2 col-xs-2 col-xs-offset-1"></asp:Label>
            <asp:TextBox runat="server" ID="txtEstateCounterValueNew" CssClass="col-md-6 col-xs-6 col-xs-offset-1"></asp:TextBox>
        </div>
    </asp:Panel>
    <asp:Button runat="server" ID="btnEstateCountersNew" Text="Adaugă contor" OnClick="btnEstateCountersNew_Click" CssClass="col-md-2 col-xs-2 col-xs-offset-3" />

    <asp:Panel runat="server" ID="associationIndex" Visible="false">
        <h3>Indiviză</h3>
        <asp:TextBox runat="server" ID="estateIndiviza" Enabled="false" />
    </asp:Panel>


</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
</asp:Content>
