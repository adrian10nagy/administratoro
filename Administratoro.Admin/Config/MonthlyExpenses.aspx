<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MonthlyExpenses.aspx.cs" Inherits="Admin.Config.MonthlyExpenses" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainStyle" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Modifică cheltuielile lunare de afișat</h2>
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <!-- Tabs -->
            <div id="wizard_verticle" class="form_wizard wizard_verticle">
                <ul class="list-unstyled wizard_steps">
                    <li>
                        <a href="#step11" runat="server" id="expenseListHref1">
                            <span class="step_no">1</span>
                        </a>
                    </li>
                    <li>
                        <a href="#step22" runat="server" id="expenseListHref2">
                            <span class="step_no">2</span>
                        </a>
                    </li>
                    <li>
                        <a href="#step33" runat="server" id="expenseListHref3">
                            <span class="step_no">3</span>
                        </a>
                    </li>
                </ul>

                <div id="step11" runat="server" visible="true" class="stepContainer">
                    <h2 class="StepTitle">Pasul 1</h2>
                    <span class="section">Alege luna pe care o dorești modificată</span>
                    <div class="form-group">
                        <div class="col-md-12 col-sm-12">
                            <asp:DropDownList ID="drpExpenseMonth" runat="server" CssClass="form-control col-md-6 col-xs-12"></asp:DropDownList>
                        </div>
                    </div>
                    <br />
                    <br />
                    <br />
                    <asp:Button runat="server" ID="btnStep1" OnClick="btnStep1_Click" Text="Următoarea" />
                </div>
                <div id="step22" runat="server" visible="false" class="stepContainer">
                    <h2 class="StepTitle">Pasul 2</h2>
                    <asp:Label ID="step22Message" runat="server" CssClass="section"></asp:Label>
                    <asp:Table ID="tblMonthlyExpenses" runat="server" class="table table-striped table-bordered dt-responsive nowrap"
                         CellSpacing="0" Width="100%" ClientIDMode="Static">
                        <asp:TableHeaderRow TableSection="TableHeader">
                            <asp:TableHeaderCell>#</asp:TableHeaderCell>
                            <asp:TableHeaderCell>Cheltuielă</asp:TableHeaderCell>
                            <asp:TableHeaderCell>Tip</asp:TableHeaderCell>
                        </asp:TableHeaderRow>
                    </asp:Table>
                    <br />
                    <br />
                    <br />
                    <asp:Button runat="server" ID="btnBackStep2" OnClick="btnBackStep2_Click" Text="Precedent" />
                    <asp:Button runat="server" ID="btnStep2" OnClick="btnStep2_Click" Text="Următoarea" />

                </div>
                <div id="step33" runat="server" visible="false" class="stepContainer">
                    <h2 class="StepTitle">Step 3</h2>
                    <asp:Label runat="server" ID="step33MEssage" CssClass="colorGreen">Modificări salvate cu succes</asp:Label>
                    <br />
                    <br />
                    <br />
                    <asp:Button runat="server" ID="btnStep3Next1" OnClick="btnStep3Next1_Click" Text="Du-mă la cheltuielile de luna asta" />
                    <asp:Button runat="server" ID="btnStep3Next2" OnClick="btnStep3Next2_Click" Text="Modific cheltuielile pe încă o lună" />
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
        </Triggers>
    </asp:UpdatePanel>
    <!-- End SmartWizard Content -->
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainScript" runat="server">
    <%--    <script src="../Scripts/jquery.smartWizard.js"></script>
    <script>
        $(document).ready(function () {

            $('#wizard_verticle').smartWizard({
                transitionEffect: 'slide',
                toolbarSettings: {
                    toolbarPosition: 'bottom', // none, top, bottom, both
                    toolbarButtonPosition: 'right', // left, right
                    showNextButton: false, // show/hide a Next button
                    showPreviousButton: false, // show/hide a Previous button
                }
            });

            $('.buttonNext').addClass('btn btn-success');
            $('.buttonPrevious').addClass('btn btn-primary');
            $('.buttonFinish').addClass('btn btn-default');
        });
    </script>--%>
</asp:Content>
