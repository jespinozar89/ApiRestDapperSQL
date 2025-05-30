namespace MyApiRestDapperSQL.Models.Entities
{
    public class Customer
    {
        public int CUSTOMER_ID { get; set; }//PK
        public string EMAIL_ADDRESS { get; set; }
        public string FULL_NAME { get; set; }
    }
}