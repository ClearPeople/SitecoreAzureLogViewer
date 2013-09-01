<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SitecoreLogsViewer.aspx.cs" Inherits="ClearPeople.AzureLogViewer.SitecoreLogsViewer" %>

<%@ Register Assembly="ClearPeople.Azure.Utylities" Namespace="ClearPeople.Azure.Utylities.Controls" TagPrefix="cc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>ClearPeople Azure Log viewer</title>

    <script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.min.js"></script>
    <script src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.16/jquery-ui.min.js"></script>

    <link type="text/css" href="Styles\jquery-ui-timepicker-addon.css" rel="stylesheet" />
    <script src="Scrypts\jquery-ui-timepicker-addon.js"></script>    
    <base target="_self" />
</head>

<body>
    <form id="form1" runat="server">
        <div id="logviewer">

            <div id="filters">
                <h3>Search filter</h3>
                <div>
                    <asp:Label ID="Label1" runat="server" Text="From" AssociatedControlID="fromdatetime"></asp:Label>
                    <asp:TextBox ID="fromdatetime" ClientIDMode="Static" runat="server"></asp:TextBox>
                    <asp:Label ID="Label2" runat="server" Text="To" AssociatedControlID="todatetime"></asp:Label>
                    <asp:TextBox ID="todatetime" ClientIDMode="Static" runat="server"></asp:TextBox>
                    <asp:Label ID="Label3" runat="server" Text="Severity" AssociatedControlID="Level"></asp:Label>
                    <asp:DropDownList ID="Level" runat="server">
                        <asp:ListItem Selected="True" Value="6">All</asp:ListItem>
                        <asp:ListItem Value="5">Debug</asp:ListItem>
                        <asp:ListItem Value="4">Info</asp:ListItem>
                        <asp:ListItem Value="3">Warning</asp:ListItem>
                        <asp:ListItem Value="2">Error</asp:ListItem>
                        <asp:ListItem Value="1">Fatal</asp:ListItem>
                    </asp:DropDownList>
                    <asp:Label ID="Label5" runat="server" Text="Instance" AssociatedControlID="Instances"></asp:Label>
                    <asp:DropDownList ID="Instances" runat="server">
                        <asp:ListItem Selected="true" Value=""> -- </asp:ListItem>
                    </asp:DropDownList>
                    <asp:Label ID="Label4" runat="server" Text="Page size" AssociatedControlID="PageSize"></asp:Label>
                    <asp:DropDownList ID="PageSize" runat="server">
                        <asp:ListItem>10</asp:ListItem>
                        <asp:ListItem>25</asp:ListItem>
                        <asp:ListItem>50</asp:ListItem>
                        <asp:ListItem Selected="True">100</asp:ListItem>
                        <asp:ListItem>500</asp:ListItem>
                        <asp:ListItem>100</asp:ListItem>
                    </asp:DropDownList>

                    <asp:Button ID="update" runat="server" Text="Search" CssClass="ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only" OnClick="update_Click" />
                </div>
                <h3>Filter current page</h3>
                <div>
                    <label for="filter">Filter</label>
                    <input type="text" name="filter" value="" id="filter" />
                </div>
            </div>

            <hr />
            <asp:Panel ID="Errors" runat="server" CssClass="ui-widget" Visible="False">
                <div class="ui-state-error ui-corner-all">
                    <asp:Literal ID="ltlerrors" runat="server"></asp:Literal>
                </div>                
            </asp:Panel>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <asp:UpdateProgress ID="UpdateProgress1" runat="server" DynamicLayout="true">
                <ProgressTemplate>
                  <div class="ui-widget">
                        
                        <div class="ui-state-highlight ui-corner-all loadingdiv">
                            <p>
                            <asp:Literal ID="ltlloading" runat="server" />
                            </p>
                        </div>
                    </div>
                </ProgressTemplate>
                
            </asp:UpdateProgress>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                
                    
                    <Triggers>
 <asp:AsyncPostBackTrigger ControlID="update" EventName="Click" />
 </Triggers>
                <ContentTemplate>
            <cc1:CPGridView ID="LogGridView" CssClass="ui-widget ui-widget-content ui-corner-all" runat="server" AutoGenerateColumns="False" AllowPaging="True" AllowCustomPaging="false" OnPageIndexChanging="LogGridView_PageIndexChanging" GridLines="None" ShowFooter="True">
                <AlternatingRowStyle CssClass="ui-state-default" />
                <RowStyle CssClass="ui-state-hover" />
                <Columns>
                    <asp:BoundField DataField="RoleInstance" HeaderText="RoleInstance" SortExpression="RoleInstance"></asp:BoundField>
                    <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level"></asp:BoundField>
                    <asp:BoundField DataField="Message" HeaderText="Message" SortExpression="Message"></asp:BoundField>
                    <asp:BoundField DataField="Timestamp" HeaderText="Timestamp" SortExpression="Timestamp"></asp:BoundField>
                    <asp:TemplateField>
                        <FooterTemplate>
                            <asp:Label ID="FooterText" runat="server" Text="Label"></asp:Label>
                        </FooterTemplate>
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <div>

                        <div style="padding: 0px 0.7em; margin-top: 20px;" class="ui-state-highlight ui-corner-all">
                            <p>
                                <span style="margin-right: 0.3em; float: left;" class="ui-icon ui-icon-info"></span>
                                <asp:Literal ID="Literal1" runat="server" Text="<%# this.NoRecordsFoundText %>"></asp:Literal>
                            </p>
                        </div>
                    </div>
                </EmptyDataTemplate>

                <HeaderStyle CssClass="ui-widget-header" />
                <FooterStyle CssClass="ui-widget-header" />
                <PagerSettings PageButtonCount="5" />
                <PagerSettings PageButtonCount="5" />

            </cc1:CPGridView>
                </ContentTemplate>
            </asp:UpdatePanel>
            
        </div>

    </form>
    <script type="text/javascript">

        $(function () {
            $('#filters').accordion({
                heightStyle: "content"
            });
            //default each row to visible
            $('#LogGridView > tbody > tr').addClass('visible');

            $('#filter').keyup(function (event) {
                //if esc is pressed or nothing is entered
                if (event.keyCode == 27 || $(this).val() == '') {
                    //if esc is pressed we want to clear the value of search box
                    $(this).val('');

                    //we want each row to be visible because if nothing
                    //is entered then all rows are matched.
                    $('#LogGridView > tbody > tr').removeClass('visible').show().addClass('visible');
                }

                    //if there is text, lets filter
                else {
                    filter('#LogGridView > tbody > tr', $(this).val());
                }

            });

        });
        //filter results based on query
        function filter(selector, query) {
            query = $.trim(query); //trim white space
            query = query.replace(/ /gi, '|'); //add OR for regex query

            $(selector).each(function () {
                ($(this).text().search(new RegExp(query, "i")) < 0) ? $(this).hide().removeClass('visible') : $(this).show().addClass('visible');
            });
        }
    </script>

</body>
</html>
