var dataTable;
let createdInput;
let expiresText;

function loadDataTable(status) {
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
            { "title": "Created", "data": "created", "width": "15%", "render": function (data) { return new Date(data).toLocaleDateString() } },
            { "title": "Expires", "data": "expires", "width": "10%", "render": function (data) { return new Date(data).toLocaleDateString() } },
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
            <a href="/Subscription/Edit/${data}" class="btn btn-primary mx-1">
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
}
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

function userSelect() {
    if (!window.location.pathname.includes('/Subscription/Create')) return;

    const $userSelect = $('#UserId');
    if ($userSelect.length === 0) return;

    $userSelect.select2({
        placeholder: "Search for a user",
        minimumInputLength: 2,
        ajax: {
            url: '/Subscription/Search',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { searchTerm: params.term };
            },
            processResults: function (data) {
                return {
                    results: data.map(user => ({
                        id: user.id,
                        text: user.name
                    }))
                };
            },
            cache: true
        }
    });

    $userSelect.on('select2:select', function (e) {
        const userId = e.params.data.id;

        $.get('/Subscription/GetUserById', { id: userId }, function (user) {
            if (user) {
                $('#UserDisplay').show();
                $('#UserDisplay strong').eq(0).text(user.firstName);
                $('#UserDisplay strong').eq(1).text(user.lastName);
                $('#UserDisplay strong').eq(2).text(user.email);
            }
        });
    });
}

function updateExpiresText() {
    if (!window.location.pathname.includes('/Subscription/Create')) return;

    const date = new Date(createdInput.value);
    if (!isNaN(date)) {
        date.setFullYear(date.getFullYear() + 1);
        const expiresValue = date.toISOString().split('T')[0];
        expiresText.value = expiresValue;
        document.getElementById('Expires').value = expiresValue;
    } else {
        expiresText.value = "";
        document.getElementById('Expires').value = "";
    }
}
function setupPriceDisplay() {
    const subscriptionTypeSelect = document.getElementById("SubscriptionTypeId");
    const priceRow = document.getElementById("subscriptionPriceRow");
    const planSpan = document.getElementById("planText");
    const priceSpan = document.getElementById("priceText");
    const accessSpan = document.getElementById("accessLevelText");

    if (!subscriptionTypeSelect || !priceRow || !planSpan || !priceSpan || !accessSpan) return;

    subscriptionTypeSelect.addEventListener("change", function () {
        const selectedOption = subscriptionTypeSelect.options[subscriptionTypeSelect.selectedIndex];
        const plan = selectedOption.getAttribute("data-plan");
        const price = selectedOption.getAttribute("data-price");
        const access = selectedOption.getAttribute("data-accesslevel");

        if (plan && price && access) {
            planSpan.textContent = plan;
            priceSpan.textContent = price + " kr";
            accessSpan.textContent = access;

            priceRow.style.display = "block";
        } else {
            priceRow.style.display = "none";
        }
    });
}

$(document).ready(function () {
    loadDataTable();
    
    createdInput = document.getElementById("createdDate");
    expiresText = document.getElementById("expiresText");

    if (createdInput && expiresText) {
        createdInput.addEventListener("change", updateExpiresText);
        window.addEventListener("load", updateExpiresText);
        updateExpiresText()
    }
    userSelect();
    setupPriceDisplay();
});