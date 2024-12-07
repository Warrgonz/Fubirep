namespace fubi_client.Models
{
    public class PrestamoDetalle
    {
       
            public int LoanID { get; set; }
            public int LoanQuantity { get; set; }
            public DateTime LoanDate { get; set; }
            public DateTime LoanDueDate { get; set; }
            public string BeneficiaryName { get; set; }
            public string ManagerName { get; set; }
            public string ManagerLastName { get; set; }
            public string ItemName { get; set; }
            public string LoanStatus { get; set; }
        
    }
}
