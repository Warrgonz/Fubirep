namespace fubi_client.Models
{
    public class PrestamoRequest
    {
        public int LoanID { get; set; }
        public DateTime LoanDate { get; set; }
        public string BeneficiaryName { get; set; }
        public string ManagerName { get; set; }
        public string ManagerLastName { get; set; }
        public string LoanStatus { get; set; }
        public string ItemDescription { get; set; }
    }
}
