// Chart.js Helper Functions
window.chartHelper = {
    // Create a pie chart
    createPieChart: function (canvasId, labels, data, backgroundColors) {
        const ctx = document.getElementById(canvasId).getContext('2d');

        // Destroy previous chart if exists
        if (window.chartHelper.charts && window.chartHelper.charts[canvasId]) {
            window.chartHelper.charts[canvasId].destroy();
        }

        const chart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColors,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'right',
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.raw.toFixed(2);
                            }
                        }
                    }
                }
            }
        });

        // Store chart reference
        if (!window.chartHelper.charts) {
            window.chartHelper.charts = {};
        }
        window.chartHelper.charts[canvasId] = chart;

        return chart;
    },

    // Destroy all charts
    destroyAllCharts: function() {
        if (window.chartHelper.charts) {
            Object.values(window.chartHelper.charts).forEach(chart => {
                chart.destroy();
            });
            window.chartHelper.charts = {};
        }
    }
};