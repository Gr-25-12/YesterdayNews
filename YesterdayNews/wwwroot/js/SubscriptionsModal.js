
document.addEventListener("DOMContentLoaded", function () {
    const modal = document.getElementById('subscribeModal');

    modal.addEventListener('show.bs.modal', async function () {
        try {
            const res = await fetch('/SubscriptionType/GetAll');

            const result = await res.json();
            const plans = result.data;
            const container = document.getElementById("subscriptionPlans");

            container.innerHTML = `
                <div class="row g-4 justify-content-center">
                    ${plans.map((plan, index) => `
                        <div class="col-lg-4 col-md-6 col-sm-12">
                            <div class="card h-100 border-0 shadow-lg position-relative overflow-hidden rounded-4 subscription-card">                                
                                <div class="card-header text-center py-4 border-0" style="background: linear-gradient(135deg, #3A2512 0%, #b39086 100%);">
                                    <h4 class="text-white mb-0 fw-bold text-uppercase letter-spacing-1 fs-5">${plan.typeName}</h4>
                                </div>
                                
                                <div class="card-body p-4 d-flex flex-column bg-secondary-subtle h-100">
                                    <div class="flex-grow-1 pt-3">
                                        <div class="text-center mb-4">
                                            <div class="price-container">
                                                <span class="display-6 fw-bold text-primary">${plan.price} <span class="fs-5">kr</span></span>
                                            </div>
                                            <p class="text-muted mt-3 fs-6 px-2">${plan.description}</p>
                                        </div>
                                    </div>
                                    
                                    <form method="post" action="/Subscription/SubscribeNow" class="subscription-form w-100">
                                        <input type="hidden" name="planId" value="${plan.id}" />
                                        
                                        <div class="mt-auto">
                                            <button type="submit" class="btn btn-lg w-100 rounded-pill fw-bold py-3 choose-plan-btn"
                                                    style="background: linear-gradient(135deg, #b39086 0%, #3A2512 100%); border: none; color: white;"
                                                    data-plan-id="${plan.id}" data-plan-name="${plan.typeName}">
                                                <span class="button-text">Choose ${plan.typeName}</span>
                                                <i class="bi bi-arrow-right ms-2"></i>
                                                <span class="spinner-border spinner-border-sm d-none loading-spinner" role="status"></span>
                                            </button>
                                        </div>
                                    </form>
                                </div>
                            </div>
                        </div>
                    `).join('')}
                </div>
            `;

            // Handle form submissions
            container.querySelectorAll('.subscription-form').forEach(form => {
                form.addEventListener('submit', async function (e) {
                    e.preventDefault();

                    // handle the not logged-in user case
                    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                    if (!tokenInput) {
                        
                        window.location.href = '/Identity/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                        return;
                    }

                    const button = this.querySelector('button[type="submit"]');
                    const buttonText = button.querySelector('.button-text');
                    const spinner = button.querySelector('.loading-spinner');

                    button.disabled = true;
                    buttonText.textContent = 'Processing...';
                    spinner.classList.remove('d-none');

                    try {
                        const formData = new FormData(this);

                        const response = await fetch(this.action, {
                            method: 'POST',
                            body: formData,
                            headers: {
                                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                            }
                        });
                       
                        if (response.redirected) {
                            window.location.href = response.url;
                        } else {
                            const result = await response.json();
                            if (result.success) {
                                window.location.href = result.redirectUrl;
                            } else {
                                toastr.error(result.message || 'Subscription failed');
                            }
                        }
                    } catch (error) {
                        console.error('Error:', error);
                        toastr.error('An error occurred during subscription');
                    } finally {
                        button.disabled = false;
                        buttonText.textContent = `Choose ${button.dataset.planName}`;
                        spinner.classList.add('d-none');
                    }
                });
            });

        } catch (err) {
            console.error("Failed to load subscription plans:", err);
        }
    });

    modal.addEventListener('hidden.bs.modal', function () {
        document.activeElement.blur();
    });
});