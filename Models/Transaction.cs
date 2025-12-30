namespace FP.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }       
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }        
    }
}