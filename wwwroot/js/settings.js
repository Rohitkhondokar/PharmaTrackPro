$(function () {
    function getToken() {
        return $('#settingsForm input[name="__RequestVerificationToken"]').val();
    }

    $("#settingsForm").on("submit", function (e) {
        e.preventDefault();

        $(".text-danger.small").text("");

        var payload = {
            PharmacyName: $("#PharmacyName").val(),
            Address: $("#Address").val(),
            Phone: $("#Phone").val(),
            Email: $("#Email").val(),
            LowStockThreshold: $("#LowStockThreshold").val(),
            NearExpiryDaysThreshold: $("#NearExpiryDaysThreshold").val(),
            CurrencySymbol: $("#CurrencySymbol").val(),
            __RequestVerificationToken: getToken()
        };

        $("#btnSaveSettings").prop("disabled", true);

        $.post("/Settings/Index", payload)
            .done(function (res) {
                if (res.success) {
                    Swal.fire({ icon: "success", title: res.message, timer: 1800, showConfirmButton: false })
                        .then(function () { window.location.reload(); });
                } else {
                    Swal.fire("Could not save", res.message, "error");
                }
            })
            .fail(function () {
                Swal.fire("Error", "Something went wrong while saving. Please try again.", "error");
            })
            .always(function () {
                $("#btnSaveSettings").prop("disabled", false);
            });
    });
});
