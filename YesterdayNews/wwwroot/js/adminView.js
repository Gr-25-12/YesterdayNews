

document.addEventListener('DOMContentLoaded', function () {
    
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded');
        return;
    }
    const data = window.dashboardData;

    const themeColors = [
        getComputedStyle(document.documentElement).getPropertyValue('--bs-primary').trim(),      
        getComputedStyle(document.documentElement).getPropertyValue('--bs-secondary').trim(),    
        getComputedStyle(document.documentElement).getPropertyValue('--bs-success').trim(),      
        getComputedStyle(document.documentElement).getPropertyValue('--bs-danger').trim(),       
        getComputedStyle(document.documentElement).getPropertyValue('--bs-warning').trim(),      
        getComputedStyle(document.documentElement).getPropertyValue('--bs-info').trim()          
    ];

    // Subscriptions Trend Chart
    const subscriptionsTrendCtx = document.getElementById('subscriptionsTrendChart');
    if (subscriptionsTrendCtx) {
        const subscriptionsTrendData = data.subscriptionsByDay;

        new Chart(subscriptionsTrendCtx, {
            type: 'line',
            data: {
                labels: subscriptionsTrendData.map(d => d.label),
                datasets: [{
                    label: 'Subscriptions',
                    data: subscriptionsTrendData.map(d => d.value),
                    borderColor: '#3A2512',
                    backgroundColor: '#b39080',
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
        const revenueTrendData = data.revenueByDay;

        new Chart(revenueTrendCtx, {
            type: 'bar',
            data: {
                labels: revenueTrendData.map(d => d.label),
                datasets: [{
                    label: 'Revenue (Sek)',
                    data: revenueTrendData.map(d => d.value),
                    backgroundColor: '#3A2512',
                    borderColor: '#b39086',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return 'SEK' + value.toLocaleString();
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
        const articlesStatusData = data.articlesByStatus;

        new Chart(articlesStatusCtx, {
            type: 'pie',
            data: {
                labels: articlesStatusData.map(d => d.label),
                datasets: [{
                    data: articlesStatusData.map(d => d.value),
                    backgroundColor: themeColors.slice(0, articlesStatusData.length),
                    borderWidth: 2,
                    borderColor: '#fff',
                    
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
        const usersRoleData = data.usersByRole;

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
        const subscriptionTypeData = data.subscriptionsByType;

        new Chart(subscriptionTypeCtx, {
            type: 'pie',
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