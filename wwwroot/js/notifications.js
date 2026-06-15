function getGlobalAntiForgeryToken() {
    return $('#globalAntiForgeryForm input[name="__RequestVerificationToken"]').val();
}

var notifIcons = {
    LowStock: '<i class="fa-solid fa-triangle-exclamation text-danger"></i>',
    NearExpiry: '<i class="fa-solid fa-clock text-warning"></i>',
    Expired: '<i class="fa-solid fa-hourglass-end text-secondary"></i>',
    PendingPurchase: '<i class="fa-solid fa-cart-shopping text-primary"></i>'
};

function renderNotificationItem(n, compact) {
    var icon = notifIcons[n.type] || '<i class="fa-solid fa-bell"></i>';
    var unreadClass = n.isRead ? "" : "fw-semibold bg-light";

    return (
        '<div class="notif-item px-3 py-2 border-bottom ' + unreadClass + '" data-id="' + n.id + '" data-link="' + (n.link || "") + '" style="cursor:pointer;">' +
        '<div class="d-flex gap-2">' +
        '<div>' + icon + '</div>' +
        '<div class="flex-grow-1">' +
        '<div class="small">' + n.title + '</div>' +
        (n.message ? '<div class="text-muted" style="font-size:0.75rem;">' + n.message + '</div>' : "") +
        '<div class="text-muted" style="font-size:0.7rem;">' + n.createdAt + '</div>' +
        '</div>' +
        (n.isRead ? "" : '<span class="badge bg-primary rounded-circle p-1" style="width:8px;height:8px;"></span>') +
        '</div>' +
        '</div>'
    );
}

function markNotificationRead(id, callback) {
    $.post("/Notifications/MarkAsRead", { id: id, __RequestVerificationToken: getGlobalAntiForgeryToken() })
        .always(function () {
            if (callback) callback();
        });
}

function markAllNotificationsRead(callback) {
    $.post("/Notifications/MarkAllAsRead", { __RequestVerificationToken: getGlobalAntiForgeryToken() })
        .always(function () {
            loadBellNotifications();
            if (callback) callback();
        });
}

function loadBellNotifications() {
    var $badge = $("#notifBadge");
    var $list = $("#notificationList");

    if ($list.length === 0) return; // bell not present on this layout render

    $.get("/Notifications/GetRecent", function (res) {
        if (res.unreadCount > 0) {
            $badge.text(res.unreadCount > 99 ? "99+" : res.unreadCount).removeClass("d-none");
        } else {
            $badge.addClass("d-none");
        }

        $list.empty();

        if (res.data.length === 0) {
            $list.append('<div class="text-center text-muted small py-3">No notifications right now.</div>');
            return;
        }

        $.each(res.data, function (i, n) {
            $list.append(renderNotificationItem(n));
        });
    });
}

function loadFullNotificationList() {
    var $container = $("#fullNotificationList");
    if ($container.length === 0) return;

    $.get("/Notifications/GetAll", function (res) {
        $container.empty();

        if (res.data.length === 0) {
            $container.append('<div class="text-center text-muted py-4">No notifications right now.</div>');
            return;
        }

        $.each(res.data, function (i, n) {
            $container.append(renderNotificationItem(n));
        });
    });
}

$(function () {
    // Bell dropdown (present on every authenticated page via _Layout.cshtml)
    loadBellNotifications();
    setInterval(loadBellNotifications, 60000); // refresh every 60s

    $(document).on("click", ".notif-item", function () {
        var id = $(this).data("id");
        var link = $(this).data("link");

        markNotificationRead(id, function () {
            loadBellNotifications();
            if (typeof loadFullNotificationList === "function") {
                loadFullNotificationList();
            }
            if (link) {
                window.location.href = link;
            }
        });
    });

    $("#btnMarkAllRead").on("click", function (e) {
        e.stopPropagation();
        markAllNotificationsRead();
    });
});
