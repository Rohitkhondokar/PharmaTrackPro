$(function () {
    var medicines = [];
    var cart = {}; // medicineId -> { medicine, quantity }

    function getToken() {
        return $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();
    }

    function formatMoney(v) {
        return "$" + (isNaN(v) ? "0.00" : parseFloat(v).toFixed(2));
    }

    function loadFormOptions() {
        $.get("/Sales/GetFormOptions", function (data) {
            medicines = data.medicines;

            var $customer = $("#CustomerId");
            $customer.find("option:not(:first)").remove();
            $.each(data.customers, function (i, c) {
                $customer.append($("<option>", { value: c.id, text: c.name }));
            });
        }).fail(function () {
            Swal.fire("Error", "Could not load medicines. Please refresh the page.", "error");
        });
    }

    function renderSearchResults(query) {
        var $results = $("#searchResults");
        $results.empty();

        if (!query) {
            $results.append('<div class="text-center text-muted py-4">Start typing to search medicines.</div>');
            return;
        }

        var matches = medicines.filter(function (m) {
            return m.name.toLowerCase().indexOf(query) !== -1;
        }).slice(0, 30);

        if (matches.length === 0) {
            $results.append('<div class="text-center text-muted py-4">No medicines match "' + query + '".</div>');
            return;
        }

        $.each(matches, function (i, m) {
            var outOfStock = m.availableQuantity <= 0;
            var rxBadge = m.requiresPrescription ? '<span class="badge bg-danger ms-2">Rx</span>' : "";
            var stockBadge = outOfStock
                ? '<span class="badge bg-secondary ms-2">Out of Stock</span>'
                : '<span class="badge bg-light text-dark ms-2">' + m.availableQuantity + ' in stock</span>';

            var item = $(
                '<button type="button" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center' +
                (outOfStock ? " disabled" : "") + '" data-id="' + m.id + '">' +
                '<span>' + m.name + (m.strength ? ' <span class="text-muted">(' + m.strength + ')</span>' : "") + rxBadge + stockBadge + '</span>' +
                '<span class="fw-semibold">' + formatMoney(m.sellingPrice) + '</span>' +
                '</button>'
            );

            $results.append(item);
        });
    }

    $("#medicineSearch").on("input", function () {
        renderSearchResults($(this).val().trim().toLowerCase());
    });

    $("#searchResults").on("click", ".list-group-item:not(.disabled)", function () {
        var id = $(this).data("id");
        var medicine = medicines.find(function (m) { return m.id === id; });
        if (!medicine) return;

        if (cart[id]) {
            if (cart[id].quantity < medicine.availableQuantity) {
                cart[id].quantity++;
            }
        } else {
            cart[id] = { medicine: medicine, quantity: 1 };
        }

        renderCart();
    });

    function renderCart() {
        var ids = Object.keys(cart);
        var $body = $("#cartBody");
        $body.empty();

        if (ids.length === 0) {
            $("#cartTable").addClass("d-none");
            $("#emptyCartMessage").removeClass("d-none");
            $("#btnCheckout").prop("disabled", true);
            $("#rxWarning").addClass("d-none");
            $("#grandTotal").text(formatMoney(0));
            return;
        }

        $("#cartTable").removeClass("d-none");
        $("#emptyCartMessage").addClass("d-none");

        var total = 0;
        var anyRx = false;

        ids.forEach(function (id) {
            var entry = cart[id];
            var subtotal = entry.quantity * entry.medicine.sellingPrice;
            total += subtotal;
            if (entry.medicine.requiresPrescription) anyRx = true;

            var row = $(
                '<tr data-id="' + id + '">' +
                '<td>' + entry.medicine.name + (entry.medicine.strength ? ' <span class="text-muted small">(' + entry.medicine.strength + ')</span>' : "") + '</td>' +
                '<td><input type="number" class="form-control form-control-sm cart-qty-input" min="1" max="' + entry.medicine.availableQuantity + '" value="' + entry.quantity + '" /></td>' +
                '<td class="text-end">' + formatMoney(entry.medicine.sellingPrice) + '</td>' +
                '<td class="text-end cart-subtotal">' + formatMoney(subtotal) + '</td>' +
                '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger btn-remove-cart-item"><i class="fa-solid fa-xmark"></i></button></td>' +
                '</tr>'
            );
            $body.append(row);
        });

        $("#grandTotal").text(formatMoney(total));

        if (anyRx) {
            $("#rxWarning").removeClass("d-none");
        } else {
            $("#rxWarning").addClass("d-none");
            $("#PrescriptionVerified").prop("checked", false);
        }

        updateCheckoutButtonState();
    }

    function updateCheckoutButtonState() {
        var hasItems = Object.keys(cart).length > 0;
        var rxOk = $("#rxWarning").hasClass("d-none") || $("#PrescriptionVerified").is(":checked");
        $("#btnCheckout").prop("disabled", !(hasItems && rxOk));
    }

    $("#cartBody").on("input", ".cart-qty-input", function () {
        var id = $(this).closest("tr").data("id");
        var entry = cart[id];
        var max = entry.medicine.availableQuantity;
        var val = parseInt($(this).val(), 10) || 1;

        if (val > max) val = max;
        if (val < 1) val = 1;

        $(this).val(val);
        entry.quantity = val;
        renderCart();
    });

    $("#cartBody").on("click", ".btn-remove-cart-item", function () {
        var id = $(this).closest("tr").data("id");
        delete cart[id];
        renderCart();
    });

    $("#rxWarning").on("change", "#PrescriptionVerified", function () {
        updateCheckoutButtonState();
    });

    $("#btnCheckout").on("click", function () {
        var ids = Object.keys(cart);
        if (ids.length === 0) return;

        var formData = new FormData();
        formData.append("CustomerId", $("#CustomerId").val());
        formData.append("PaymentMethod", $("#PaymentMethod").val());
        formData.append("PrescriptionVerified", $("#PrescriptionVerified").is(":checked"));
        formData.append("__RequestVerificationToken", getToken());

        ids.forEach(function (id, i) {
            formData.append("Items[" + i + "].MedicineId", id);
            formData.append("Items[" + i + "].Quantity", cart[id].quantity);
        });

        $("#btnCheckout").prop("disabled", true);

        $.ajax({
            url: "/Sales/Checkout",
            method: "POST",
            data: formData,
            processData: false,
            contentType: false
        })
            .done(function (res) {
                if (res.success) {
                    var total = $("#grandTotal").text();
                    Swal.fire({
                        icon: "success",
                        title: "Sale #" + res.saleId + " completed",
                        text: "Total: " + total,
                        showDenyButton: true,
                        confirmButtonText: "New Sale",
                        denyButtonText: "Print Receipt"
                    }).then(function (result) {
                        if (result.isDenied) {
                            window.open("/Sales/Receipt/" + res.saleId, "_blank");
                        }
                        cart = {};
                        $("#medicineSearch").val("");
                        renderSearchResults("");
                        renderCart();
                        loadFormOptions(); // refresh stock counts
                    });
                } else {
                    Swal.fire("Could not complete sale", res.message, "error");
                    updateCheckoutButtonState();
                }
            })
            .fail(function () {
                Swal.fire("Error", "Something went wrong while processing the sale.", "error");
                updateCheckoutButtonState();
            });
    });

    loadFormOptions();
    renderCart();
});
