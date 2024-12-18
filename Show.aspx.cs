using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BTL
{
    public partial class Show : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int userId = 2; // ID người dùng đăng nhập (giả lập)
                LoadOrderHistory(userId);
            }
        }

        private void LoadOrderHistory(int userId)
        {
            using (SqlConnection conn = new SqlConnection("Data Source=Cuonglune25\\SQLEXPRESS;Initial Catalog=mydata;Integrated Security=True"))
            {
                string query = @"
                    SELECT o.id AS OrderID, 
                           o.toltalprice AS TotalPrice, 
                           p.name AS ProductName, 
                           oi.quantity AS Quantity, 
                           oi.price AS Price,
                           (oi.quantity * oi.price) AS TotalPricePerItem
                    FROM tblorder o
                    JOIN tblorderiteam oi ON o.id = oi.order_id
                    JOIN tblproduct p ON oi.product_id = p.id
                    WHERE o.user_id = @userId
                    ORDER BY o.id DESC"; // Sắp xếp đơn hàng mới nhất lên trên

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Kiểm tra xem có dữ liệu không trước khi bind vào GridView
                if (dt.Rows.Count > 0)
                {
                    GridViewOrders.DataSource = dt;
                    GridViewOrders.DataBind();
                }
                else
                {
                    // Nếu không có đơn hàng nào, có thể thông báo cho người dùng
                    Response.Write("<script>alert('No orders found.');</script>");
                }
            }
        }
    }
}
