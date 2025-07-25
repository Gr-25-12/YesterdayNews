var dataTable;

function loadDataTable() {
    dataTable = $('#articlesTable').DataTable({
        "language": {
            "searchPlaceholder": "Search articles...", 
        },
        "responsive": true,
        "processing": true,
        "serverSide": false,
        "ajax": {
            url: '/Article/GetAll',
            type: 'GET',
            dataType: 'json',
            dataSrc: 'data' 
        },
        "columns": [
            {
                "data": "headline",
                
            },
            {
                "data": "author",
                "render": function (data) {
                    return `${data.firstName} ${data.lastName}`;
                },
                
            },
            {
                "data": "dateStamp",
                "render": function (data) {
                    return new Date(data).toLocaleDateString();
                },
               
            },
            {
                "data": "category.name",
               
            },
            {
                "data": "articleStatus",
                
            },
            {
                "data": "views",
                "width": "5%",
                "className": "text-center"
            },
            {
                "data": "likes",
                "width": "5%",
                "className": "text-center"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/Article/Edit/${data}" class="btn btn-primary btn-sm mx-1">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onclick="Delete('/Article/Delete/${data}')" class="btn btn-danger btn-sm mx-1">
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
            toastr.error("Failed to load articles. Please try again.");
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