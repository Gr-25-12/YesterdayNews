var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/user/getall' },
        dataSrc: 'data',
        "columns": [
            {"title": "Name", "data": "fullName", "width": "15%"},
            { "title": "Email", "data": "email", "width": "15%" },
            { "title": "Role", "data": "role", "width": "10%" },
            {
                data: { id: 'id', lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        //means the customer is locked
                        return `
                        <div class="text-center">
                        
                     <a Onclick=lockUnlock('${data.id}') class="btn btn-success text-white mx-2"> 
                     <i class="bi bi-ublock-fill"></i> unlock user
                     </a>      
                     <a href="/User/RoleMangement?userId=${data.id}" class="btn btn-warning text-white mx-2"> 
                     <i class="bi bi-pencil-square"></i> Permisson
                     </a> 
                           </div>`
                    } else {
                        return `   <div class="text-center">

                            <a Onclick=lockUnlock('${data.id}') class="btn btn-danger text-white mx-2">
                                <i class="bi bi-ublock-fill"></i> lock user
                            </a>
                            <a href="/User/RoleMangement?userId=${data.id}" class="btn btn-warning text-white mx-2">
                                <i class="bi bi-pencil-square"></i> Permissons
                            </a>
                        </div>`
                    }


                },
                "width": "25%"
            }
        ]
    });
}

function lockUnlock(id) {
    $.ajax({
        type: "POST",
        url: "/User/LockUnlock",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message)
                dataTable.ajax.reload();
            }
        }
    })
}