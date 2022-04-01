namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class CityReport
    {
        public string TotalIncomeFormated
        {
            get
            {
                return TotalIncome.ToString("C");
            }
        }

        public string Name { get; set; }
        public string PostalCode { get; set; }
        public int NumberOfMembers { get; set; }
        public double TotalIncome { get; set; }
    }
}