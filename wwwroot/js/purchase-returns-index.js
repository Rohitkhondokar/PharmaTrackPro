$(function () {
    function formatMoney(v) {
        return "$" + parseFloat(v).toFixed(2);
    }

    $("#purchaseReturnsTable").DataTable({
        ajax: {
            url: "/PurchaseReturns/GetAll",
            dataSrc: "data"
        },
        columns: [
            { data: "id", render: function (id) { return "#" + id; } },
            {
                data: "purchaseId",
                render: function (id) {
                    return '<a href="/Purchases/Details/' + id + '">#' + id + '</a>';
                }
            },
            { data: "supplierName" },
            { data: "returnDate" },
            { data: "reason" },
            { data: "itemCount" },
            { data: "totalRefundAmount", render: formatMoney },
            {
                data: "id",
                orderable: false,
                className: "text-end",
                render: function (id) {
                    return '<a class="btn btn-sm btn-outline-primary" href="/PurchaseReturns/Details/' + id + '">' +
                        '<i class="fa-solid fa-eye"></i> View</a>';
                }
            }
        ],
        order: [[0, "desc"]],
        responsive: true
    });
});
