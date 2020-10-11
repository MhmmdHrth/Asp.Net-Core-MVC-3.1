var dataTable

$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("inprocess")) {
        return loadDataTable("GetOrderList?status=inprocess")
    }
    else if (url.includes("paymentPending")) {
        return loadDataTable("GetOrderList?status=paymentPending")
    }
    else if (url.includes("completed")) {
        return loadDataTable("GetOrderList?status=completed")
    }
    else if (url.includes("rejected")) {
        return loadDataTable("GetOrderList?status=rejected")
    }
    else (url.includes("all"))
        return loadDataTable("GetOrderList?status=all")
})

function loadDataTable(dataUrl) {
    dataTable = $("#tblData").DataTable({
        "ajax": {
            "url": `/Admin/Order/${dataUrl}`
        },
        "columns": [
            { "data": "id", "width": "15%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Order/Details/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i>
                                </a>
                            </div>`;
                }
            }
        ]
    })
}