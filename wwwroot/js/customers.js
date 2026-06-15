$(function () {
    var canManage = window.ptCanManageCustomers === true;
    var canDelete = window.ptCanDeleteCustomers === true;

    function getToken() {
        return $('#customerForm input[name="__RequestVerificationToken"]').val();
    }

    function dashIfEmpty(v) {
        return v ? v : '<span class="text-muted">—</span>';
    }

    var columns = [
        { data: "name" },
        { data: "phone", render: dashIfEmpty },
        { data: "email", render: dashIfEmpty },
        { data: "dateOfBirth", render: dashIfEmpty },
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

    if (canManage) {
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
                if (canDelete) {
                    html += '<button class="btn btn-sm btn-outline-danger btn-delete" data-id="' + row.id + '">' +
                        '<i class="fa-solid fa-trash"></i></button>';
                }
                return html;
            }
        });
    }

    var table = $("#customersTable").DataTable({
        ajax: {
            url: "/Customers/GetAll",
            data: function (d) {
                d.includeDeleted = $("#chkShowDeleted").is(":checked");
            },
            dataSrc: "data"
        },
        columns: columns,
        order: [[0, "asc"]],
        responsive: true
    });

    $("#chkShowDeleted").on("change", function () {
        table.ajax.reload();
    });

    function resetForm() {
        $("#customerForm")[0].reset();
        $("#Id").val(0);
        $("#IsActive").prop("checked", true);
        $(".text-danger.small").text("");
        $("#customerModalTitle").text("Add Customer");
    }

    $("#btnAddCustomer").on("click", function () {
        resetForm();
        new bootstrap.Modal("#customerModal").show();
    });

    $("#customersTable").on("click", ".btn-edit", function () {
        var id = $(this).data("id");

        $.get("/Customers/GetById", { id: id }, function (data) {
            resetForm();
            $("#customerModalTitle").text("Edit Customer");
            $("#Id").val(data.id);
            $("#Name").val(data.name);
            $("#Phone").val(data.phone);
            $("#Email").val(data.email);
            $("#Address").val(data.address);
            $("#DateOfBirth").val(data.dateOfBirth);
            $("#MedicalNotes").val(data.medicalNotes);
            $("#IsActive").prop("checked", data.isActive);
            new bootstrap.Modal("#customerModal").show();
        }).fail(function () {
            Swal.fire("Error", "Could not load customer details.", "error");
        });
    });

    $("#customerForm").on("submit", function (e) {
        e.preventDefault();

        var id = parseInt($("#Id").val(), 10);
        var url = id > 0 ? "/Customers/Edit" : "/Customers/Create";

        var payload = {
            Id: id,
            Name: $("#Name").val(),
            Phone: $("#Phone").val(),
            Email: $("#Email").val(),
            Address: $("#Address").val(),
            DateOfBirth: $("#DateOfBirth").val(),
            MedicalNotes: $("#MedicalNotes").val(),
            IsActive: $("#IsActive").is(":checked"),
            __RequestVerificationToken: getToken()
        };

        $("#btnSaveCustomer").prop("disabled", true);

        $.post(url, payload)
            .done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(document.getElementById("customerModal")).hide();
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
                $("#btnSaveCustomer").prop("disabled", false);
            });
    });

    $("#customersTable").on("click", ".btn-delete", function () {
        var id = $(this).data("id");

        ptConfirm("This customer will be moved to deleted items. You can restore it later.", function () {
            $.post("/Customers/Delete", { id: id, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    table.ajax.reload();
                    Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
                })
                .fail(function () {
                    Swal.fire("Error", "Could not delete customer.", "error");
                });
        });
    });

    $("#customersTable").on("click", ".btn-restore", function () {
        var id = $(this).data("id");

        $.post("/Customers/Restore", { id: id, __RequestVerificationToken: getToken() })
            .done(function (res) {
                table.ajax.reload();
                Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
            })
            .fail(function () {
                Swal.fire("Error", "Could not restore customer.", "error");
            });
    });
});
