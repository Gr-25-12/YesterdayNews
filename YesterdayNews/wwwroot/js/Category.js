var dataTable;

function loadDataTable() {
    dataTable = $('#myDataTable').DataTable({
        "responsive": true,
        "processing": true,
        "serverSide": false,
        "ajax": {
            url: '/Category/GetAll', 
            type: 'GET',
            dataType: 'json',
            dataSrc: 'data'
        },
        "columns": [
            {
                "data": "name",
                "title": "Category Name",
                "width": "50%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="btn-group w-75" role="group">
                            <a href="/Category/Edit/${data}" class="btn btn-primary btn-sm mx-1">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onclick="Delete('/Category/Delete/${data}')" class="btn btn-danger btn-sm mx-1">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>
                    `;
                },
                "width": "50%",
                "orderable": false,
                "className": "text-center"
            }
        ]
        
    });
}

$(document).ready(function () {
    loadDataTable();
});
