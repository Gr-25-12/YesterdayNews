// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Show/hide loader functions
function showGlobalLoader() {
    document.getElementById('global-loader').style.display = 'flex';
}

function hideGlobalLoader() {
    document.getElementById('global-loader').style.display = 'none';
}


(function () {
    
    const originalOpen = XMLHttpRequest.prototype.open;
    const originalSend = XMLHttpRequest.prototype.send;

    
    XMLHttpRequest.prototype.open = function () {
        this.addEventListener('loadstart', showGlobalLoader);
        this.addEventListener('loadend', hideGlobalLoader);
        this.addEventListener('error', hideGlobalLoader);
        this.addEventListener('abort', hideGlobalLoader);
        originalOpen.apply(this, arguments);
    };

    
    XMLHttpRequest.prototype.send = function () {
        originalSend.apply(this, arguments);
    };

    //for api calls 
    const originalFetch = window.fetch;
    window.fetch = function () {
        showGlobalLoader();
        return originalFetch.apply(this, arguments)
            .then(response => {
                hideGlobalLoader();
                return response;
            })
            .catch(error => {
                hideGlobalLoader();
                throw error;
            });
    };
})();

    // page transitions
document.addEventListener('DOMContentLoaded', function () {
    if (typeof Turbolinks !== 'undefined') {
        document.addEventListener('turbolinks:click', showGlobalLoader);
        document.addEventListener('turbolinks:render', hideGlobalLoader);
    }
   
    // For forms submissions
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (form.method.toLowerCase() === 'get') return;
        showGlobalLoader();
    });

    showHoursAgo();
});


// Toggle password visibility
document.querySelectorAll('.toggle-password').forEach(button => {
    button.addEventListener('click', function () {
        const input = this.parentNode.querySelector('input');
        const icon = this.querySelector('i');
        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.remove('bi-eye');
            icon.classList.add('bi-eye-slash');
        } else {
            input.type = 'password';
            icon.classList.remove('bi-eye-slash');
            icon.classList.add('bi-eye');
        }
    });
});


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
function showHoursAgo(selector = '.time-ago') {
    document.querySelectorAll(selector).forEach(el => {
        const timestamp = new Date(el.dataset.timestamp);
        const now = new Date();
        const diffMs = now - timestamp;
        const diffHours = Math.floor(diffMs / 1000 / 60 / 60);
        const span = el.querySelector('.hours-ago');
        if (span) {
            span.textContent = `${diffHours} h ago`;
        }
    });
}