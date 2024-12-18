using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BTL
{
    public partial class Cart : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Login.aspx");
            }

        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Write("<script>alert('You must log in to proceed!');</script>");
                Response.Redirect("Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserId"]); // Lấy ID từ phiên đăng nhập
            decimal totalPrice = 0;

            List<CartItem> cartItems = GetCartItems(userId);

            if (cartItems == null || !cartItems.Any())
            {
                Response.Write("<script>alert('Your cart is empty!');</script>");
                return;
            }

            using (SqlConnection conn = new SqlConnection("Data Source=Cuonglune25\\SQLEXPRESS;Initial Catalog=mydata;Integrated Security=True"))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Tạo shipment
                    int shipmentId = CreateShipment(conn, transaction, userId, "Customer Name", "0987654321", "Customer Address");

                    // 2. Tạo payment
                    totalPrice = cartItems.Sum(item => item.Quantity * item.Price);
                    int paymentId = CreatePayment(conn, transaction, userId, "Credit Card", totalPrice);

                    // 3. Tạo order
                    int orderId = CreateOrder(conn, transaction, userId, totalPrice, paymentId, shipmentId);

                    // 4. Tạo order items và cập nhật số lượng tồn kho
                    foreach (var item in cartItems)
                    {
                        AddOrderItem(conn, transaction, orderId, item.ProductId, item.Quantity, item.Price);
                        UpdateProductStock(conn, transaction, item.ProductId, item.Quantity);
                    }

                    // 5. Xóa sản phẩm khỏi giỏ hàng
                    ClearCart(conn, transaction, userId);

                    transaction.Commit();
                    Response.Redirect("OrderSuccess.aspx");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Response.Write($"<script>alert('Checkout failed: {ex.Message}');</script>");
                }
            }
        }

        private List<CartItem> GetCartItems(int userId)
        {
            List<CartItem> cartItems = new List<CartItem>();
            string query = "SELECT c.id, c.quantity, p.price " +
                           "FROM tblcart c " +
                           "JOIN tblproduct p ON c.id = p.id " + // Join để lấy giá sản phẩm
                           "WHERE c.user_id = @userId";

            using (SqlConnection conn = new SqlConnection("Data Source=Cuonglune25\\SQLEXPRESS;Initial Catalog=mydata;Integrated Security=True"))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CartItem item = new CartItem
                            {
                                ProductId = reader.GetInt32(0),
                                Quantity = reader.GetInt32(1),
                                Price = reader.GetDecimal(2) // Lấy thêm giá sản phẩm
                            };
                            cartItems.Add(item);
                        }
                    }
                }
            }

            return cartItems;
        }

        private int CreateShipment(SqlConnection conn, SqlTransaction transaction, int userId, string fullname, string phone, string address)
        {
            string query = "INSERT INTO tblshipment (fullname, phone, address, user_id, status, date) " +
                           "VALUES (@fullname, @phone, @address, @user_id, 'Pending', GETDATE()); SELECT SCOPE_IDENTITY();";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@fullname", fullname);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@address", address);
                cmd.Parameters.AddWithValue("@user_id", userId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int CreatePayment(SqlConnection conn, SqlTransaction transaction, int userId, string method, decimal amount)
        {
            string query = "INSERT INTO tblpayment (method, amount, user_id, date) " +
                           "VALUES (@method, @amount, @user_id, GETDATE()); SELECT SCOPE_IDENTITY();";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@method", method);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@user_id", userId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void UpdateProductStock(SqlConnection conn, SqlTransaction transaction, int productId, int quantity)
        {
            string query = "UPDATE tblproduct SET stock = stock - @quantity WHERE id = @productId";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.ExecuteNonQuery();
            }
        }

        private int CreateOrder(SqlConnection conn, SqlTransaction transaction, int userId, decimal totalPrice, int paymentId, int shipmentId)
        {
            string query = "INSERT INTO tblorder (user_id, totalprice, payment_id, shipment_id) " +
                           "VALUES (@user_id, @totalPrice, @paymentId, @shipmentId); SELECT SCOPE_IDENTITY();";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@totalPrice", totalPrice);
                cmd.Parameters.AddWithValue("@paymentId", paymentId);
                cmd.Parameters.AddWithValue("@shipmentId", shipmentId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void AddOrderItem(SqlConnection conn, SqlTransaction transaction, int orderId, int productId, int quantity, decimal price)
        {
            string query = "INSERT INTO tblorderitem (order_id, product_id, quantity, price) " +
                           "VALUES (@orderId, @productId, @quantity, @price)";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.ExecuteNonQuery();
            }
        }

        private void ClearCart(SqlConnection conn, SqlTransaction transaction, int userId)
        {
            string query = "DELETE FROM tblcart WHERE user_id = @userId";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
