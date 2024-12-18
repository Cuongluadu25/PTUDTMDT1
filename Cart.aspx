<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cart.aspx.cs" Inherits="BTL.Cart" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            
    <h2>Shopping Cart</h2>
    <asp:GridView ID="GridViewCart" runat="server" AutoGenerateColumns="False" DataKeyNames="id">
        <Columns>
            <asp:BoundField DataField="name" HeaderText="Product Name" />
            <asp:BoundField DataField="quantity" HeaderText="Quantity" />
            <asp:BoundField DataField="price" HeaderText="Price" DataFormatString="{0:C}" />
            <asp:TemplateField HeaderText="Total">
                <ItemTemplate>
                    <%# Convert.ToDecimal(Eval("quantity")) * Convert.ToDecimal(Eval("price")) %>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <div style="margin-top: 20px;">
        <asp:Button ID="btnCheckout" runat="server" Text="Checkout" OnClick="btnCheckout_Click" CssClass="btn btn-primary" />
    </div>
</asp:Content>
        </div>
    </form>
</body>
</html>
