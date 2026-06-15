$(function () {
    function formatMoney(v) {
        return "$" + parseFloat(v).toFixed(2);
    }

    $("#saleReturnsTable").DataTable({
        ajax: {
            url: "/SaleReturns/GetAll",
            dataSrc: "data"
        },
        columns: [
            { data: "id", render: function (id) { return "#" + id; } },
            {
                data: "saleId",
                render: function (id) {
                    return '<a href="/Sales/Details/' + id + '">#' + id + '</a>';
                }
            },
            { data: "customerName" },
            { data: "returnDate" },
            { data: "reason" },
            { data: "itemCount" },
            { data: "totalRefundAmount", render: formatMoney },
            {
                data: "id",
                orderable: false,
                className: "text-end",
                render: function (id) {
                    return '<a class="btn btn-sm btn-outline-primary" href="/SaleReturns/Details/' + id + '">' +
                        '<i class="fa-solid fa-eye"></i> View</a>';
                }
            }
        ],
        order: [[0, "desc"]],
        responsive: true
    });
});
