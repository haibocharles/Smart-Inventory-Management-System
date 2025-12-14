using LHP_Inventory_management_system_MVC.Models;
using MySql.Data.MySqlClient;
using System.Data;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
namespace LHP_Inventory_management_system_MVC.Data
{
    public class OrderRepository
    {
        private readonly string _connectString;
        public OrderRepository(string connectString)
        {
            _connectString = connectString;
        }

        //獲取所有出入庫交易紀錄

        public List<Orders> GetOrders() 
        {
            var orders = new List<Orders>();

            using (var connection = new MySqlConnection(_connectString))
            {
                connection.Open();
                var command = new MySqlCommand(@"
                    select * from orders
                    order by OrderDate Desc", connection);

              using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Orders
                        {
                            Id = reader.GetInt32("Id"),
                            OrderNumber = reader.GetString("OrderNumber"),
                            OrderType = reader.GetString("OrderType"),
                            OrderDate = reader.GetDateTime("OrderDate"),
                            OperatorName = reader.IsDBNull("OperatorName") ? null : reader.GetString("OperatorName"),
                            SupplierOrCustomer = reader.IsDBNull("SupplierOrCustomer") ? null : reader.GetString("SupplierOrCustomer"),
                            PartCode = reader.GetString("PartCode"),
                            PartName = reader.IsDBNull("PartName") ? null : reader.GetString("PartName"),
                            Quantity = reader.GetInt32("Quantity"),
                            UnitPrice = reader.GetDecimal("UnitPrice"),
                            TotalAmount = reader.GetDecimal("TotalAmount"),
                            FromLocation = reader.IsDBNull("FromLocation") ? null : reader.GetString("FromLocation"),
                            ToLocation = reader.IsDBNull("ToLocation") ? null : reader.GetString("ToLocation"),
                            Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks")
                        });
                    }
                }
            }
            return orders;
        }



        // 添加新訂單

        public bool AddOrder(Orders order)
        {
            using (var connection = new MySqlConnection(_connectString))
            {
                connection.Open();
                var command = new MySqlCommand(@"
                    INSERT INTO Orders (
                        OrderNumber, OrderType, OrderDate, OperatorName, 
                        SupplierOrCustomer, PartCode, PartName, Quantity, 
                        UnitPrice, TotalAmount, FromLocation, ToLocation, Remarks
                    ) VALUES (
                        @OrderNumber, @OrderType, @OrderDate, @OperatorName,
                        @SupplierOrCustomer, @PartCode, @PartName, @Quantity,
                        @UnitPrice, @TotalAmount, @FromLocation, @ToLocation, @Remarks
                    )", connection);

                command.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                command.Parameters.AddWithValue("@OrderType", order.OrderType);
                command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                command.Parameters.AddWithValue("@OperatorName", order.OperatorName);
                command.Parameters.AddWithValue("@SupplierOrCustomer", order.SupplierOrCustomer);
                command.Parameters.AddWithValue("@PartCode", order.PartCode);
                command.Parameters.AddWithValue("@PartName", order.PartName);
                command.Parameters.AddWithValue("@Quantity", order.Quantity);
                command.Parameters.AddWithValue("@UnitPrice", order.UnitPrice);
                command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                command.Parameters.AddWithValue("@FromLocation", order.FromLocation);
                command.Parameters.AddWithValue("@ToLocation", order.ToLocation);
                command.Parameters.AddWithValue("@Remarks", order.Remarks);
                return command.ExecuteNonQuery() > 0;
            }
          
        }

    }

}

