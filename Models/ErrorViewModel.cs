namespace FP.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId 
        { 
            get 
            { 
                if (!string.IsNullOrEmpty(RequestId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } 
        }
    }
}
