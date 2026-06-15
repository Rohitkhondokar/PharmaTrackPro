$(function () {
    var canEdit = window.ptCanEditMedicines === true;
    var canDelete = window.ptCanDeleteMedicines === true;

    function getToken() {
        return $('#medicineForm input[name="__RequestVerificationToken"]').val();
    }

    function dashIfEmpty(v) {
        return v ? v : '<span class="text-muted">—</span>';
    }

    function formatMoney(v) {
        return "$" + parseFloat(v).toFixed(2);
    }

    var columns = [
        {
            data: "imagePath",
            orderable: false,
            render: function (path) {
                if (!path) {
                    return '<div class="text-muted"><i class="fa-solid fa-image"></i></div>';
                }
                return '<img src="' + path + '" class="rounded" style="width:36px;height:36px;object-fit:cover;" />';
            }
        },
        { data: "name" },
        { data: "strength", render: dashIfEmpty },
        { data: "unitOfMeasure" },
        { data: "categoryName" },
        { data: "manufacturerName" },
        { data: "purchasePrice", render: formatMoney },
        { data: "sellingPrice", render: formatMoney },
        {
            data: "requiresPrescription",
            render: function (v) {
                return v ? '<span class="badge bg-danger">Rx</span>' : '<span class="badge bg-light text-dark">OTC</span>';
            }
        },
        {
            data: "barcode",
            render: function (v) {
                return v ? '<span class="font-monospace small">' + v + '</span>' : '<span class="text-muted">—</span>';
            }
        },
        {
            data: "isDeleted",
            render: function (isDeleted, type, row) {
                if (isDeleted) return '<span class="badge bg-secondary">Deleted</span>';
                return row.isActive
                    ? '<span class="badge bg-success">Active</span>'
                    : '<span class="badge bg-warning text-dark">Inactive</span>';
            }
        }
    ];

    if (canEdit) {
        columns.push({
            data: null,
            orderable: false,
            className: "text-end",
            render: function (row) {
                if (row.isDeleted) {
                    if (!canDelete) return "";
                    return '<button class="btn btn-sm btn-outline-success btn-restore" data-id="' + row.id + '">' +
                           '<i class="fa-solid fa-rotate-left"></i> Restore</button>';
                }
                var html = '<button class="btn btn-sm btn-outline-primary btn-edit me-1" data-id="' + row.id + '">' +
                    '<i class="fa-solid fa-pen"></i></button>';
                html += '<a class="btn btn-sm btn-outline-secondary me-1" href="/Medicines/PrintLabel?id=' + row.id + '" target="_blank">' +
                    '<i class="fa-solid fa-print"></i></a>';
                if (canDelete) {
                    html += '<button class="btn btn-sm btn-outline-danger btn-delete" data-id="' + row.id + '">' +
                        '<i class="fa-solid fa-trash"></i></button>';
                }
                return html;
            }
        });
    }

    var table = $("#medicinesTable").DataTable({
        ajax: {
            url: "/Medicines/GetAll",
            data: function (d) {
                d.includeDeleted = $("#chkShowDeleted").is(":checked");
            },
            dataSrc: "data"
        },
        columns: columns,
        order: [[1, "asc"]],
        responsive: true
    });

    $("#chkShowDeleted").on("change", function () {
        table.ajax.reload();
    });

    var optionsLoaded = false;

    function loadFormOptions(callback) {
        if (optionsLoaded) {
            callback();
            return;
        }

        $.get("/Medicines/GetFormOptions", function (data) {
            var $cat = $("#CategoryId");
            var $man = $("#ManufacturerId");

            $.each(data.categories, function (i, c) {
                $cat.append($("<option>", { value: c.id, text: c.name }));
            });
            $.each(data.manufacturers, function (i, m) {
                $man.append($("<option>", { value: m.id, text: m.name }));
            });

            optionsLoaded = true;
            callback();
        }).fail(function () {
            Swal.fire("Error", "Could not load categories/manufacturers.", "error");
        });
    }

    function resetForm() {
        $("#medicineForm")[0].reset();
        $("#Id").val(0);
        $("#IsActive").prop("checked", true);
        $("#RequiresPrescription").prop("checked", false);
        $(".text-danger.small").text("");
        $("#medicineModalTitle").text("Add Medicine");
        $("#imagePreview").addClass("d-none").attr("src", "");
        $("#barcodeSection").addClass("d-none");
    }

    $("#btnAddMedicine").on("click", function () {
        loadFormOptions(function () {
            resetForm();
            new bootstrap.Modal("#medicineModal").show();
        });
    });

    $("#medicinesTable").on("click", ".btn-edit", function () {
        var id = $(this).data("id");

        loadFormOptions(function () {
            $.get("/Medicines/GetById", { id: id }, function (data) {
                resetForm();
                $("#medicineModalTitle").text("Edit Medicine");
                $("#Id").val(data.id);
                $("#Name").val(data.name);
                $("#GenericName").val(data.genericName);
                $("#Strength").val(data.strength);
                $("#CategoryId").val(data.categoryId);
                $("#ManufacturerId").val(data.manufacturerId);
                $("#UnitOfMeasure").val(data.unitOfMeasure);
                $("#PurchasePrice").val(data.purchasePrice);
                $("#SellingPrice").val(data.sellingPrice);
                $("#RequiresPrescription").prop("checked", data.requiresPrescription);
                $("#IsActive").prop("checked", data.isActive);

                if (data.imagePath) {
                    $("#imagePreview").attr("src", data.imagePath).removeClass("d-none");
                }

                if (data.barcode) {
                    $("#barcodeSection").removeClass("d-none");
                    $("#barcodePreview").attr("src", data.barcodeImagePath);
                    $("#qrPreview").attr("src", data.qrCodeImagePath);
                    $("#barcodeValueText").text(data.barcode);
                    $("#btnPrintLabel").attr("href", "/Medicines/PrintLabel?id=" + data.id);
                }

                new bootstrap.Modal("#medicineModal").show();
            }).fail(function () {
                Swal.fire("Error", "Could not load medicine details.", "error");
            });
        });
    });

    $("#medicineForm").on("submit", function (e) {
        e.preventDefault();

        var id = parseInt($("#Id").val(), 10);
        var url = id > 0 ? "/Medicines/Edit" : "/Medicines/Create";

        var formData = new FormData();
        formData.append("Id", id);
        formData.append("Name", $("#Name").val());
        formData.append("GenericName", $("#GenericName").val());
        formData.append("Strength", $("#Strength").val());
        formData.append("CategoryId", $("#CategoryId").val());
        formData.append("ManufacturerId", $("#ManufacturerId").val());
        formData.append("UnitOfMeasure", $("#UnitOfMeasure").val());
        formData.append("PurchasePrice", $("#PurchasePrice").val());
        formData.append("SellingPrice", $("#SellingPrice").val());
        formData.append("RequiresPrescription", $("#RequiresPrescription").is(":checked"));
        formData.append("IsActive", $("#IsActive").is(":checked"));
        formData.append("__RequestVerificationToken", getToken());

        var fileInput = document.getElementById("ImageFile");
        if (fileInput.files.length > 0) {
            formData.append("ImageFile", fileInput.files[0]);
        }

        $("#btnSaveMedicine").prop("disabled", true);

        $.ajax({
            url: url,
            method: "POST",
            data: formData,
            processData: false,
            contentType: false
        })
            .done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(document.getElementById("medicineModal")).hide();
                    table.ajax.reload();
                    Swal.fire({ icon: "success", title: res.message, timer: 1800, showConfirmButton: false });
                } else {
                    Swal.fire("Could not save", res.message, "error");
                }
            })
            .fail(function () {
                Swal.fire("Error", "Something went wrong while saving. Please try again.", "error");
            })
            .always(function () {
                $("#btnSaveMedicine").prop("disabled", false);
            });
    });

    $("#medicinesTable").on("click", ".btn-delete", function () {
        var id = $(this).data("id");

        ptConfirm("This medicine will be moved to deleted items. You can restore it later.", function () {
            $.post("/Medicines/Delete", { id: id, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    table.ajax.reload();
                    Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
                })
                .fail(function () {
                    Swal.fire("Error", "Could not delete medicine.", "error");
                });
        });
    });

    $("#medicinesTable").on("click", ".btn-restore", function () {
        var id = $(this).data("id");

        $.post("/Medicines/Restore", { id: id, __RequestVerificationToken: getToken() })
            .done(function (res) {
                table.ajax.reload();
                Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
            })
            .fail(function () {
                Swal.fire("Error", "Could not restore medicine.", "error");
            });
    });
});
