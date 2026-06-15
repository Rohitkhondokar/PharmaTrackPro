namespace PharmaTrackPro.ViewModels.Sales
{
    public class PosCustomerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PosMedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Strength { get; set; }
        public decimal SellingPrice { get; set; }
        public bool RequiresPrescription { get; set; }

        /// <summary>Total remaining quantity across all non-expired batches — what's actually sellable right now.</summary>
        public int AvailableQuantity { get; set; }
    }

    public class PosDataViewModel
    {
        public List<PosCustomerViewModel> Customers { get; set; } = new();
        public List<PosMedicineViewModel> Medicines { get; set; } = new();
    }
}
