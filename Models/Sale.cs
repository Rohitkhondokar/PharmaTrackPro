using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmaTrackPro.Models.Enums;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A completed point-of-sale transaction. No soft delete — like Purchase,
    /// this is a financial record. There's no "pending" state (unlike
    /// Purchase): a Sale only exists once checkout has fully succeeded and
    /// stock has been deducted. Reversing one is the job of the future
    /// Sales Return module, not editing this record.
    /// </summary>
    public class Sale
    {
        public int Id { get; set; }

        // Nullable — walk-in customers are allowed without a Customer record.
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        [Required]
        [Display(Name = "Sale Date")]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string CashierUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(CashierUserId))]
        public ApplicationUser? CashierUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public ICollection<SaleReturn> Returns { get; set; } = new List<SaleReturn>();
    }
}
