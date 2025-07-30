var dataTable;

function loadDataTable() {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            url: '/SubscriptionType/getall',
            dataSrc: 'data'
        },
        "columns": [
            { "title": "Type Name", data: 'typeName' },
            { "title": "Description", data: 'description' },
            { "title": "Price", data: 'price' },
            {
                "title": "Subscriptions count", data: 'subscriptions', "render": function (data) {
                    return `${data.length}`;
                },
            {
                "title": "Actions",
                data: 'id',
                render: function (data) {
                    return `
                        <div class="w-100 btn-group gap-2" role="group">
                            <a href="/SubscriptionType/id=${data}" class="btn btn-primary mx-1">
                                <i class="bi bi-pencil-square px-2"></i>Edit
                            </a>
                            <a onClick=Delete("/SubscriptionType/id=${data}") class="btn btn-danger mx-1">
                                <i class="bi bi-trash px-2"></i>Delete
                            </a>
                        </div>
                    `;
                }
            }
        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message)
                }
            })
        }
    });
}


$(document).ready(function () {
    loadDataTable();
});