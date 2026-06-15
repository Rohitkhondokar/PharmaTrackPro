using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PharmaTrackPro.Models.Enums;

namespace PharmaTrackPro.ViewModels.Medicine
{
    public class MedicineFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Medicine name is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters.")]
        [Display(Name = "Medicine Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Generic Name")]
        public string? GenericName { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Manufacturer is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a manufacturer.")]
        [Display(Name = "Manufacturer")]
        public int ManufacturerId { get; set; }

        [StringLength(30)]
        public string? Strength { get; set; }

        [Required]
        [Display(Name = "Unit of Measure")]
        public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Tablet;

        [Required(ErrorMessage = "Purchase price is required.")]
        [Range(0, 999999.99, ErrorMessage = "Purchase price must be zero or greater.")]
        [Display(Name = "Purchase Price")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "Selling price is required.")]
        [Range(0, 999999.99, ErrorMessage = "Selling price must be zero or greater.")]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "Requires Prescription")]
        public bool RequiresPrescription { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Photo")]
        public IFormFile? ImageFile { get; set; }
    }
}
