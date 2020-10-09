var dataTable

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $("#tblData").DataTable({
        "ajax": {
            "url": "/Admin/User/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "company.name", "width": "15%" },
            { "data": "role", "width": "15%" },
            {
                "data": {
                    id: "id",
                    lockoutEnd: "lockoutEnd"
                },
                "render": function (data) {
                    var dateToday = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > dateToday) {
                        //user is locked
                        return `<div class="text-center">
                                <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-lock-open"></i> Unblock
                                </a>
                            </div>`;
                    }
                    else {
                        //user is unlock
                        return `<div class="text-center">
                                <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-lock"></i> Block
                                </a>
                            </div>`;
                    }
                }
            }
        ]
    })
}

function LockUnlock(id) {
    swal({
        title: "Are you sure want to block/unblock?",
        text: "You can block/unblock them back!",
        icon: "warning",
        buttons: true
    }).then(blockUnblock => {
        if (blockUnblock) {
            $.ajax({
                type: "POST",
                url: "/Admin/User/LockUnlock",
                data: JSON.stringify(id),
                contentType: "application/json",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}