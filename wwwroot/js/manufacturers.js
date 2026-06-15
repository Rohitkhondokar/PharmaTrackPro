$(function () {
    var canManage = window.ptCanManageManufacturers === true;

    function getToken() {
        return $('#manufacturerForm input[name="__RequestVerificationToken"]').val();
    }

    function dashIfEmpty(v) {
        return v ? v : '<span class="text-muted">—</span>';
    }

    var columns = [
        { data: "name" },
        { data: "contactPerson", render: dashIfEmpty },
        { data: "phone", render: dashIfEmpty },
        { data: "email", render: dashIfEmpty },
        {
            data: "isDeleted",
            render: function (isDeleted, type, row) {
                if (isDeleted) return '<span class="badge bg-secondary">Deleted</span>';
                return row.isActive
                    ? '<span class="badge bg-success">Active</span>'
                    : '<span class="badge bg-warning text-dark">Inactive</span>';
            }
        },
        { data: "createdAt" }
    ];

    if (canManage) {
        columns.push({
            data: null,
            orderable: false,
            className: "text-end",
            render: function (row) {
                if (row.isDeleted) {
                    return '<button class="btn btn-sm btn-outline-success btn-restore" data-id="' + row.id + '">' +
                           '<i class="fa-solid fa-rotate-left"></i> Restore</button>';
                }
                return (
                    '<button class="btn btn-sm btn-outline-primary btn-edit me-1" data-id="' + row.id + '">' +
                    '<i class="fa-solid fa-pen"></i></button>' +
                    '<button class="btn btn-sm btn-outline-danger btn-delete" data-id="' + row.id + '">' +
                    '<i class="fa-solid fa-trash"></i></button>'
                );
            }
        });
    }

    var table = $("#manufacturersTable").DataTable({
        ajax: {
            url: "/Manufacturers/GetAll",
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
        $("#manufacturerForm")[0].reset();
        $("#Id").val(0);
        $("#IsActive").prop("checked", true);
        $(".text-danger.small").text("");
        $("#manufacturerModalTitle").text("Add Manufacturer");
    }

    $("#btnAddManufacturer").on("click", function () {
        resetForm();
        new bootstrap.Modal("#manufacturerModal").show();
    });

    $("#manufacturersTable").on("click", ".btn-edit", function () {
        var id = $(this).data("id");

        $.get("/Manufacturers/GetById", { id: id }, function (data) {
            resetForm();
            $("#manufacturerModalTitle").text("Edit Manufacturer");
            $("#Id").val(data.id);
            $("#Name").val(data.name);
            $("#ContactPerson").val(data.contactPerson);
            $("#Phone").val(data.phone);
            $("#Email").val(data.email);
            $("#Address").val(data.address);
            $("#IsActive").prop("checked", data.isActive);
            new bootstrap.Modal("#manufacturerModal").show();
        }).fail(function () {
            Swal.fire("Error", "Could not load manufacturer details.", "error");
        });
    });

    $("#manufacturerForm").on("submit", function (e) {
        e.preventDefault();

        var id = parseInt($("#Id").val(), 10);
        var url = id > 0 ? "/Manufacturers/Edit" : "/Manufacturers/Create";

        var payload = {
            Id: id,
            Name: $("#Name").val(),
            ContactPerson: $("#ContactPerson").val(),
            Phone: $("#Phone").val(),
            Email: $("#Email").val(),
            Address: $("#Address").val(),
            IsActive: $("#IsActive").is(":checked"),
            __RequestVerificationToken: getToken()
        };

        $("#btnSaveManufacturer").prop("disabled", true);

        $.post(url, payload)
            .done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(document.getElementById("manufacturerModal")).hide();
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
                $("#btnSaveManufacturer").prop("disabled", false);
            });
    });

    $("#manufacturersTable").on("click", ".btn-delete", function () {
        var id = $(this).data("id");

        ptConfirm("This manufacturer will be moved to deleted items. You can restore it later.", function () {
            $.post("/Manufacturers/Delete", { id: id, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    table.ajax.reload();
                    Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
                })
                .fail(function () {
                    Swal.fire("Error", "Could not delete manufacturer.", "error");
                });
        });
    });

    $("#manufacturersTable").on("click", ".btn-restore", function () {
        var id = $(this).data("id");

        $.post("/Manufacturers/Restore", { id: id, __RequestVerificationToken: getToken() })
            .done(function (res) {
                table.ajax.reload();
                Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
            })
            .fail(function () {
                Swal.fire("Error", "Could not restore manufacturer.", "error");
            });
    });
});
