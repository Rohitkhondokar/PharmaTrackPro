$(function () {
    var medicineOptionsHtml = "";
    var medicinePriceMap = {};

    function getToken() {
        return $('#purchaseForm input[name="__RequestVerificationToken"]').val();
    }

    function formatMoney(v) {
        return "$" + (isNaN(v) ? "0.00" : parseFloat(v).toFixed(2));
    }

    function loadFormOptions() {
        return $.get("/Purchases/GetFormOptions", function (data) {
            var $supplier = $("#SupplierId");
            $.each(data.suppliers, function (i, s) {
                $supplier.append($("<option>", { value: s.id, text: s.name }));
            });

            $.each(data.medicines, function (i, m) {
                var label = m.name + (m.strength ? " (" + m.strength + ")" : "");
                medicineOptionsHtml += '<option value="' + m.id + '">' + label + '</option>';
                medicinePriceMap[m.id] = m.purchasePrice;
            });
        });
    }

    function recalcRow($row) {
        var qty = parseFloat($row.find(".qty-input").val()) || 0;
        var cost = parseFloat($row.find(".cost-input").val()) || 0;
        $row.find(".subtotal-cell").text(formatMoney(qty * cost));
        recalcGrandTotal();
    }

    function recalcGrandTotal() {
        var total = 0;
        $("#lineItemsBody .line-item-row").each(function () {
            var qty = parseFloat($(this).find(".qty-input").val()) || 0;
            var cost = parseFloat($(this).find(".cost-input").val()) || 0;
            total += qty * cost;
        });
        $("#grandTotal").text(formatMoney(total));
    }

    function addLine() {
        var template = document.getElementById("lineItemTemplate");
        var $tr = $(template.content.querySelector(".line-item-row").cloneNode(true));
        $tr.find(".medicine-select").append(medicineOptionsHtml);
        $("#lineItemsBody").append($tr);
        return $tr;
    }

    $("#btnAddLine").on("click", function () {
        addLine();
    });

    // Auto-fill unit cost when a medicine is selected
    $("#lineItemsBody").on("change", ".medicine-select", function () {
        var $row = $(this).closest(".line-item-row");
        var medicineId = $(this).val();
        if (medicineId && medicinePriceMap[medicineId] !== undefined) {
            $row.find(".cost-input").val(medicinePriceMap[medicineId]);
            recalcRow($row);
        }
    });

    $("#lineItemsBody").on("input", ".qty-input, .cost-input", function () {
        recalcRow($(this).closest(".line-item-row"));
    });

    $("#lineItemsBody").on("click", ".btn-remove-line", function () {
        $(this).closest(".line-item-row").remove();
        recalcGrandTotal();
    });

    loadFormOptions().done(function () {
        addLine(); // start with one empty row
    });

    $("#purchaseForm").on("submit", function (e) {
        e.preventDefault();

        $(".text-danger.small").text("");

        var rows = [];
        var valid = true;

        $("#lineItemsBody .line-item-row").each(function () {
            var medicineId = $(this).find(".medicine-select").val();
            var quantity = $(this).find(".qty-input").val();
            var unitCost = $(this).find(".cost-input").val();
            var batchNumber = $(this).find(".batch-input").val();
            var expiryDate = $(this).find(".expiry-input").val();

            if (!medicineId || !quantity || !unitCost || !batchNumber || !expiryDate) {
                valid = false;
                return;
            }

            rows.push({
                MedicineId: medicineId,
                Quantity: quantity,
                UnitCost: unitCost,
                BatchNumber: batchNumber,
                ExpiryDate: expiryDate
            });
        });

        if (!$("#SupplierId").val()) {
            $("#SupplierIdError").text("Select a supplier.");
            return;
        }

        if (rows.length === 0 || !valid) {
            $("#ItemsError").text("Every line item needs a medicine, quantity, unit cost, batch number, and expiry date.");
            return;
        }

        // Build form data with ASP.NET Core's expected "Items[i].Property" keys
        // so the List<PurchaseItemInputViewModel> binds correctly — a raw JSON
        // body would need [FromBody], which complicates antiforgery validation.
        var formData = new FormData();
        formData.append("SupplierId", $("#SupplierId").val());
        formData.append("InvoiceNumber", $("#InvoiceNumber").val());
        formData.append("PurchaseDate", $("#PurchaseDate").val());
        formData.append("Notes", $("#Notes").val());
        formData.append("__RequestVerificationToken", getToken());

        rows.forEach(function (item, i) {
            formData.append("Items[" + i + "].MedicineId", item.MedicineId);
            formData.append("Items[" + i + "].Quantity", item.Quantity);
            formData.append("Items[" + i + "].UnitCost", item.UnitCost);
            formData.append("Items[" + i + "].BatchNumber", item.BatchNumber);
            formData.append("Items[" + i + "].ExpiryDate", item.ExpiryDate);
        });

        $("#btnSavePurchase").prop("disabled", true);

        $.ajax({
            url: "/Purchases/Create",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false
        })
            .done(function (res) {
                if (res.success) {
                    Swal.fire({ icon: "success", title: res.message, timer: 1500, showConfirmButton: false })
                        .then(function () {
                            window.location.href = "/Purchases/Details/" + res.purchaseId;
                        });
                } else {
                    Swal.fire("Could not save", res.message, "error");
                }
            })
            .fail(function () {
                Swal.fire("Error", "Something went wrong while saving. Please try again.", "error");
            })
            .always(function () {
                $("#btnSavePurchase").prop("disabled", false);
            });
    });
});
