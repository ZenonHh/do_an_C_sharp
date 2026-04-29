// ===== DASHBOARD CHARTS & REAL-TIME =====

let scansChart = null;
let deviceChart = null;
let notificationQueue = [];

// Initialize charts
function initializeCharts() {
    const scansCtx = document.getElementById('scansChart');
    const deviceCtx = document.getElementById('deviceChart');

    if (!scansCtx || !deviceCtx) {
        console.warn('Chart contexts not found');
        return;
    }

    // Scans per POI Chart
    scansChart = new Chart(scansCtx, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: 'Lượt Nghe',
                data: [],
                backgroundColor: 'rgba(79, 70, 229, 0.8)',
                borderColor: 'rgba(79, 70, 229, 1)',
                borderWidth: 2,
                borderRadius: 8,
                hoverBackgroundColor: 'rgba(63, 60, 199, 1)',
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    labels: {
                        font: { size: 14, weight: 'bold' },
                        color: '#2c3e50'
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: 'rgba(0,0,0,0.05)' },
                    ticks: { color: '#7f8c8d' }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: '#7f8c8d' }
                }
            }
        }
    });

    // Device activity doughnut chart
    deviceChart = new Chart(deviceCtx, {
        type: 'doughnut',
        data: {
            labels: ['Online', 'Offline', 'Idle'],
            datasets: [{
                data: [0, 0, 0],
                backgroundColor: [
                    'rgba(16, 185, 129, 0.8)',
                    'rgba(239, 68, 68, 0.8)',
                    'rgba(156, 163, 175, 0.8)'
                ],
                borderColor: [
                    'rgba(16, 185, 129, 1)',
                    'rgba(239, 68, 68, 1)',
                    'rgba(156, 163, 175, 1)'
                ],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        font: { size: 13 },
                        color: '#7f8c8d',
                        padding: 15
                    }
                }
            }
        }
    });
}

// Update scans chart
async function updateScansChart() {
    try {
        const response = await fetch('/api/pois/stats');
        const pois = await response.json();

        if (!scansChart || !Array.isArray(pois)) return;

        const labels = pois.slice(0, 10).map(p => p.name || 'Unknown');
        const data = pois.slice(0, 10).map(p => p.scanCount || 0);

        scansChart.data.labels = labels;
        scansChart.data.datasets[0].data = data;
        scansChart.update();
    } catch (error) {
        console.error('Error updating scans chart:', error);
    }
}

// Update device chart
async function updateDeviceChart() {
    try {
        const response = await fetch('/api/devices/stats');
        const stats = await response.json();

        if (!deviceChart) return;

        deviceChart.data.datasets[0].data = [
            stats.online || 0,
            stats.offline || 0,
            stats.idle || 0
        ];
        deviceChart.update();
    } catch (error) {
        console.error('Error updating device chart:', error);
    }
}

// Show real-time notification
function showDeviceNotification(message, type = 'info', deviceType = '') {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: white;
        border-radius: 12px;
        padding: 16px 20px;
        box-shadow: 0 8px 24px rgba(0,0,0,0.15);
        z-index: 2000;
        animation: slideInRight 0.4s ease;
        border-left: 4px solid;
        font-family: inherit;
        max-width: 350px;
    `;

    const colorMap = {
        'info': '#4f46e5',
        'success': '#10b981',
        'warning': '#f59e0b',
        'error': '#ef4444'
    };

    notification.style.borderLeftColor = colorMap[type] || colorMap['info'];
    notification.innerHTML = `
        <div style="display: flex; align-items: center; gap: 10px;">
            <span style="font-size: 20px;">
                ${type === 'success' ? '✅' : type === 'warning' ? '⚠️' : type === 'error' ? '❌' : 'ℹ️'}
            </span>
            <div>
                <div style="font-weight: 600; color: #2c3e50;">${message}</div>
                ${deviceType ? `<div style="font-size: 12px; color: #7f8c8d; margin-top: 4px;">${deviceType}</div>` : ''}
            </div>
        </div>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.4s ease';
        setTimeout(() => notification.remove(), 400);
    }, 3000);
}

// Add CSS animations
function addNotificationStyles() {
    if (document.getElementById('notificationStyles')) return;

    const style = document.createElement('style');
    style.id = 'notificationStyles';
    style.textContent = `
        @keyframes slideInRight {
            from {
                transform: translateX(400px);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }

        @keyframes slideOutRight {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(400px);
                opacity: 0;
            }
        }

        .notification {
            animation-duration: 0.4s;
        }
    `;
    document.head.appendChild(style);
}

// Setup WebSocket connection for real-time updates
function setupWebSocketConnection() {
    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const host = window.location.host;
    const wsUrl = `${protocol}//${host}/ws/admin`;

    try {
        const ws = new WebSocket(wsUrl);

        ws.onopen = () => {
            console.log('WebSocket connected');
            showDeviceNotification('Kết nối admin đã thiết lập', 'success', '🟢 Connected');
        };

        ws.onmessage = (event) => {
            try {
                const data = JSON.parse(event.data);
                
                // Handle device access notifications
                if (data.type === 'device_access') {
                    const emoji = getDeviceEmoji(data.deviceType);
                    showDeviceNotification(
                        `${emoji} ${data.deviceInfo || 'New Device'} đã truy cập`,
                        'info',
                        data.accessPoint || 'pois-list'
                    );

                    // Refresh online count
                    updateDeviceCount();
                }
                
                // Handle POI scan notifications
                if (data.type === 'poi_scanned') {
                    showDeviceNotification(
                        `🎧 Có người đang nghe: ${data.poiName}`,
                        'success',
                        `Lượt quét: ${data.scanCount}`
                    );

                    // Update charts
                    updateScansChart();
                }

                // Handle new scan access
                if (data.type === 'qr_scanned') {
                    const emoji = getDeviceEmoji(data.deviceType);
                    showDeviceNotification(
                        `${emoji} ${data.deviceInfo} vừa quét QR`,
                        'info',
                        data.qrCode || ''
                    );
                }
            } catch (error) {
                console.error('WebSocket message parse error:', error);
            }
        };

        ws.onerror = (error) => {
            console.error('WebSocket error:', error);
            // Silently fail - app works with polling
        };

        ws.onclose = () => {
            console.log('WebSocket disconnected');
            // Silently attempt to reconnect after 5 seconds
            setTimeout(setupWebSocketConnection, 5000);
        };
    } catch (error) {
        console.error('WebSocket setup error:', error);
    }
}

// Get device emoji
function getDeviceEmoji(deviceType) {
    const type = (deviceType || '').toLowerCase();
    if (type.includes('android')) return '🤖';
    if (type.includes('ios') || type.includes('iphone')) return '📱';
    if (type.includes('windows')) return '💻';
    if (type.includes('mac')) return '🍎';
    return '📱';
}

// Update device count
async function updateDeviceCount() {
    try {
        const response = await fetch('/api/devices/online-count');
        const data = await response.json();
        const badge = document.getElementById('devicesOnlineCount');
        if (badge) {
            badge.textContent = data.count || 0;
        }
        document.getElementById('devicesOnline').textContent = data.count || 1;
    } catch (error) {
        console.error('Error updating device count:', error);
    }
}

// Initialize on page load
function initializeDashboard() {
    addNotificationStyles();
    
    // Initialize charts only if Chart.js is loaded
    if (typeof Chart !== 'undefined') {
        setTimeout(initializeCharts, 500);
        
        // Update charts periodically
        setInterval(updateScansChart, 10000);
        setInterval(updateDeviceChart, 10000);
    } else {
        console.warn('Chart.js not loaded, skipping chart initialization');
    }

    // Setup real-time updates
    setupWebSocketConnection();
    
    // Update device count periodically
    setInterval(updateDeviceCount, 5000);
}

// Start dashboard when DOM is ready
document.addEventListener('DOMContentLoaded', initializeDashboard);
