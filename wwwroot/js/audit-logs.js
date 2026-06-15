$(function () {
    var table = $("#auditLogsTable").DataTable({
        ajax: {
            url: "/AuditLogs/GetAll",
            data: function (d) {
                d.action = $("#filterAction").val();
                d.entityType = $("#filterEntityType").val();
                d.from = $("#filterFrom").val();
                d.to = $("#filterTo").val();
            },
            dataSrc: "data"
        },
        columns: [
            { data: "createdAt" },
            { data: "userName" },
            {
                data: "action",
                render: function (v) {
                    return '<span class="badge bg-light text-dark border">' + v + '</span>';
                }
            },
            {
                data: null,
                render: function (row) {
                    return row.entityType + (row.entityId ? " #" + row.entityId : "");
                }
            },
            { data: "details", render: function (v) { return v || '<span class="text-muted">—</span>'; } },
            { data: "ipAddress", render: function (v) { return v || '<span class="text-muted">—</span>'; } }
        ],
        order: [[0, "desc"]],
        responsive: true
    });

    $("#btnApplyFilter").on("click", function () {
        table.ajax.reload();
    });
});
