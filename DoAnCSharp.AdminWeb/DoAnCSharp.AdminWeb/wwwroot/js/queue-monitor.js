// =====================================================
//  QR Queue Monitor — Enhanced
//  Integrated into the Admin Panel (index.html)
// =====================================================

'use strict';

// ── State ────────────────────────────────────────────
let queueRefreshInterval = null;
let queueChart = null;

// Rolling 60-second history (one point per 2 s = 30 samples max)
const CHART_MAX_POINTS = 30;
const chartLabels   = [];
const chartActive   = [];   // active-processing count
const chartQueued   = [];   // queued count

// ── Lifecycle ────────────────────────────────────────
function initQueueMonitoring() {
    initChart();
    fetchQueueStats();          // immediate first call
    if (queueRefreshInterval) clearInterval(queueRefreshInterval);
    queueRefreshInterval = setInterval(fetchQueueStats, 2000);
}

function stopQueueMonitoring() {
    if (queueRefreshInterval) {
        clearInterval(queueRefreshInterval);
        queueRefreshInterval = null;
    }
}

// ── Chart setup ──────────────────────────────────────
function initChart() {
    const canvas = document.getElementById('queueChart');
    if (!canvas) return;

    // Chart.js must be loaded (it's a synchronous <script> now, but guard anyway)
    if (typeof Chart === 'undefined') {
        console.warn('[Queue] Chart.js not loaded yet — chart skipped');
        return;
    }

    // Destroy any previous instance (tab re-entry)
    if (queueChart) { queueChart.destroy(); queueChart = null; }

    // Seed empty data
    chartLabels.length  = 0;
    chartActive.length  = 0;
    chartQueued.length  = 0;

    queueChart = new Chart(canvas.getContext('2d'), {
        type: 'line',
        data: {
            labels: chartLabels,
            datasets: [
                {
                    label: 'Đang xử lý',
                    data: chartActive,
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59,130,246,0.10)',
                    borderWidth: 2,
                    tension: 0.35,
                    fill: true,
                    pointRadius: 2,
                },
                {
                    label: 'Đang chờ',
                    data: chartQueued,
                    borderColor: '#f59e0b',
                    backgroundColor: 'rgba(245,158,11,0.10)',
                    borderWidth: 2,
                    tension: 0.35,
                    fill: true,
                    pointRadius: 2,
                },
            ],
        },
        options: {
            responsive: true,
            animation: { duration: 300 },
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { position: 'top', labels: { font: { size: 12 }, boxWidth: 14 } },
                tooltip: {
                    callbacks: {
                        title: labels => labels[0].label + ' giây trước',
                    },
                },
            },
            scales: {
                x: {
                    ticks: { font: { size: 11 }, maxTicksLimit: 10 },
                    grid:  { display: false },
                },
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1, font: { size: 11 } },
                    grid: { color: 'rgba(0,0,0,0.05)' },
                },
            },
        },
    });
}

function pushChartPoint(active, queued) {
    const now = new Date();
    const label = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

    chartLabels.push(label);
    chartActive.push(active);
    chartQueued.push(queued);

    if (chartLabels.length > CHART_MAX_POINTS) {
        chartLabels.shift();
        chartActive.shift();
        chartQueued.shift();
    }

    if (queueChart) queueChart.update('none'); // 'none' = skip animation for live updates
}

// ── Fetch & render ────────────────────────────────────
async function fetchQueueStats() {
    try {
        const res = await fetch('/api/qrqueue/stats');
        if (!res.ok) throw new Error('HTTP ' + res.status);
        const data = await res.json();
        updateQueueUI(data);
    } catch (err) {
        console.error('[Queue] fetch error:', err);
    }
}

function updateQueueUI(stats) {
    const maxC = stats.maxConcurrentRequests || 10;
    const maxQ = stats.maxQueueSize          || 100;
    const act  = stats.activeRequests        || 0;
    const que  = stats.queuedRequests        || 0;

    // ── Stat cards
    setText('queueActiveCount',    act);
    setText('queueQueuedCount',    que);
    setText('queueProcessedCount', stats.totalProcessed  || 0);
    setText('queueRejectedCount',  stats.totalRejected   || 0);
    setText('queueMaxConcurrent',  maxC);
    setText('queueMaxQueue',       maxQ);
    setText('queueRate',           (stats.averageProcessingRate || 0).toFixed(2));

    // ── Capacity bars
    const actPct = Math.min(100, (act / maxC) * 100);
    const quePct = Math.min(100, (que / maxQ) * 100);
    setStyle('queueActiveBar', 'width', actPct + '%');
    setStyle('queueQueueBar',  'width', quePct + '%');
    setText('queueActivePct', `${act} / ${maxC}`);
    setText('queueQueuedPct', `${que} / ${maxQ}`);

    // ── Uptime badge
    const upSec = stats.serviceUptimeSeconds || 0;
    setText('queueUptimeBadge', '⏱ Uptime: ' + formatUptime(upSec));

    // ── Last-refresh timestamp
    setText('queueLastRefresh', '🔄 ' + new Date().toLocaleTimeString('vi-VN'));

    // ── Chart
    pushChartPoint(act, que);

    // ── Active list
    setText('activeListCount', act);
    renderActiveList(stats.activeRequestsList || []);

    // ── Queued list
    setText('queuedListCount', que);
    renderQueuedList(stats.queuedRequestsList || []);
}

// ── List renderers ────────────────────────────────────
function renderActiveList(items) {
    const el = document.getElementById('activeRequestsList');
    if (!el) return;
    if (!items.length) {
        el.innerHTML = emptyState('✅', 'Không có request đang xử lý', 'Hệ thống đang rảnh');
        return;
    }
    el.innerHTML = items.map(item => `
        <div style="display:flex; align-items:center; gap:12px; padding:12px 14px;
                    background:#dbeafe; border-radius:10px; border:1px solid #93c5fd;
                    margin-bottom:10px; animation:fadeIn .3s ease;">
            <div style="font-size:20px; flex-shrink:0;">⚡</div>
            <div style="flex:1; min-width:0;">
                <div style="font-size:13px; font-weight:700; color:#0f172a;
                             font-family:'Courier New',monospace; overflow:hidden;
                             text-overflow:ellipsis; white-space:nowrap;">
                    ${escHtml(item.deviceId)}
                </div>
                <div style="font-size:11px; color:#64748b; margin-top:2px;">
                    📱 ${escHtml(item.qrCode)}
                </div>
            </div>
            <div style="text-align:right; flex-shrink:0;">
                <div style="font-size:13px; font-weight:700; color:#1d4ed8;">
                    ${(item.waitTimeSeconds || 0).toFixed(1)}s
                </div>
                <div style="font-size:11px; color:#64748b;">đã chờ</div>
            </div>
            <span style="flex-shrink:0; padding:4px 10px; border-radius:20px; font-size:12px;
                         font-weight:700; ${item.isPaidUser
                             ? 'background:#d1fae5;color:#065f46;'
                             : 'background:#f3f4f6;color:#374151;'}">
                ${item.isPaidUser ? '⭐ Paid' : '🆓 Free'}
            </span>
        </div>
    `).join('');
}

function renderQueuedList(items) {
    const el = document.getElementById('queuedRequestsList');
    if (!el) return;
    if (!items.length) {
        el.innerHTML = emptyState('🎉', 'Hàng đợi trống', 'Mọi request xử lý ngay lập tức');
        return;
    }
    el.innerHTML = items.map(item => `
        <div style="display:flex; align-items:center; gap:12px; padding:12px 14px;
                    background:${item.isPaidUser ? '#d1fae5' : '#f8fafc'};
                    border-radius:10px;
                    border:1px solid ${item.isPaidUser ? '#6ee7b7' : '#e2e8f0'};
                    margin-bottom:10px; animation:fadeIn .3s ease;">
            <div style="font-size:18px; font-weight:800; color:#64748b;
                         min-width:36px; text-align:center;">#${item.queuePosition}</div>
            <div style="flex:1; min-width:0;">
                <div style="font-size:13px; font-weight:700; color:#0f172a;
                             font-family:'Courier New',monospace; overflow:hidden;
                             text-overflow:ellipsis; white-space:nowrap;">
                    ${escHtml(item.deviceId)}
                </div>
                <div style="font-size:11px; color:#64748b; margin-top:2px;">
                    📱 ${escHtml(item.qrCode)}
                </div>
            </div>
            <div style="text-align:right; flex-shrink:0;">
                <div style="font-size:13px; font-weight:700; color:#92400e;">
                    ~${item.estimatedWaitSeconds || 0}s
                </div>
                <div style="font-size:11px; color:#64748b;">còn lại</div>
            </div>
            <span style="flex-shrink:0; padding:4px 10px; border-radius:20px; font-size:12px;
                         font-weight:700; ${item.isPaidUser
                             ? 'background:#d1fae5;color:#065f46;'
                             : 'background:#f3f4f6;color:#374151;'}">
                ${item.isPaidUser ? '⭐ Paid' : '🆓 Free'}
            </span>
        </div>
    `).join('');
}

// ── Simulation actions ────────────────────────────────
// completionMs: how long the server waits before auto-completing this test request
// (mirrors the real QR scan processing time so you can watch items move through)
async function addTestRequest(isPaid, completionMs = 4000) {
    try {
        const res = await fetch(
            `/api/qrqueue/test/enqueue?isPaid=${isPaid}&completionMs=${completionMs}`,
            { method: 'POST' }
        );
        if (!res.ok) throw new Error('HTTP ' + res.status);
        await fetchQueueStats();
    } catch (err) {
        console.error('[Queue] enqueue error:', err);
        alert('Lỗi khi thêm request: ' + err.message);
    }
}

async function queueSimBurst() {
    // Burst: 10 mixed requests, each auto-completing after 3–8 s randomly
    for (let i = 0; i < 10; i++) {
        const isPaid = Math.random() > 0.7;
        const ms = 3000 + Math.floor(Math.random() * 5000); // 3–8 s
        await addTestRequest(isPaid, ms);
        await delay(150);
    }
}

async function runCustomSim() {
    const count   = parseInt(document.getElementById('simCount').value,   10) || 5;
    const paidPct = parseInt(document.getElementById('simPaidPct').value, 10) / 100 || 0.3;
    const ms      = parseInt(document.getElementById('simDelay').value,   10) || 120;
    const btn     = document.getElementById('simRunBtn');
    const status  = document.getElementById('simStatus');

    // completionMs = how long each request "processes" = 2× the inter-request delay,
    // min 2 s so you can actually see it in the list, max 20 s so it clears eventually
    const completionMs = Math.min(Math.max(ms * 2, 2000), 20000);

    btn.disabled = true;
    btn.textContent = '⏳ Đang chạy...';

    for (let i = 0; i < count; i++) {
        const isPaid = Math.random() < paidPct;
        try {
            await fetch(
                `/api/qrqueue/test/enqueue?isPaid=${isPaid}&completionMs=${completionMs}`,
                { method: 'POST' }
            );
        } catch (_) { /* ignore individual failures */ }
        status.textContent = `📤 Đã gửi ${i + 1} / ${count} request...`;
        await fetchQueueStats();
        if (ms > 0) await delay(ms);
    }

    status.textContent = `✅ Hoàn thành — đã gửi ${count} requests. Hàng đợi tự dọn sau ~${Math.round(completionMs/1000)}s.`;
    btn.disabled = false;
    btn.textContent = '▶ Chạy';
    setTimeout(() => { if (status) status.textContent = ''; }, 6000);
}

async function clearQueue() {
    if (!confirm('Xóa toàn bộ hàng đợi và các request đang xử lý?')) return;
    try {
        const res = await fetch('/api/qrqueue/clear', { method: 'POST' });
        if (!res.ok) throw new Error('HTTP ' + res.status);
        await fetchQueueStats();
    } catch (err) {
        alert('Lỗi xóa hàng đợi: ' + err.message);
    }
}

// ── Helpers ───────────────────────────────────────────
function setText(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
}

function setStyle(id, prop, val) {
    const el = document.getElementById(id);
    if (el) el.style[prop] = val;
}

function delay(ms) { return new Promise(r => setTimeout(r, ms)); }

function escHtml(str) {
    return String(str)
        .replace(/&/g,'&amp;').replace(/</g,'&lt;')
        .replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function emptyState(icon, title, sub) {
    return `<div style="text-align:center; padding:32px 20px; color:#94a3b8;">
        <div style="font-size:36px; margin-bottom:8px;">${icon}</div>
        <div style="font-weight:600; font-size:14px;">${title}</div>
        <div style="font-size:12px; margin-top:4px;">${sub}</div>
    </div>`;
}

function formatUptime(totalSeconds) {
    totalSeconds = Math.floor(totalSeconds);
    const h = Math.floor(totalSeconds / 3600);
    const m = Math.floor((totalSeconds % 3600) / 60);
    const s = totalSeconds % 60;
    if (h > 0) return `${h}h ${m}m ${s}s`;
    if (m > 0) return `${m}m ${s}s`;
    return `${s}s`;
}

// ── CSS for row animation (injected once) ─────────────
(function injectStyles() {
    if (document.getElementById('qm-styles')) return;
    const style = document.createElement('style');
    style.id = 'qm-styles';
    style.textContent = `
        @keyframes fadeIn {
            from { opacity:0; transform:translateY(-6px); }
            to   { opacity:1; transform:translateY(0); }
        }
    `;
    document.head.appendChild(style);
})();

// ── Hook into existing switchTab ──────────────────────
const _origSwitchTab = window.switchTab;
window.switchTab = function(tabName) {
    if (typeof _origSwitchTab === 'function') _origSwitchTab(tabName);
    if (tabName === 'queue') initQueueMonitoring();
    else stopQueueMonitoring();
};

// Also expose addMultipleRequests for backward compat
window.addMultipleRequests = queueSimBurst;

console.log('✅ Queue-monitor.js (enhanced) loaded');
