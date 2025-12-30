namespace FP.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal Spent { get; set; }
        public DateTime Month { get; set; }
        public string UserId { get; set; } = string.Empty;

        public decimal Remaining 
        { 
            get 
            { 
                return Amount - Spent; 
            } 
        }
        
        public decimal PercentageUsed 
        { 
            get 
            { 
                return Amount > 0 ? (Spent / Amount) * 100 : 0;
            }
        }

        public decimal Limit 
        { 
            get 
            { 
                return Amount; 
            } 
        }
    }
}