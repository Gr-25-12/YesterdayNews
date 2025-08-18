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
            {
                "data": 'description', "width": "30%", "render": function (data, type, row) {
                    // Strip HTML tags for display
                    const strippedData = $('<div>').html(data).text();

                    return `
        <div class="description-cell">
            <span class="short-text">${strippedData.length > 50 ? strippedData.slice(0, 50) + '...' : strippedData}</span>
            ${strippedData.length > 50 ?
                            `<span class="full-text d-none">${strippedData}</span>
                 <a href="#" class="toggle-description ms-2 text-info underline">Show More</a>`
                            : ''
                        }
        </div>
    `;
                }
             },
            {
                "data": 'price',
                "width": "15%",
                "render": function (data) {
                    return `${data.toFixed(2)} Kr`;
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

$(document).on('click', '.toggle-description', function (e) {
    e.preventDefault();
    const $cell = $(this).closest('.description-cell');
    $cell.find('.short-text, .full-text').toggleClass('d-none');
    $(this).text(function (i, text) {
        return text === 'Show More' ? 'Show Less' : 'Show More';
    });
});