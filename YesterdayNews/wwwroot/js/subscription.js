var dataTable;

dataTable = $('#myDataTable').DataTable({
    "language": {
        "searchPlaceholder": "Search Subscriptions...",
    },
    "responsive": true,
    "processing": true,
    "serverSide": false,
    "ajax": {
        url: '/Subscription/GetAll',
        type: 'GET',
        dataSrc: 'data'
    },
    "columns": [
        { "title": "User Email", "data": "userEmail", "width": "15%" },
        { "title": "Created", "data": "created", "width": "15%" },
        { "title": "Expires", "data": "expires", "width": "10%" },
        { "title": "Payment Completed", "data": "paymentComplete", "width": "10%" },
        { "title": "Deleted", "data": "isDeleted", "width": "10%" },
        { "title": "Subscription Type", "data": "typeName", "width": "10%" },
        {
            "data": 'id',
            "width": "20%",
            "render": function (data, type, row) {
                const isDeleted = row.isDeleted;

                let buttons = `
        <div class="w-100 btn-group gap-2" role="group">
            <a href="/SubscriptionType/Edit/${data}" class="btn btn-primary mx-1">
                <i class="bi bi-pencil-square px-2"></i>Edit
            </a>`;

                if (!isDeleted) {
                    buttons += `
            <a onClick="Delete('/Subscription/Delete/${data}')" class="btn btn-danger mx-1">
                <i class="bi bi-trash px-2"></i>Delete
            </a>`;
                }

                buttons += `</div>`;
                return buttons;
            }
        }
    ]
});
function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "If Payment is confirmed, refund will be issued for the customer!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, cancel subscription!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

$(document).ready(function () {
    loadDataTable();
});