$("#notification_icon").click(function () {
    const len = $(this).data("len");

    GetNotifis(0, len).then((res) => {
        if (!res || res.code != 0) {
            return;
        }
        RenNewMessage(res.data);
    })
})

$("#notification_see_more").click(function () {
    const notiIcon = $("#notification_icon");
    const len = notiIcon.data("len");
    const start = $("#noti_container li").last().data("content");
    console.log(start)

    GetNotifis(start, 20).then((res) => {
        if (!res || res.code !== 0 || !res.data) {
            return;
        }
        notiIcon.data("len", len + 20);
        RenMoreMessage(res.data);
    })
})

function GetNotifis(start, len) {
    return $.ajax({
        url: "/trang-chu/get-noti",
        type: "GET",
        data: {
            startNotifiId: start,
            length: len
        }
    })
}

function RenNewMessage(data) {
    console.log(data);
    const container = $("#noti_container");
    container.empty();

    if (!data || data.length === 0) {
        container.append("<li class='text-center' style='font-size:13px;'>Không có thông báo</li>");
        return;
    }

    Array.from(data).forEach((noti) => {
        const notiText = RendOneNoti(noti.id, noti.notification.title, noti.createddate, noti.notificationcontent, noti.link);
        container.append(notiText);
    })
}

function RenMoreMessage(data) {
    console.log(data);
    const container = $("#noti_container");

    Array.from(data).forEach((noti) => {
        const notiText = RendOneNoti(noti.id, noti.notification.title, noti.createddate, noti.notificationcontent, noti.link);
        container.prepend(notiText);
    })
}

function RendOneNoti(target, title, date, content, link) {
    return ` <li data-content="${target}">
                <a class="dropdown-item d-flex align-items-start flex-column px-2" href="${link}">
                    <i class="bi bi-envelope me-2 text-primary"></i>
                    <div>
                        <div style="font-size:15px!important" class="fw-bold text-wrap">${title}</div>
                        <small>${getTimeAgo(date)}</small>
                    </div>
                    <p class="text-wrap content mb-0" style="font-size:13px!important">${content}</p>
                </a>
            </li>`;
}

function getTimeAgo(dateTime) {
    const now = new Date();
    const timeSpan = now - new Date(dateTime); // milliseconds
    const seconds = timeSpan / 1000;
    const minutes = seconds / 60;
    const hours = minutes / 60;
    const days = hours / 24;

    if (seconds < 60) {
        return "Vài giây trước";
    } else if (minutes < 60) {
        return `${Math.floor(minutes)} phút trước`;
    } else if (hours < 24) {
        return `${Math.floor(hours)} giờ trước`;
    } else if (days < 30) {
        return `${Math.floor(days)} ngày trước`;
    } else if (days < 365) {
        return `${Math.floor(days / 30)} tháng trước`;
    } else {
        return `${Math.floor(days / 365)} năm trước`;
    }
}
