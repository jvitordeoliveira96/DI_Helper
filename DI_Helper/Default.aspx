<%@ Page Title="Taxa DI" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DI_Helper._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Taxa DI</h1>
        <p class="lead">A aplicação Web a seguir tem o intuito de auxiliar um operador de crédito a encontrar a taxa DI para um empréstimo com um determinado prazo. </p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Data da curva</h2>
            <div>
            <asp:DropDownList ID="MonthDropDownList1" runat="server" OnSelectedIndexChanged="Update_Calendar1" AutoPostBack="true"> </asp:DropDownList>    
            <asp:DropDownList ID="YearDropDownList1" runat="server" OnSelectedIndexChanged="Update_Calendar1" AutoPostBack="true"> </asp:DropDownList>
            <p></p>    
            <asp:Calendar ID="Calendar1" runat="server" ondayrender="Calendar1_DayRender"></asp:Calendar>
            </div>
            <p></p>
            <asp:Button class="btn btn-default" ID="LoadDate" runat="server" Text="Carregar" OnClick="LoadDate_Click" Height="31px" />
            <p></p>
        </div>
        <div class="col-md-4">
            <asp:Literal ID="TaxesTitle" runat="server" />
            <asp:Literal ID="TaxesList" runat="server" />
        </div>
        <div class="col-md-4">
            <h2>Interpolar taxa para data</h2>
            <div>
                <asp:DropDownList ID="MonthDropDownList2" runat="server" OnSelectedIndexChanged="Update_Calendar2" AutoPostBack="true"> </asp:DropDownList>
                <asp:DropDownList ID="YearDropDownList2" runat="server" OnSelectedIndexChanged="Update_Calendar2" AutoPostBack="true"> </asp:DropDownList>
                <p></p>
                <asp:Calendar ID="Calendar2" runat="server" ondayrender="Calendar2_DayRender"></asp:Calendar>
            </div>
            <div>
                <p></p>
                <div><asp:DropDownList ID="InterpolationTypeList" runat="server" OnSelectedIndexChanged="InterpolationType_Change" AutoPostBack="true"> </asp:DropDownList></div>
                <h4>Choque de juros (%)</h4>
                <asp:TextBox ID="InterestRateShock" placeholder="0" runat="server"></asp:TextBox>
                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="InterestRateShock" runat="server" ErrorMessage="Somente números são permitidos!" ValidationExpression="^-*[0-9,\.]+$"></asp:RegularExpressionValidator>            
                <p></p>
                <asp:Button class="btn btn-default" ID="CalculateTax" runat="server" OnClick="CalculateTax_Click" Text="Calcular" Height="31px" />

            </div>
            <p><p></p></p>
            <div>
                <asp:Literal ID="FinalResults" runat="server" />
            </div>
            
        </div>
    </div>
    
    <div id="plotDiv">
         <asp:Literal ID="TaxPlotly" runat="server" />
    </div>

    

</asp:Content>
