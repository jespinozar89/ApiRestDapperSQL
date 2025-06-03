namespace MyApiRestDapperSQL.Models.Entities
{
    public class OrderItem
    {
        public int ORDER_ID { get; set; }
        public int LINE_ITEM_ID { get; set; }
        public int PRODUCT_ID { get; set; }
        public decimal UNIT_PRICE { get; set; }
        public int QUANTITY { get; set; }
        public int? SHIPMENT_ID { get; set; }
    }
}