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
                                ${index === 1 ? '<div class="popular-badge position-absolute top-0 end-0 bg-warning text-dark px-3 py-1 rounded-start-3 fw-bold small"><i class="bi bi-star-fill me-1"></i>POPULAR</div>' : ''}
                                
                                <!-- Card Header with Gradient -->
                                <div class="card-header text-center py-4 border-0" style="background: linear-gradient(135deg, #3A2512 0%, #b39086 100%);">
                                    <h4 class="text-white mb-0 fw-bold text-uppercase letter-spacing-1 fs-5">${plan.typeName}</h4>
                                </div>
                                
                                
                                <div class="card-body p-4 d-flex flex-column bg-light">
                                  
                                    <div class="text-center mb-4">
                                        <div class="price-container">
                                            <span class="display-4 fw-bold text-primary">${plan.price}</span>
                                            ${plan.period ? `<span class="text-muted fs-6 ms-2">/${plan.period}</span>` : ''}
                                        </div>
                                        <p class="text-muted mt-3 fs-6 px-2">${plan.description}</p>
                                    </div>
                                    
                                  
                                    <div class="features-section flex-grow-1">
                                        ${plan.features && plan.features.length > 0 ? `
                                            <ul class="list-unstyled">
                                                ${plan.features.map(feature => `
                                                    <li class="mb-3 d-flex align-items-start">
                                                        <div class="feature-icon-wrapper me-3 mt-1">
                                                            <i class="bi bi-check-circle-fill text-success fs-6"></i>
                                                        </div>
                                                        <span class="fs-6 text-dark">${feature}</span>
                                                    </li>
                                                `).join('')}
                                            </ul>
                                        ` : '<p class="text-muted text-center">No features listed</p>'}
                                    </div>
                                    
                                   
                                    <div class="mt-4">
                                        <a role="button" href="Subscription/Paynow" class="btn btn-lg w-100 rounded-pill fw-bold py-3 choose-plan-btn transition-all" 
                                                style="background: linear-gradient(135deg, #b39086 0%, #3A2512 100%); border: none; color: white;"
                                                onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 8px 25px rgba(58, 37, 18, 0.3)';"
                                                onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 4px 15px rgba(0,0,0,0.1)';"
                                                data-plan-id="${plan.id || ''}" data-plan-name="${plan.typeName}">
                                            Choose ${plan.typeName}
                                            <i class="bi bi-arrow-right ms-2"></i>
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `).join('')}
                </div>
            `;

        } catch (err) {
            console.log("Failed to load subscription plans:", err);
          
        }
    });

    modal.addEventListener('hidden.bs.modal', function () {
        document.activeElement.blur();
    });

});