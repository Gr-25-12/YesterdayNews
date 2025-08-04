var dataTable;

function loadDataTable() {
    dataTable = $('#myDataTable').DataTable({
        "language": {
            "searchPlaceholder": "Search Subscriptions types...",
        },
        "responsive": true,
        "processing": true,
        "serverSide": false,
        "ajax": {
            url: '/SubscriptionType/GetAll',
            type: 'GET',
            dataSrc: 'data'
        },
        "columns": [
            { "data": 'typeName', "width": "20%" },
            { "data": 'description', "width": "30%" },
            {
                "data": 'price',
                "width": "15%",
                "render": function (data) {
                    return `${data.toFixed(2)} Kr`;
                }
            },
            {
                "data": 'subscriptions',
                "width": "15%",
                "render": function (data) {
                    return data ? data.length : 0;
                }
            },
            {
                "data": 'id',
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="w-100 btn-group gap-2" role="group">
                            <a href="/SubscriptionType/Edit/${data}" class="btn btn-primary mx-1">
                                <i class="bi bi-pencil-square px-2"></i>Edit
                            </a>
                            <a onClick="Delete('/SubscriptionType/Delete/${data}')" class="btn btn-danger mx-1">
                                <i class="bi bi-trash px-2"></i>Delete
                            </a>
                        </div>
                    `;
                }
            }
        ],
        
    });
}


$(document).ready(function () {
    loadDataTable();
});