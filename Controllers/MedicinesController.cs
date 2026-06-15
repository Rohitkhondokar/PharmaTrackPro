using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.ViewModels.Medicine;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class MedicinesController : Controller
    {
        private readonly IMedicineService _medicineService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;

        public MedicinesController(
            IMedicineService medicineService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService)
        {
            _medicineService = medicineService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool includeDeleted = false)
        {
            var medicines = await _medicineService.GetAllAsync(includeDeleted);

            var data = medicines.Select(m => new
            {
                m.Id,
                m.Name,
                m.GenericName,
                m.Strength,
                UnitOfMeasure = m.UnitOfMeasure.ToString(),
                CategoryName = m.Category != null ? m.Category.Name : "—",
                ManufacturerName = m.Manufacturer != null ? m.Manufacturer.Name : "—",
                m.PurchasePrice,
                m.SellingPrice,
                m.RequiresPrescription,
                m.ImagePath,
                m.Barcode,
                m.IsActive,
                m.IsDeleted
            });

            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var medicine = await _medicineService.GetByIdAsync(id);
            if (medicine is null)
            {
                return NotFound();
            }

            return Json(new
            {
                medicine.Id,
                medicine.Name,
                medicine.GenericName,
                medicine.CategoryId,
                medicine.ManufacturerId,
                medicine.Strength,
                UnitOfMeasure = (int)medicine.UnitOfMeasure,
                medicine.PurchasePrice,
                medicine.SellingPrice,
                medicine.RequiresPrescription,
                medicine.ImagePath,
                medicine.Barcode,
                medicine.BarcodeImagePath,
                medicine.QrCodeImagePath,
                medicine.IsActive
            });
        }

        /// <summary>Active categories + manufacturers for populating the Create/Edit dropdowns.</summary>
        [HttpGet]
        public async Task<IActionResult> GetFormOptions()
        {
            var categories = await _categoryService.GetAllAsync();
            var manufacturers = await _manufacturerService.GetAllAsync();

            return Json(new
            {
                categories = categories.Where(c => c.IsActive).Select(c => new { c.Id, c.Name }),
                manufacturers = manufacturers.Where(m => m.IsActive).Select(m => new { m.Id, m.Name })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager,Pharmacist")]
        public async Task<IActionResult> Create(MedicineFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _medicineService.CreateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager,Pharmacist")]
        public async Task<IActionResult> Edit(MedicineFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please correct the highlighted fields." });
            }

            var result = await _medicineService.UpdateAsync(model);
            return Json(new { result.Success, result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> PrintLabel(int id)
        {
            var medicine = await _medicineService.GetByIdAsync(id);
            if (medicine is null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicineService.SoftDeleteAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,StoreManager")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _medicineService.RestoreAsync(id);
            return Json(new { result.Success, result.Message });
        }
    }
}
