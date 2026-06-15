$(function () {
    function formatMoney(v) {
        return "$" + parseFloat(v).toFixed(2);
    }

    $("#salesTable").DataTable({
        ajax: {
            url: "/Sales/GetAllSales",
            dataSrc: "data"
        },
        columns: [
            { data: "id", render: function (id) { return "#" + id; } },
            { data: "customerName" },
            { data: "cashierName" },
            { data: "saleDate" },
            { data: "paymentMethod" },
            { data: "itemCount" },
            { data: "totalAmount", className: "text-end", render: formatMoney },
            {
                data: "id",
                orderable: false,
                className: "text-end",
                render: function (id) {
                    return '<a class="btn btn-sm btn-outline-primary me-1" href="/Sales/Details/' + id + '">' +
                        '<i class="fa-solid fa-eye"></i> View</a>' +
                        '<a class="btn btn-sm btn-outline-secondary" href="/Sales/Receipt/' + id + '" target="_blank">' +
                        '<i class="fa-solid fa-print"></i></a>';
                }
            }
        ],
        order: [[0, "desc"]],
        responsive: true
    });
});
