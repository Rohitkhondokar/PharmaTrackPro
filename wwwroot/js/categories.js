$(function () {
    var canManage = window.ptCanManageCategories === true;

    function getToken() {
        return $('#categoryForm input[name="__RequestVerificationToken"]').val();
    }

    var columns = [
        { data: "name" },
        {
            data: "description",
            render: function (d) { return d ? d : '<span class="text-muted">—</span>'; }
        },
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

    var table = $("#categoriesTable").DataTable({
        ajax: {
            url: "/Categories/GetAll",
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
        $("#categoryForm")[0].reset();
        $("#Id").val(0);
        $("#IsActive").prop("checked", true);
        $(".text-danger.small").text("");
        $("#categoryModalTitle").text("Add Category");
    }

    $("#btnAddCategory").on("click", function () {
        resetForm();
        new bootstrap.Modal("#categoryModal").show();
    });

    $("#categoriesTable").on("click", ".btn-edit", function () {
        var id = $(this).data("id");

        $.get("/Categories/GetById", { id: id }, function (data) {
            resetForm();
            $("#categoryModalTitle").text("Edit Category");
            $("#Id").val(data.id);
            $("#Name").val(data.name);
            $("#Description").val(data.description);
            $("#IsActive").prop("checked", data.isActive);
            new bootstrap.Modal("#categoryModal").show();
        }).fail(function () {
            Swal.fire("Error", "Could not load category details.", "error");
        });
    });

    $("#categoryForm").on("submit", function (e) {
        e.preventDefault();

        var id = parseInt($("#Id").val(), 10);
        var url = id > 0 ? "/Categories/Edit" : "/Categories/Create";

        var payload = {
            Id: id,
            Name: $("#Name").val(),
            Description: $("#Description").val(),
            IsActive: $("#IsActive").is(":checked"),
            __RequestVerificationToken: getToken()
        };

        $("#btnSaveCategory").prop("disabled", true);

        $.post(url, payload)
            .done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(document.getElementById("categoryModal")).hide();
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
                $("#btnSaveCategory").prop("disabled", false);
            });
    });

    $("#categoriesTable").on("click", ".btn-delete", function () {
        var id = $(this).data("id");

        ptConfirm("This category will be moved to deleted items. You can restore it later.", function () {
            $.post("/Categories/Delete", { id: id, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    table.ajax.reload();
                    Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
                })
                .fail(function () {
                    Swal.fire("Error", "Could not delete category.", "error");
                });
        });
    });

    $("#categoriesTable").on("click", ".btn-restore", function () {
        var id = $(this).data("id");

        $.post("/Categories/Restore", { id: id, __RequestVerificationToken: getToken() })
            .done(function (res) {
                table.ajax.reload();
                Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
            })
            .fail(function () {
                Swal.fire("Error", "Could not restore category.", "error");
            });
    });
});
