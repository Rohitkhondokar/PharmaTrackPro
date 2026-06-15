$(function () {
    function getToken() {
        return $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();
    }

    $("#btnReceive").on("click", function () {
        var id = $(this).data("id");

        ptConfirm("This will create stock batches for every line item on this order. Continue?", function () {
            $.post("/Purchases/Receive", { id: id, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    if (res.success) {
                        Swal.fire({ icon: "success", title: res.message, timer: 1500, showConfirmButton: false })
                            .then(function () { window.location.reload(); });
                    } else {
                        Swal.fire("Could not receive order", res.message, "error");
                    }
                })
                .fail(function () {
                    Swal.fire("Error", "Something went wrong.", "error");
                });
        });
    });

    $("#btnCancel").on("click", function () {
        var id = $(this).data("id");

        ptConfirm("This purchase order will be marked as cancelled. This cannot be undone.", function () {
            $.post("/Purchases/Cancel", { id: id, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    if (res.success) {
                        Swal.fire({ icon: "success", title: res.message, timer: 1500, showConfirmButton: false })
                            .then(function () { window.location.reload(); });
                    } else {
                        Swal.fire("Could not cancel order", res.message, "error");
                    }
                })
                .fail(function () {
                    Swal.fire("Error", "Something went wrong.", "error");
                });
        });
    });
});
