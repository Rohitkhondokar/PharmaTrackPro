$(function () {
    var rolesLoaded = false;

    function getToken() {
        return $('#userForm input[name="__RequestVerificationToken"]').val();
    }

    function dashIfEmpty(v) {
        return v ? v : '<span class="text-muted">—</span>';
    }

    var table = $("#usersTable").DataTable({
        ajax: {
            url: "/Users/GetAll",
            data: function (d) {
                d.includeDeleted = $("#chkShowDeleted").is(":checked");
            },
            dataSrc: "data"
        },
        columns: [
            { data: "fullName" },
            { data: "email" },
            { data: "role", render: function (v) { return '<span class="badge bg-light text-dark border">' + v + '</span>'; } },
            {
                data: "isDeleted",
                render: function (isDeleted, type, row) {
                    if (isDeleted) return '<span class="badge bg-secondary">Deleted</span>';
                    return row.isActive
                        ? '<span class="badge bg-success">Active</span>'
                        : '<span class="badge bg-warning text-dark">Inactive</span>';
                }
            },
            { data: "lastLoginAt", render: function (v) { return v ? new Date(v).toLocaleString() : dashIfEmpty(v); } },
            {
                data: null,
                orderable: false,
                className: "text-end",
                render: function (row) {
                    if (row.isDeleted) {
                        return '<button class="btn btn-sm btn-outline-success btn-restore" data-id="' + row.id + '">' +
                               '<i class="fa-solid fa-rotate-left"></i> Restore</button>';
                    }
                    return (
                        '<button class="btn btn-sm btn-outline-primary btn-edit me-1" data-id="' + row.id + '" title="Edit">' +
                        '<i class="fa-solid fa-pen"></i></button>' +
                        '<button class="btn btn-sm btn-outline-secondary btn-reset-password me-1" data-id="' + row.id + '" data-name="' + row.fullName + '" title="Reset Password">' +
                        '<i class="fa-solid fa-key"></i></button>' +
                        '<button class="btn btn-sm btn-outline-warning btn-toggle-active me-1" data-id="' + row.id + '" title="' + (row.isActive ? "Deactivate" : "Activate") + '">' +
                        '<i class="fa-solid fa-power-off"></i></button>' +
                        '<button class="btn btn-sm btn-outline-danger btn-delete" data-id="' + row.id + '" title="Delete">' +
                        '<i class="fa-solid fa-trash"></i></button>'
                    );
                }
            }
        ],
        order: [[0, "asc"]],
        responsive: true
    });

    $("#chkShowDeleted").on("change", function () {
        table.ajax.reload();
    });

    function loadRoles(callback) {
        if (rolesLoaded) {
            callback();
            return;
        }
        $.get("/Users/GetRoles", function (roles) {
            var $role = $("#Role");
            $.each(roles, function (i, r) {
                $role.append($("<option>", { value: r, text: r }));
            });
            rolesLoaded = true;
            callback();
        });
    }

    function resetForm() {
        $("#userForm")[0].reset();
        $("#Id").val("");
        $("#IsActive").prop("checked", true);
        $(".text-danger.small").text("");
        $("#userModalTitle").text("Add User");
        $("#passwordGroup").removeClass("d-none");
        $("#Password").prop("required", true);
    }

    $("#btnAddUser").on("click", function () {
        loadRoles(function () {
            resetForm();
            new bootstrap.Modal("#userModal").show();
        });
    });

    $("#usersTable").on("click", ".btn-edit", function () {
        var id = $(this).data("id");

        loadRoles(function () {
            $.get("/Users/GetById", { id: id }, function (data) {
                resetForm();
                $("#userModalTitle").text("Edit User");
                $("#Id").val(data.id);
                $("#FullName").val(data.fullName);
                $("#Email").val(data.email);
                $("#Role").val(data.role);
                $("#IsActive").prop("checked", data.isActive);

                // Password isn't editable here — use Reset Password instead
                $("#passwordGroup").addClass("d-none");
                $("#Password").prop("required", false).val("");

                new bootstrap.Modal("#userModal").show();
            }).fail(function () {
                Swal.fire("Error", "Could not load user details.", "error");
            });
        });
    });

    $("#userForm").on("submit", function (e) {
        e.preventDefault();

        var id = $("#Id").val();
        var url = id ? "/Users/Edit" : "/Users/Create";

        var payload = {
            Id: id,
            FullName: $("#FullName").val(),
            Email: $("#Email").val(),
            Role: $("#Role").val(),
            IsActive: $("#IsActive").is(":checked"),
            Password: $("#Password").val(),
            __RequestVerificationToken: getToken()
        };

        $("#btnSaveUser").prop("disabled", true);

        $.post(url, payload)
            .done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(document.getElementById("userModal")).hide();
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
                $("#btnSaveUser").prop("disabled", false);
            });
    });

    $("#usersTable").on("click", ".btn-toggle-active", function () {
        var id = $(this).data("id");

        $.post("/Users/ToggleActive", { id: id, __RequestVerificationToken: getToken() })
            .done(function (res) {
                table.ajax.reload();
                Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
            })
            .fail(function () {
                Swal.fire("Error", "Could not update user status.", "error");
            });
    });

    $("#usersTable").on("click", ".btn-delete", function () {
        var id = $(this).data("id");

        ptConfirm("This user account will be deactivated and moved to deleted items.", function () {
            $.post("/Users/Delete", { id: id, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    table.ajax.reload();
                    Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
                })
                .fail(function () {
                    Swal.fire("Error", "Could not delete user.", "error");
                });
        });
    });

    $("#usersTable").on("click", ".btn-restore", function () {
        var id = $(this).data("id");

        $.post("/Users/Restore", { id: id, __RequestVerificationToken: getToken() })
            .done(function (res) {
                table.ajax.reload();
                Swal.fire({ icon: res.success ? "success" : "error", title: res.message, timer: 1800, showConfirmButton: false });
            })
            .fail(function () {
                Swal.fire("Error", "Could not restore user.", "error");
            });
    });

    // ---------------- Reset Password ----------------

    $("#usersTable").on("click", ".btn-reset-password", function () {
        var id = $(this).data("id");
        var name = $(this).data("name");

        $("#ResetUserId").val(id);
        $("#NewPassword").val("");
        $(".text-danger.small").text("");
        $("#resetPasswordUserInfo").text("Setting a new password for " + name + ".");

        new bootstrap.Modal("#resetPasswordModal").show();
    });

    $("#resetPasswordForm").on("submit", function (e) {
        e.preventDefault();

        var newPassword = $("#NewPassword").val();
        if (!newPassword || newPassword.length < 8) {
            $("#NewPasswordError").text("Password must be at least 8 characters.");
            return;
        }

        $("#btnSaveResetPassword").prop("disabled", true);

        $.post("/Users/ResetPassword", {
            UserId: $("#ResetUserId").val(),
            NewPassword: newPassword,
            __RequestVerificationToken: getToken()
        })
            .done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(document.getElementById("resetPasswordModal")).hide();
                    Swal.fire({ icon: "success", title: res.message, timer: 1800, showConfirmButton: false });
                } else {
                    Swal.fire("Could not reset password", res.message, "error");
                }
            })
            .fail(function () {
                Swal.fire("Error", "Something went wrong. Please try again.", "error");
            })
            .always(function () {
                $("#btnSaveResetPassword").prop("disabled", false);
            });
    });
});
