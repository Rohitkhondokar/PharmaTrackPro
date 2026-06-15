$(function () {
    var canManage = window.ptCanManageExpiry === true;
    var nearExpiryDays = window.ptNearExpiryDays;
    var allRows = [];
    var currentFilter = "expired";

    function getToken() {
        return $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();
    }

    function formatMoney(v) {
        return "$" + parseFloat(v).toFixed(2);
    }

    var columns = [
        {
            data: null,
            render: function (row) {
                return row.medicineName + (row.strength ? ' <span class="text-muted">(' + row.strength + ')</span>' : "");
            }
        },
        { data: "batchNumber", render: function (v) { return '<span class="font-monospace small">' + v + '</span>'; } },
        {
            data: "expiryDate",
            render: function (v, type, row) {
                var cls = row.isExpired ? "text-danger fw-bold" : (row.daysUntilExpiry <= nearExpiryDays ? "text-warning fw-semibold" : "");
                return '<span class="' + cls + '">' + v + '</span>';
            }
        },
        {
            data: "daysUntilExpiry",
            className: "text-end",
            render: function (v, type, row) {
                return row.isExpired ? "Expired " + Math.abs(v) + "d ago" : v + "d left";
            }
        },
        { data: "quantityRemaining", className: "text-end" },
        { data: "unitCost", className: "text-end", render: formatMoney }
    ];

    if (canManage) {
        columns.push({
            data: null,
            orderable: false,
            className: "text-end",
            render: function (row) {
                if (!row.isExpired) return "";
                return '<button class="btn btn-sm btn-outline-danger btn-writeoff" data-id="' + row.batchId + '" data-name="' + row.medicineName + '">' +
                    '<i class="fa-solid fa-ban"></i> Write Off</button>';
            }
        });
    }

    var table = $("#expiryTable").DataTable({
        data: [],
        columns: columns,
        order: [[2, "asc"]],
        responsive: true
    });

    function applyFilter(filter) {
        currentFilter = filter;
        var filtered = allRows;

        if (filter === "expired") {
            filtered = allRows.filter(function (r) { return r.isExpired; });
        } else if (filter === "near") {
            filtered = allRows.filter(function (r) { return !r.isExpired && r.daysUntilExpiry <= nearExpiryDays; });
        }

        table.clear();
        table.rows.add(filtered);
        table.draw();
    }

    function updateCounts() {
        $("#countExpired").text(allRows.filter(function (r) { return r.isExpired; }).length);
        $("#countNear").text(allRows.filter(function (r) { return !r.isExpired && r.daysUntilExpiry <= nearExpiryDays; }).length);
        $("#countAll").text(allRows.length);
    }

    function loadData() {
        $.get("/Expiry/GetAll", function (res) {
            allRows = res.data;
            updateCounts();
            applyFilter(currentFilter);
        }).fail(function () {
            Swal.fire("Error", "Could not load the expiry report.", "error");
        });
    }

    $("#expiryTabs .nav-link").on("click", function () {
        $("#expiryTabs .nav-link").removeClass("active");
        $(this).addClass("active");
        applyFilter($(this).data("filter"));
    });

    $("#expiryTable").on("click", ".btn-writeoff", function () {
        var batchId = $(this).data("id");
        var name = $(this).data("name");

        ptConfirm("This will zero out the remaining quantity for this expired batch of " + name + ". Continue?", function () {
            $.post("/Expiry/WriteOff", { batchId: batchId, __RequestVerificationToken: getToken() })
                .done(function (res) {
                    if (res.success) {
                        Swal.fire({ icon: "success", title: res.message, timer: 1500, showConfirmButton: false });
                        loadData();
                    } else {
                        Swal.fire("Could not write off", res.message, "error");
                    }
                })
                .fail(function () {
                    Swal.fire("Error", "Something went wrong.", "error");
                });
        });
    });

    loadData();
});
