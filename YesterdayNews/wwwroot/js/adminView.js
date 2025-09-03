document.addEventListener('DOMContentLoaded', function () {
    // Check if Chart.js is loaded
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded');
        return;
    }

    // Subscriptions Trend Chart
    const subscriptionsTrendCtx = document.getElementById('subscriptionsTrendChart');
    if (subscriptionsTrendCtx) {
        const subscriptionsTrendData = @Html.Raw(Json.Serialize(Model.SubscriptionsByDay));

        new Chart(subscriptionsTrendCtx, {
            type: 'line',
            data: {
                labels: subscriptionsTrendData.map(d => d.label),
                datasets: [{
                    label: 'Subscriptions',
                    data: subscriptionsTrendData.map(d => d.value),
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0, 123, 255, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                }
            }
        });
    }

    // Revenue Trend Chart
    const revenueTrendCtx = document.getElementById('revenueTrendChart');
    if (revenueTrendCtx) {
        const revenueTrendData = @Html.Raw(Json.Serialize(Model.RevenueByDay));

        new Chart(revenueTrendCtx, {
            type: 'bar',
            data: {
                labels: revenueTrendData.map(d => d.label),
                datasets: [{
                    label: 'Revenue ($)',
                    data: revenueTrendData.map(d => d.value),
                    backgroundColor: '#28a745',
                    borderColor: '#28a745',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                }
            }
        });
    }

    // Articles by Status Pie Chart
    const articlesStatusCtx = document.getElementById('articlesStatusChart');
    if (articlesStatusCtx) {
        const articlesStatusData = @Html.Raw(Json.Serialize(Model.ArticlesByStatus));

        new Chart(articlesStatusCtx, {
            type: 'doughnut',
            data: {
                labels: articlesStatusData.map(d => d.label),
                datasets: [{
                    data: articlesStatusData.map(d => d.value),
                    backgroundColor: articlesStatusData.map(d => d.color),
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    // Users by Role Pie Chart
    const usersRoleCtx = document.getElementById('usersRoleChart');
    if (usersRoleCtx) {
        const usersRoleData = @Html.Raw(Json.Serialize(Model.UsersByRole));

        new Chart(usersRoleCtx, {
            type: 'doughnut',
            data: {
                labels: usersRoleData.map(d => d.label),
                datasets: [{
                    data: usersRoleData.map(d => d.value),
                    backgroundColor: usersRoleData.map(d => d.color),
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    // Subscriptions by Type Pie Chart
    const subscriptionTypeCtx = document.getElementById('subscriptionTypeChart');
    if (subscriptionTypeCtx) {
        const subscriptionTypeData = @Html.Raw(Json.Serialize(Model.SubscriptionsByType));

        new Chart(subscriptionTypeCtx, {
            type: 'doughnut',
            data: {
                labels: subscriptionTypeData.map(d => d.label),
                datasets: [{
                    data: subscriptionTypeData.map(d => d.value),
                    backgroundColor: subscriptionTypeData.map(d => d.color),
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }
});