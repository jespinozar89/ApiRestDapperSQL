namespace MyApiRestDapperSQL.Models.Entities
{
    public class Order
    {
        public int ORDER_ID { get; set; }
        public DateTime ORDER_TMS { get; set; }
        public int CUSTOMER_ID { get; set; }
        public string ORDER_STATUS { get; set; }
        public int STORE_ID { get; set; }
    }
}