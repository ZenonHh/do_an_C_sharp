// ===== DEVICES MANAGEMENT WITH PAGINATION ===== 

let devicesData = [];
let currentFilterStatus = 'all';
let currentViewMode = 'card';
let devicesRefreshInterval;
let currentDevicePage = 0;
const DEVICES_PER_PAGE = 5;

// Load devices khi load trang
document.addEventListener('DOMContentLoaded', function() {
  // Only init if we're on devices tab
  const devicesTab = document.getElementById('devices');
  if (devicesTab) {
    initDevicesSection();
  }
});

function initDevicesSection() {
  loadAllDevices();
  setupDevicesEventListeners();
  startDevicesAutoRefresh();
}

// ===== API CALLS =====
async function loadAllDevices() {
  try {
    const response = await fetch('/api/devices');
    if (!response.ok) throw new Error('Không lấy được danh sách thiết bị');

    devicesData = await response.json();
    currentDevicePage = 0;
    renderDevicesUI();
  } catch (error) {
    console.error('❌ Lỗi khi lấy devices:', error);
    showDevicesError('Lỗi khi tải danh sách thiết bị');
  }
}

async function toggleDeviceStatus(deviceId) {
  try {
    const device = devicesData.find(d => d.id === deviceId);
    if (!device) return;
    
    const response = await fetch(`/api/devices/${deviceId}/toggle`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ isActive: !device.isActive })
    });
    
    if (!response.ok) throw new Error('Không thể cập nhật trạng thái');
    
    device.isActive = !device.isActive;
    renderDevicesUI();
    showDevicesSuccess(`Thiết bị ${device.isActive ? 'kích hoạt' : 'vô hiệu hóa'} thành công`);
  } catch (error) {
    console.error('❌ Lỗi khi cập nhật:', error);
    showDevicesError('Lỗi khi cập nhật trạng thái');
  }
}

async function deleteDevice(deviceId) {
  if (!confirm('Bạn chắc chắn muốn xóa thiết bị này?')) return;
  
  try {
    const response = await fetch(`/api/devices/${deviceId}`, {
      method: 'DELETE'
    });
    
    if (!response.ok) throw new Error('Không thể xóa thiết bị');
    
    devicesData = devicesData.filter(d => d.id !== deviceId);
    renderDevicesUI();
    showDevicesSuccess('Xóa thiết bị thành công');
  } catch (error) {
    console.error('❌ Lỗi khi xóa:', error);
    showDevicesError('Lỗi khi xóa thiết bị');
  }
}

// ===== FILTER & SEARCH =====
function filterDevices(status) {
  currentFilterStatus = status;
  currentDevicePage = 0;

  // Update filter button states
  document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.classList.remove('active');
  });
  event.target.classList.add('active');

  renderDevicesUI();
}

function searchDevices(query) {
  const filtered = devicesData.filter(device => {
    const q = query.toLowerCase();
    return (
      device.deviceName?.toLowerCase().includes(q) ||
      device.deviceModel?.toLowerCase().includes(q) ||
      device.ipAddress?.toLowerCase().includes(q) ||
      device.deviceOS?.toLowerCase().includes(q)
    );
  });

  currentDevicePage = 0;
  renderDevicesWithFilter(filtered);
}

function applyFilter(devices) {
  if (currentFilterStatus === 'online') {
    return devices.filter(d => d.isOnline);
  } else if (currentFilterStatus === 'offline') {
    return devices.filter(d => !d.isOnline);
  }
  return devices;
}

// ===== VIEW MODES =====
function setViewMode(mode) {
  currentViewMode = mode;
  
  document.querySelectorAll('.view-btn').forEach(btn => {
    btn.classList.remove('active');
  });
  document.querySelector(`[data-view="${mode}"]`).classList.add('active');
  
  renderDevicesUI();
}

// ===== RENDER UI WITH PAGINATION =====
function renderDevicesUI() {
  const filtered = applyFilter(devicesData);
  updateDevicesStats(filtered);

  if (currentViewMode === 'card') {
    renderDevicesCards(filtered);
  } else {
    renderDevicesTable(filtered);
  }
}

function renderDevicesWithFilter(devices) {
  updateDevicesStats(devices);

  if (currentViewMode === 'card') {
    renderDevicesCards(devices);
  } else {
    renderDevicesTable(devices);
  }
}

function updateDevicesStats(devices) {
  const online = devices.filter(d => d.isOnline).length;
  const offline = devices.filter(d => !d.isOnline).length;
  const total = devices.length;

  const statsHtml = `
    <div class="device-stat-card online">
      <h4>🟢 Online</h4>
      <div class="count">${online}</div>
    </div>
    <div class="device-stat-card offline">
      <h4>🔴 Offline</h4>
      <div class="count">${offline}</div>
    </div>
    <div class="device-stat-card">
      <h4>📱 Tổng</h4>
      <div class="count">${total}</div>
    </div>
  `;

  const statsContainer = document.getElementById('devicesStats');
  if (statsContainer) {
    statsContainer.innerHTML = statsHtml;
  }
}

function renderDevicesCards(devices) {
  const container = document.getElementById('devicesList');

  if (!container) return;

  if (devices.length === 0) {
    container.innerHTML = '<div class="no-devices"><p>Không tìm thấy thiết bị</p></div>';
    renderDevicesPagination(0);
    return;
  }

  // Pagination
  const totalPages = Math.ceil(devices.length / DEVICES_PER_PAGE);
  const startIdx = currentDevicePage * DEVICES_PER_PAGE;
  const endIdx = startIdx + DEVICES_PER_PAGE;
  const devicesToShow = devices.slice(startIdx, endIdx);

  container.className = 'devices-container';
  container.innerHTML = devicesToShow.map(device => `
    <div class="device-card">
      <div class="device-header">
        <div class="device-name">${device.deviceName || 'Unknown'}</div>
        <div class="device-status ${device.isOnline ? 'online pulsing' : 'offline'}">
          ${device.isOnline ? 'Online' : 'Offline'}
        </div>
      </div>

      <div class="device-info">
        <div class="device-info-row">
          <span class="device-info-label">Model:</span>
          <span class="device-info-value">${device.deviceModel || '-'}</span>
        </div>
        <div class="device-info-row">
          <span class="device-info-label">OS:</span>
          <span class="device-info-value">${device.deviceOS || '-'}</span>
        </div>
        <div class="device-info-row">
          <span class="device-info-label">App:</span>
          <span class="device-info-value">${device.appVersion || '-'}</span>
        </div>
        <div class="device-info-row">
          <span class="device-info-label">IP:</span>
          <span class="device-info-value">${device.ipAddress || '-'}</span>
        </div>
        <div class="device-info-row">
          <span class="device-info-label">Cuối:</span>
          <span class="device-info-value">${formatTime(device.lastOnlineAt)}</span>
        </div>
      </div>

      ${device.locationInfo ? `<div class="device-location">${device.locationInfo}</div>` : ''}

      <div class="device-actions">
        <button onclick="toggleDeviceStatus(${device.id})">
          ${device.isActive ? '✓ Kích Hoạt' : '✗ Vô Hiệu'}
        </button>
        <button class="danger" onclick="deleteDevice(${device.id})">🗑️ Xóa</button>
      </div>
    </div>
  `).join('');

  renderDevicesPagination(totalPages);
}

function renderDevicesTable(devices) {
  const container = document.getElementById('devicesList');

  if (!container) return;

  if (devices.length === 0) {
    container.innerHTML = '<div class="no-devices"><p>Không tìm thấy thiết bị</p></div>';
    renderDevicesPagination(0);
    return;
  }

  // Pagination
  const totalPages = Math.ceil(devices.length / DEVICES_PER_PAGE);
  const startIdx = currentDevicePage * DEVICES_PER_PAGE;
  const endIdx = startIdx + DEVICES_PER_PAGE;
  const devicesToShow = devices.slice(startIdx, endIdx);

  container.className = '';
  container.innerHTML = `
    <table class="devices-table">
      <thead>
        <tr>
          <th>Tên Thiết Bị</th>
          <th>Model</th>
          <th>OS</th>
          <th>App</th>
          <th>Trạng Thái</th>
          <th>IP Address</th>
          <th>Cuối Cùng Online</th>
          <th>Hành Động</th>
        </tr>
      </thead>
      <tbody>
        ${devicesToShow.map(device => `
          <tr>
            <td>${device.deviceName || '-'}</td>
            <td>${device.deviceModel || '-'}</td>
            <td>${device.deviceOS || '-'}</td>
            <td>${device.appVersion || '-'}</td>
            <td class="status-${device.isOnline ? 'online' : 'offline'}">
              ${device.isOnline ? '🟢 Online' : '🔴 Offline'}
            </td>
            <td>${device.ipAddress || '-'}</td>
            <td>${formatTime(device.lastOnlineAt)}</td>
            <td>
              <button style="padding: 4px 8px; font-size: 11px; margin-right: 4px;" onclick="toggleDeviceStatus(${device.id})">
                ${device.isActive ? '✓' : '✗'}
              </button>
              <button style="padding: 4px 8px; font-size: 11px; background: #e74c3c; color: white; border: none;" onclick="deleteDevice(${device.id})">🗑️</button>
            </td>
          </tr>
        `).join('')}
      </tbody>
    </table>
  `;

  renderDevicesPagination(totalPages);
}

function renderDevicesPagination(totalPages) {
  const paginationDiv = document.createElement('div');
  paginationDiv.className = 'pagination';
  paginationDiv.style.marginTop = '20px';

  if (totalPages <= 1) {
    const container = document.getElementById('devicesList');
    const existing = container.parentElement.querySelector('.pagination');
    if (existing) existing.remove();
    return;
  }

  let html = '';

  // Previous
  html += `<button ${currentDevicePage === 0 ? 'disabled' : ''} onclick="changeDevicePage(${currentDevicePage - 1})">◀ Trước</button>`;

  // Pages
  for (let i = 0; i < totalPages; i++) {
    html += `<button class="${i === currentDevicePage ? 'active' : ''}" onclick="changeDevicePage(${i})">${i + 1}</button>`;
  }

  // Next
  html += `<button ${currentDevicePage >= totalPages - 1 ? 'disabled' : ''} onclick="changeDevicePage(${currentDevicePage + 1})">Sau ▶</button>`;

  paginationDiv.innerHTML = html;

  const container = document.getElementById('devicesList');
  const existing = container.parentElement.querySelector('.pagination');
  if (existing) existing.remove();
  container.parentElement.appendChild(paginationDiv);
}

function changeDevicePage(page) {
  const filtered = applyFilter(devicesData);
  const totalPages = Math.ceil(filtered.length / DEVICES_PER_PAGE);
  if (page < 0 || page >= totalPages) return;
  currentDevicePage = page;
  renderDevicesUI();
  document.getElementById('devicesList').scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// ===== EVENT LISTENERS =====
function setupDevicesEventListeners() {
  const searchInput = document.getElementById('devicesSearchInput');
  if (searchInput) {
    searchInput.addEventListener('input', (e) => searchDevices(e.target.value));
  }
  
  const refreshBtn = document.getElementById('devicesRefreshBtn');
  if (refreshBtn) {
    refreshBtn.addEventListener('click', () => {
      refreshBtn.classList.add('spinning');
      loadAllDevices().finally(() => {
        refreshBtn.classList.remove('spinning');
      });
    });
  }
}

// ===== AUTO REFRESH =====
function startDevicesAutoRefresh() {
  clearInterval(devicesRefreshInterval);
  devicesRefreshInterval = setInterval(() => {
    loadAllDevices();
  }, 5000); // Refresh mỗi 5 giây
}

// ===== HELPERS =====
function formatTime(date) {
  if (!date) return '-';
  const d = new Date(date);
  return d.toLocaleString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
}

function showDevicesSuccess(msg) {
  console.log('✅', msg);
  // Có thể thêm toast notification sau
}

function showDevicesError(msg) {
  console.error('❌', msg);
  // Có thể thêm toast notification sau
}

// Cleanup khi đổi tab
function stopDevicesAutoRefresh() {
  clearInterval(devicesRefreshInterval);
}
