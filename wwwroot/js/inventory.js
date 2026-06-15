$(function () {
    var canManage = window.ptCanManageInventory === true;
    var currentMedicineId = null;
    var currentMedicineName = "";

    function getToken() {
        return $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();
    }

    function formatMoney(v) {
        return "$" + parseFloat(v).toFixed(2);
    }

    var table = $("#inventoryTable").DataTable({
        ajax: {
            url: "/Inventory/GetAll",
            dataSrc: "data"
        },
        columns: [
            {
                data: null,
                render: function (row) {
                    return row.name + (row.strength ? ' <span class="text-muted">(' + row.strength + ')</span>' : "");
                }
            },
            { data: "categoryName" },
            { data: "manufacturerName" },
            { data: "unitOfMeasure" },
            {
                data: "totalQuantity",
                className: "text-end",
                render: function (v, type, row) {
                    var cls = row.isLowStock ? "text-danger fw-bold" : "";
                    return '<span class="' + cls + '">' + v + '</span>';
                }
            },
            {
                data: null,
                orderable: false,
                render: function (row) {
                    var badges = "";
                    if (row.isLowStock) badges += '<span class="badge bg-danger me-1">Low Stock</span>';
                    if (row.hasExpiredBatches) badges += '<span class="badge bg-secondary me-1">Expired Stock</span>';
                    if (row.hasNearExpiryBatches) badges += '<span class="badge bg-warning text-dark me-1">Near Expiry</span>';
                    return badges || '<span class="text-muted">—</span>';
                }
            },
            { data: "sellingPrice", className: "text-end", render: formatMoney },
            {
                data: null,
                orderable: false,
                className: "text-end",
                render: function (row) {
                    return '<button class="btn btn-sm btn-outline-primary btn-view-batches" data-id="' + row.medicineId + '" data-name="' + row.name + '">' +
                        '<i class="fa-solid fa-boxes-stacked"></i> Batches</button>';
                }
            }
        ],
        order: [[0, "asc"]],
        responsive: true
    });

    function loadBatches(medicineId, medicineName) {
        currentMedicineId = medicineId;
        currentMedicineName = medicineName;
        $("#batchesModalTitle").text("Batches — " + medicineName);

        $.get("/Inventory/GetBatches", { medicineId: medicineId }, function (res) {
            var $body = $("#batchesModalBody");
            $body.empty();

            if (res.data.length === 0) {
                $body.append('<tr><td colspan="6" class="text-center text-muted py-3">No batches for this medicine yet.</td></tr>');
                return;
            }

            $.each(res.data, function (i, b) {
                var expiryClass = b.isExpired ? "text-danger fw-bold" : "";
                var actionsHtml = canManage
                    ? '<button class="btn btn-sm btn-outline-secondary btn-adjust" data-id="' + b.id +
                      '" data-batch="' + b.batchNumber + '" data-remaining="' + b.quantityRemaining + '">' +
                      '<i class="fa-solid fa-sliders"></i> Adjust</button>'
                    : "";

                var row = '<tr>' +
                    '<td class="font-monospace small">' + b.batchNumber + '</td>' +
                    '<td class="' + expiryClass + '">' + b.expiryDate + '</td>' +
                    '<td class="text-end">' + b.quantityReceived + '</td>' +
                    '<td class="text-end">' + b.quantityRemaining + '</td>' +
                    '<td class="text-end">' + formatMoney(b.unitCost) + '</td>' +
                    '<td class="text-end">' + actionsHtml + '</td>' +
                    '</tr>';
                $body.append(row);
            });

            new bootstrap.Modal("#batchesModal").show();
        }).fail(function () {
            Swal.fire("Error", "Could not load batches for this medicine.", "error");
        });
    }

    $("#inventoryTable").on("click", ".btn-view-batches", function () {
        loadBatches($(this).data("id"), $(this).data("name"));
    });

    $("#batchesModalBody").on("click", ".btn-adjust", function () {
        var batchId = $(this).data("id");
        var batchNumber = $(this).data("batch");
        var remaining = $(this).data("remaining");

        $("#AdjustBatchId").val(batchId);
        $("#AdjustNewQuantity").val(remaining);
        $("#AdjustReason").val("");
        $(".text-danger.small").text("");
        $("#adjustBatchInfo").text("Batch " + batchNumber + " — current quantity: " + remaining);

        new bootstrap.Modal("#adjustModal").show();
    });

    $("#adjustForm").on("submit", function (e) {
        e.preventDefault();

        $(".text-danger.small").text("");

        var newQuantity = $("#AdjustNewQuantity").val();
        var reason = $("#AdjustReason").val().trim();

        if (newQuantity === "" || newQuantity < 0) {
            $("#AdjustNewQuantityError").text("Enter a valid quantity.");
            return;
        }
        if (!reason) {
            $("#AdjustReasonError").text("A reason is required.");
            return;
        }

        $("#btnSaveAdjustment").prop("disabled", true);

        $.post("/Inventory/Adjust", {
            batchId: $("#AdjustBatchId").val(),
            newQuantity: newQuantity,
            reason: reason,
            __RequestVerificationToken: getToken()
        })
            .done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(document.getElementById("adjustModal")).hide();
                    Swal.fire({ icon: "success", title: res.message, timer: 1500, showConfirmButton: false });
                    table.ajax.reload();
                    loadBatches(currentMedicineId, currentMedicineName);
                } else {
                    Swal.fire("Could not save", res.message, "error");
                }
            })
            .fail(function () {
                Swal.fire("Error", "Something went wrong. Please try again.", "error");
            })
            .always(function () {
                $("#btnSaveAdjustment").prop("disabled", false);
            });
    });
});
