// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

//$('[data-bs-toggle="modal"]').on('click', function (e) {
//    // Prevent the default action that opens the modal automatically
//    e.preventDefault();
//    $(this).trigger("click");
//    const targetModalSelector = $(this).attr('data-bs-target');
//    const modalInstance = new bootstrap.Modal(targetModal);
//    modalInstance.show();
//})

function sanitizeInput(value) {
    // Loại bỏ các ký tự có thể gây nguy hiểm
    return value
        .replace(/[<>\/\\'"&=;(){}[\]`$]/g, '')      
        .replace(/(script|onerror|onload|javascript|alert)/gi, '');
}

$('input[type="text"], textarea').on('input', function () {
    let clean = sanitizeInput($(this).val());
    $(this).val(clean);
});

$(".modal .modal-body").on("scroll", function () {
    if ($(this).scrollTop() > 0) {
        $(this).closest(".modal-content").find(".modal-header").addClass("shadow-sm");
    }
    else {
        $(this).closest(".modal-content").find(".modal-header").removeClass("shadow-sm");
    }
})

$(`input[name='phoneNumber'], input.number`).on("input", function () {
    let value = $(this).val();
    let onlyNumbers = value.replace(/\D/g, '');
    $(this).val(onlyNumbers);
})

$('input.only_string').on('input', function () {
    let value = $(this).val();
    let onlyLetters = value.replace(/[^A-Za-zÀ-Ỷà-ỷĂăÂâĐđÊêÔôƠơƯưÁáÉéÍíÓóÚúÝýàèìòùỳĂăẰằẮắẴẵẶặÂâẦầẤấẪẫẬậÊêỀềẾếỄễỆệÔôỒồỐốỖỗỘộƠơỜờỚớỠỡỢợƯưỪừỨứỮữỰự\s]/g, '');
    $(this).val(onlyLetters);
});

document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

$(".login_container form input[name='Password']").on("keypress", function (e) {
    if (e.key === "Enter") {
        $(".login_container form")[0].submit();
    }
})

$("#changePassword_modal .save_btn").on("click", function () {
    const formElement$ = $(this).closest(".modal-content").find("form");

    const currPass = formElement$.find("input[name='currentPass']");
    const newPass = formElement$.find("input[name='newPass']");
    const confirmPass = formElement$.find("input[name='confirmPass']");

    if (currPass.val() == "") {
        currPass.focus();
        return;
    }
    if (newPass.val() == "") {
        newPass.focus();
        return;
    }
    if (confirmPass.val() == "") {
        confirmPass.focus();
        return;
    }

    $("#page-loader").css("display", "flex");
    $.ajax({
        url: "/oracle",
        type: "POST",
        data: {
            currentPass: currPass.val(),
            newPass: newPass.val(),
            confirmPass: confirmPass.val()
        }
    }).then((response) => {
        $("#page-loader").css("display", "none");

        if (response.code === 0) {
            $(".modal#success_modal").find("#message").text(response.message);
            $(".modal#success_modal").modal("show");
        }
        else {
            $(".modal#error_modal").find("#message").text(response.message);
            $(".modal#error_modal").modal("show");
        }
    });
})

const listModalsOpenAfterNotice = ["changePassword_modal", "account_index_modal", "topic_index_modal", "submit_proposal_model", "committee_assignment_modal"];
$("#error_modal").on("hidden.bs.modal", () => {
    listModalsOpenAfterNotice.forEach((id) => {
        if (id == previusModel) {
            $(`#${id}`).modal("show");
            return;
        }
    })
})
$(".modal:not(.no_reset)").on("hidden.bs.modal", function () {
    $(this).find("form").each(function () {

        this.reset();
        const form = $(this);
        if (form.data('validator')) {
            form.removeData('validator');
        }
        form.validate().resetForm();
        form.find(".error").removeClass("error");
    })
})

let previusModel = "";
$(".modal").on("hide.bs.modal", function () {
    const id = $(this).attr("id");
    if (id == "error_modal") {
        return;
    }
    previusModel = id;
})

$('.modal').on('show.bs.modal', function () {
    $(".modal").modal("hide");
    $(".modal-backdrop").remove();
})

$(".pagination .page-item:not(.active).item").on("click", function () {

    const newPageNumber = $(this).find(".page-link").text()
    const form = $(this).closest("form");
    form.find("input[name='PageNumber']").val(newPageNumber);
    form.submit();
})
$(".pagination .page-left:not(.disabled)").on("click", function () {

    const currentPageItem = $(this).closest(".pagination").find(".page-item.active");
    const currentPageNumber = parseInt($(this).closest(".pagination").find(".page-item.active").find(".page-link").text());
    if (currentPageNumber == 1) return;

    const form = $(this).closest("form");
    form.find("input[name='PageNumber']").val(currentPageNumber - 1);
    form.submit();
})
$(".pagination .page-right:not(.disabled)").on("click", function () {

    const pagination = $(this).closest(".pagination");
    const currentPageItem = pagination.find(".page-item.active");
    const currentPageNumber = parseInt(currentPageItem.text());
    const lastPageItem = pagination.find(".page-item.item").last();
    if (currentPageItem[0] == lastPageItem[0]) return;

    const form = $(this).closest("form");
    form.find("input[name='PageNumber']").val(currentPageNumber + 1);
    form.submit();
})

$(".pagination .page-first:not(.disabled)").on("click", function () {

    const form = $(this).closest("form");
    form.find("input[name='PageNumber']").val(1);
    form.submit();
})

$(".pagination .page-last:not(.disabled)").on("click", function () {

    const form = $(this).closest("form");
    form.find("input[name='PageNumber']").val(parseInt($(this).data("number")));
    form.submit();
})

$("select.page_len_select").on("change", function () {
    if ($(this).val() === "-1") {
        return;
    }
    $(this).closest("form").submit();
})

$('.password_input_container .icon_eye').click(function () {
    const icon = $(this);
    const input = icon.closest('.password_input_container').find('input');
    if (icon.hasClass('open')) {
        icon.removeClass('open');
        input.attr('type', 'password');
        icon.attr('src', icon.attr("src").replace('eye', 'eye_close'))
    }
    else {
        icon.addClass('open');
        input.attr('type', 'text');
        icon.attr('src', icon.attr("src").replace('_close', ''))
    }
})

$(window).resize(() => {
    const height = window.innerHeight - 60;
    $("main").height(height);
})
$(window).trigger("resize");

window.addEventListener("load", function () {
    const loader = document.getElementById("page-loader");
    if (loader) {
        loader.style.display = "none";
    }
});

$(document).ready(function () {
    $.validator.addMethod("vietNamPhone", function (value, element) {
        return this.optional(element) || /^0\d{9}$/.test(value);
    }, "Hãy nhập đúng định dạng của số điện thoại");

    $.validator.addMethod("fullName", function (value, element) {
        return this.optional(element) || /^[a-zA-ZÀ-ỹ\s]+$/.test(value.trim());
    }, "Hãy nhập đúng định dạng họ tên");

    $.validator.addMethod("notEqual", function (value, element, arg) {
        return value !== arg;
    }, "not equal error default message");
})

function ShowConfirmModal(message) {
    return new Promise((resolve) => {
        const Modal = $("#confirm_modal");
        const Message = Modal.find("#message");

        Message.text(message);
        Modal.modal("show");

        Modal.find("#ConfirmBtn").off("click").click(() => {
            Modal.modal("hide");
            resolve(true);
        })

        Modal.off("hidden.bs.modal").on("hidden.bs.modal", function () {
            resolve(false);
        })
    })
}