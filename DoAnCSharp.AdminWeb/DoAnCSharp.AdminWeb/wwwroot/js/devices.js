// ===== DEVICES MANAGEMENT =====

let devicesData = [];
let currentDevicePage = 0;
let currentFilterStatus = 'all';
const DEVICES_PER_PAGE = 10;
let devicesRefreshInterval;

const DESKTOP_OS = ['Windows', 'macOS', 'Linux'];

function isOnline(device) {
  const sixtySecAgo = new Date(Date.now() - 60 * 1000);
  return new Date(device.lastOnlineAt) >= sixtySecAgo && !DESKTOP_OS.includes(device.deviceOS);
}

document.addEventListener('DOMContentLoaded', function () {
  if (document.getElementById('devices')) {
    setupDevicesEventListeners();
    startDevicesAutoRefresh();
  }
});

async function loadAllDevices() {
  const container = document.getElementById('devicesList');
  if (container) container.innerHTML = '<div class="loading" style="padding:40px 0;"><div class="spinner"></div></div>';

  try {
    const res = await fetch('/api/devices');
    if (!res.ok) throw new Error();
    devicesData = await res.json();
    currentDevicePage = 0;
    renderDevicesTable(getFilteredDevices());
  } catch {
    if (container) container.innerHTML = '<p style="text-align:center;color:#e74c3c;padding:40px;">❌ Lỗi tải danh sách thiết bị</p>';
  }
}

function filterDevices(status) {
  currentFilterStatus = status;
  currentDevicePage = 0;
  document.querySelectorAll('#devices .filter-btn').forEach(btn => btn.classList.remove('active'));
  event.target.classList.add('active');
  renderDevicesTable(getFilteredDevices());
}

function getFilteredDevices() {
  let filtered = devicesData;

  if (currentFilterStatus === 'online') filtered = filtered.filter(isOnline);
  else if (currentFilterStatus === 'offline') filtered = filtered.filter(d => !isOnline(d));

  const q = (document.getElementById('devicesSearchInput')?.value || '').toLowerCase().trim();
  if (q) {
    filtered = filtered.filter(d =>
      d.deviceName?.toLowerCase().includes(q) ||
      d.deviceModel?.toLowerCase().includes(q) ||
      d.ipAddress?.toLowerCase().includes(q) ||
      d.deviceOS?.toLowerCase().includes(q)
    );
  }
  return filtered;
}

function renderDevicesTable(devices) {
  const container = document.getElementById('devicesList');
  if (!container) return;

  const existing = container.parentElement.querySelector('.devices-pagination');
  if (existing) existing.remove();

  if (devices.length === 0) {
    container.innerHTML = '<p style="text-align:center;color:#999;padding:40px;">Không tìm thấy thiết bị nào</p>';
    return;
  }

  const totalPages = Math.ceil(devices.length / DEVICES_PER_PAGE);
  const start = currentDevicePage * DEVICES_PER_PAGE;
  const paged = devices.slice(start, start + DEVICES_PER_PAGE);

  container.innerHTML = `
    <table>
      <thead>
        <tr>
          <th style="width:50px;">ID</th>
          <th>Tên Thiết Bị</th>
          <th>Model</th>
          <th>Hệ Điều Hành</th>
          <th>Phiên Bản App</th>
          <th>IP Address</th>
          <th>Cuối Online</th>
          <th>Đăng Ký</th>
          <th style="text-align:center;">Thao Tác</th>
        </tr>
      </thead>
      <tbody>
        ${paged.map((d, i) => {
          const osIcon = d.deviceOS === 'Android' ? '🤖' : d.deviceOS === 'iOS' ? '🍎' : '💻';
          return `
          <tr>
            <td style="text-align:center;color:#999;">${start + i + 1}</td>
            <td style="font-weight:600;">${d.deviceName || '<span style="color:#999;">—</span>'}</td>
            <td>${d.deviceModel || '—'}</td>
            <td>${osIcon} ${d.deviceOS || '—'}</td>
            <td>${d.appVersion || '—'}</td>
            <td style="font-size:13px;color:#6b7280;">${d.ipAddress || '—'}</td>
            <td style="font-size:13px;color:#6b7280;">${formatDeviceTime(d.lastOnlineAt)}</td>
            <td style="font-size:13px;color:#6b7280;">${formatDeviceTime(d.registeredAt)}</td>
            <td style="text-align:center;">
              <button onclick="deleteDevice(${d.id})" class="small danger">🗑️ Xóa</button>
            </td>
          </tr>`;
        }).join('')}
      </tbody>
    </table>
  `;

  renderDevicesPagination(totalPages);
}

function renderDevicesPagination(totalPages) {
  const container = document.getElementById('devicesList');
  if (!container || totalPages <= 1) return;

  const paginationDiv = document.createElement('div');
  paginationDiv.className = 'pagination devices-pagination';
  paginationDiv.style.marginTop = '20px';

  let html = `<button ${currentDevicePage === 0 ? 'disabled' : ''} onclick="changeDevicePage(${currentDevicePage - 1})">◀ Trước</button>`;
  for (let i = 0; i < totalPages; i++) {
    html += `<button class="${i === currentDevicePage ? 'active' : ''}" onclick="changeDevicePage(${i})">${i + 1}</button>`;
  }
  html += `<button ${currentDevicePage >= totalPages - 1 ? 'disabled' : ''} onclick="changeDevicePage(${currentDevicePage + 1})">Sau ▶</button>`;

  paginationDiv.innerHTML = html;
  container.parentElement.appendChild(paginationDiv);
}

function changeDevicePage(page) {
  const filtered = getFilteredDevices();
  const totalPages = Math.ceil(filtered.length / DEVICES_PER_PAGE);
  if (page < 0 || page >= totalPages) return;
  currentDevicePage = page;
  renderDevicesTable(filtered);
  document.getElementById('devicesList')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

async function deleteDevice(deviceId) {
  if (!confirm('Xóa thiết bị này?\nHành động này không thể hoàn tác.')) return;
  try {
    const res = await fetch(`/api/devices/${deviceId}`, { method: 'DELETE' });
    if (!res.ok) throw new Error();
    devicesData = devicesData.filter(d => d.id !== deviceId);
    renderDevicesTable(getFilteredDevices());
  } catch {
    alert('Lỗi khi xóa thiết bị');
  }
}

function setupDevicesEventListeners() {
  const searchInput = document.getElementById('devicesSearchInput');
  if (searchInput) {
    searchInput.addEventListener('input', () => {
      currentDevicePage = 0;
      renderDevicesTable(getFilteredDevices());
    });
  }

  const refreshBtn = document.getElementById('devicesRefreshBtn');
  if (refreshBtn) {
    refreshBtn.addEventListener('click', () => {
      refreshBtn.classList.add('spinning');
      loadAllDevices().finally(() => refreshBtn.classList.remove('spinning'));
    });
  }
}

function startDevicesAutoRefresh() {
  clearInterval(devicesRefreshInterval);
  devicesRefreshInterval = setInterval(() => {
    if (document.getElementById('devices')?.classList.contains('active')) {
      loadAllDevices();
    }
  }, 15000);
}

function stopDevicesAutoRefresh() {
  clearInterval(devicesRefreshInterval);
}

function formatDeviceTime(date) {
  if (!date) return '—';
  return new Date(date).toLocaleString('vi-VN', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit'
  });
}
