$(function () {
    function getToken() {
        return $('#returnForm input[name="__RequestVerificationToken"]').val();
    }

    function formatMoney(v) {
        return "$" + (isNaN(v) ? "0.00" : parseFloat(v).toFixed(2));
    }

    function recalcRow($row) {
        var qty = parseFloat($row.find(".return-qty-input").val()) || 0;
        var cost = parseFloat($row.data("cost")) || 0;
        $row.find(".refund-cell").text(formatMoney(qty * cost));
        recalcGrandTotal();
    }

    function recalcGrandTotal() {
        var total = 0;
        $(".batch-row").each(function () {
            var qty = parseFloat($(this).find(".return-qty-input").val()) || 0;
            var cost = parseFloat($(this).data("cost")) || 0;
            total += qty * cost;
        });
        $("#grandTotal").text(formatMoney(total));
    }

    $(document).on("input", ".return-qty-input", function () {
        var $row = $(this).closest(".batch-row");
        var max = parseInt($row.data("max"), 10);
        var val = parseInt($(this).val(), 10) || 0;

        if (val > max) {
            $(this).val(max);
        }
        if (val < 0) {
            $(this).val(0);
        }

        recalcRow($row);
    });

    $("#returnForm").on("submit", function (e) {
        e.preventDefault();

        $(".text-danger.small").text("");

        if (!$("#Reason").val().trim()) {
            $("#ReasonError").text("A reason is required.");
            return;
        }

        var rows = [];
        $(".batch-row").each(function () {
            var qty = parseInt($(this).find(".return-qty-input").val(), 10) || 0;
            if (qty > 0) {
                rows.push({ BatchId: $(this).data("batch-id"), Quantity: qty });
            }
        });

        if (rows.length === 0) {
            $("#ItemsError").text("Enter a return quantity greater than zero for at least one item.");
            return;
        }

        var formData = new FormData();
        formData.append("PurchaseId", $("#PurchaseId").val());
        formData.append("ReturnDate", $("#ReturnDate").val());
        formData.append("Reason", $("#Reason").val());
        formData.append("__RequestVerificationToken", getToken());

        rows.forEach(function (item, i) {
            formData.append("Items[" + i + "].BatchId", item.BatchId);
            formData.append("Items[" + i + "].Quantity", item.Quantity);
        });

        $("#btnSaveReturn").prop("disabled", true);

        $.ajax({
            url: "/PurchaseReturns/Create",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false
        })
            .done(function (res) {
                if (res.success) {
                    Swal.fire({ icon: "success", title: res.message, timer: 1500, showConfirmButton: false })
                        .then(function () {
                            window.location.href = "/PurchaseReturns/Details/" + res.returnId;
                        });
                } else {
                    Swal.fire("Could not save return", res.message, "error");
                }
            })
            .fail(function () {
                Swal.fire("Error", "Something went wrong while saving. Please try again.", "error");
            })
            .always(function () {
                $("#btnSaveReturn").prop("disabled", false);
            });
    });
});
