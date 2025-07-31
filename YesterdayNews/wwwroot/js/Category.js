var dataTable;

function loadDataTable() {
    dataTable = $('#categoriesTable').DataTable({
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
                "title": "Category Name"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/Category/Edit/${data}" class="btn btn-primary btn-sm mx-1">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onclick="Delete('/Category/Delete/${data}')" class="btn btn-danger btn-sm mx-1">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>
                    `;
                },
                "width": "20%",
                "orderable": false,
                "className": "text-center"
            }
        ],
        "error": function (xhr, error, thrown) {
            console.error("DataTables error:", xhr.responseText);
            toastr.error("Failed to load categories. Please try again.");
        }
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
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
