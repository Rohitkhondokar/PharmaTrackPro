$(function () {
    function formatMoney(v) {
        return "$" + parseFloat(v).toFixed(2);
    }

    var statusBadge = {
        "Pending": '<span class="badge bg-warning text-dark">Pending</span>',
        "Received": '<span class="badge bg-success">Received</span>',
        "Cancelled": '<span class="badge bg-secondary">Cancelled</span>'
    };

    var table = $("#purchasesTable").DataTable({
        ajax: {
            url: "/Purchases/GetAll",
            dataSrc: "data"
        },
        columns: [
            { data: "id", render: function (id) { return "#" + id; } },
            { data: "supplierName" },
            { data: "invoiceNumber", render: function (v) { return v ? v : '<span class="text-muted">—</span>'; } },
            { data: "purchaseDate" },
            { data: "itemCount" },
            { data: "totalAmount", render: formatMoney },
            { data: "status", render: function (s) { return statusBadge[s] || s; } },
            {
                data: "id",
                orderable: false,
                className: "text-end",
                render: function (id) {
                    return '<a class="btn btn-sm btn-outline-primary" href="/Purchases/Details/' + id + '">' +
                        '<i class="fa-solid fa-eye"></i> View</a>';
                }
            }
        ],
        order: [[0, "desc"]],
        responsive: true
    });
});
