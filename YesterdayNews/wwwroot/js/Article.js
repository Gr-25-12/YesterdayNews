var dataTable;

function loadDataTable(status) {
    dataTable = $('#myDataTable').DataTable({
        "language": {
            "searchPlaceholder": "Search articles...", 
        },
        "responsive": true,
        "processing": true,
        "serverSide": false,
        "ajax": {
            url: '/Article/GetAll?status='+status,
            type: 'GET',
            dataType: 'json',
            dataSrc: 'data' 
        },
        "columns": [
            {
                "data": 'headline', "width": "50%", "render": function (data, type, row) {
                    // Strip HTML tags for display
                    const strippedData = $('<div>').html(data).text();

                    return `
                            <div class="headline-cell">
                                <span class="short-text">${strippedData.length > 20 ? strippedData.slice(0, 20) + '...' : strippedData}</span>
                                ${strippedData.length > 50 ?
                                                `<span class="full-text d-none">${strippedData}</span>
                                     <a href="#" class="toggle-headline ms-2 text-info underline">Show More</a>`
                                                : ''
                                            }
                            </div>
                        `;
                }
            },
            {
                "data": "author",
                "width": "15%",
                "render": function (data) {
                    return `${data.firstName} ${data.lastName}`;
                },
                
            },
            {
                "data": "dateStamp",
                "width": "10%",
                "render": function (data) {
                    return new Date(data).toLocaleDateString();
                },
               
            },
            {
                "data": "category.name",
                "width": "10%"
            },
            {
                "data": "articleStatus",
                "width": "10%"
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
                "render": function (data, type, row) {
                    return `
            <div class="btn-group" role="group">
                <a href="/Article/Details/${data}" class="btn btn-secondary btn-sm mx-1">
                    <i class="bi bi-list px-2"></i> Details
                </a>
                
                <a onclick="handleEditClick(${data}, '${row.articleStatus}')" class="btn btn-primary btn-sm mx-1">
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
    });
}
function setupImagePreview() {
    const input = document.getElementById('imageInput');
    const preview = document.getElementById('imagePreview');

    if (input) {
        input.addEventListener('change', () => {
            const file = input.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = e => {
                    preview.innerHTML = `<img src="${e.target.result}" style="max-width:100%; height:auto;" />`;
                };
                reader.readAsDataURL(file);
            }
        });
    }
}
function setupImagePreview() {
    const input = document.getElementById('imageInput');
    const preview = document.getElementById('imagePreview');

    if (input) {
        input.addEventListener('change', () => {
            const file = input.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = e => {
                    preview.innerHTML = `<img src="${e.target.result}" style="max-width:100%; height:auto;" />`;
                };
                reader.readAsDataURL(file);
            }
        });
    }
}

function handleEditClick(articleId, articleStatus) {
    
    if (articleStatus === "Archived") {
        Swal.fire({
            icon: 'error',
            title: 'Ooops...',
            text: 'This article has been archived and cannot be edited.',
            confirmButtonColor: '#3A2512'
        });
        return false; 
    }

   
    window.location.href = `/Article/Edit/${articleId}`;
}

$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("draft")) {

        loadDataTable("draft");
    } else if (url.includes("pendingReview")) {
        loadDataTable("pendingReview");

    } else if (url.includes("rejected")) {
        loadDataTable("rejected");

    } else if (url.includes("published")) {
        loadDataTable("published");

    } else if (url.includes("archived")) {
        loadDataTable("archived");

    } else {
        loadDataTable("all");
    }

    setupImagePreview();
});

$(document).on('click', '.toggle-headline', function (e) {
    e.preventDefault();
    const $cell = $(this).closest('.headline-cell');
    $cell.find('.short-text, .full-text').toggleClass('d-none');
    $(this).text(function (i, text) {
        return text === 'Show More' ? 'Show Less' : 'Show More';
    });
});