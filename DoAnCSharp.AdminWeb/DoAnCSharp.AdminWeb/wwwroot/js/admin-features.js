// ===== IMAGE UPLOAD & PREVIEW =====
let selectedImages = [];

function handleImageSelect(event) {
    const files = Array.from(event.target.files);
    
    if (files.length > 10) {
        alert('Chỉ được chọn tối đa 10 ảnh!');
        return;
    }

    selectedImages = files;
    displayImagePreviews(files);
}

function displayImagePreviews(files) {
    const previewGrid = document.getElementById('imagePreviewGrid');
    
    if (files.length === 0) {
        previewGrid.style.display = 'none';
        return;
    }

    previewGrid.style.display = 'grid';
    previewGrid.innerHTML = '';

    files.forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = (e) => {
            const div = document.createElement('div');
            div.className = 'image-preview-item';
            div.innerHTML = `
                <img src="${e.target.result}" alt="${file.name}" />
                <button class="remove-image" onclick="removeImagePreview(${index})" type="button">×</button>
                ${index === 0 ? '<span class="main-badge">Ảnh Chính</span>' : ''}
            `;
            previewGrid.appendChild(div);
        };
        reader.readAsDataURL(file);
    });
}

function removeImagePreview(index) {
    selectedImages.splice(index, 1);
    displayImagePreviews(selectedImages);
    
    // Update file input
    const dataTransfer = new DataTransfer();
    selectedImages.forEach(file => dataTransfer.items.add(file));
    document.getElementById('poiImages').files = dataTransfer.files;
}

// ===== QR CODE GENERATION & DISPLAY =====
function generateQRCodeForPOI() {
    const randomId = 'POI_' + Math.random().toString(36).substr(2, 10).toUpperCase();
    document.getElementById('poiQRCode').value = randomId;
}

async function viewQRCode(qrCode) {
    if (!qrCode) {
        alert('Không có mã QR!');
        return;
    }

    // Create modal to show QR code
    const modal = document.createElement('div');
    modal.className = 'modal active';
    modal.innerHTML = `
        <div class="modal-content" style="max-width: 400px; text-align: center;">
            <h3>📱 Mã QR Code</h3>
            <div class="qr-code-container">
                <div class="qr-code-label">Quét mã để xem thông tin quán</div>
                <canvas id="qrCanvas"></canvas>
                <div class="qr-code-value">${qrCode}</div>
                <button class="qr-download-btn" onclick="downloadQRCode('${qrCode}')">
                    💾 Tải QR Code
                </button>
            </div>
            <div class="modal-footer">
                <button onclick="this.closest('.modal').remove()" class="secondary">Đóng</button>
            </div>
        </div>
    `;
    document.body.appendChild(modal);

    // Generate QR code using a library (you'll need to include qrcode.js)
    // For now, we'll show a placeholder
    const canvas = document.getElementById('qrCanvas');
    const ctx = canvas.getContext('2d');
    canvas.width = 256;
    canvas.height = 256;
    
    // Draw placeholder (you should use QRCode.js library for real QR codes)
    ctx.fillStyle = '#667eea';
    ctx.fillRect(0, 0, 256, 256);
    ctx.fillStyle = 'white';
    ctx.font = '16px Arial';
    ctx.textAlign = 'center';
    ctx.fillText('QR Code:', 128, 110);
    ctx.fillText(qrCode, 128, 140);
    ctx.font = '12px Arial';
    ctx.fillText('(Cần thư viện QRCode.js', 128, 170);
    ctx.fillText('để hiển thị thực tế)', 128, 190);
}

function downloadQRCode(qrCode) {
    const canvas = document.getElementById('qrCanvas');
    const link = document.createElement('a');
    link.download = `qr-${qrCode}.png`;
    link.href = canvas.toDataURL();
    link.click();
}

// ===== REAL-TIME DEVICE TRACKING =====
let onlineDevices = new Set();
let onlineUpdateInterval = null;

function startOnlineTracking() {
    // Update online count every 5 seconds
    onlineUpdateInterval = setInterval(async () => {
        await updateOnlineCount();
    }, 5000);
}

function stopOnlineTracking() {
    if (onlineUpdateInterval) {
        clearInterval(onlineUpdateInterval);
        onlineUpdateInterval = null;
    }
}

async function updateOnlineCount() {
    try {
        const response = await fetch('/api/devices/stats');
        if (!response.ok) return;

        const data = await response.json();
        
        // Update dashboard if visible
        const onlineCountElement = document.getElementById('dashboardOnlineCount');
        if (onlineCountElement) {
            onlineCountElement.textContent = data.online || 0;
        }

        // Update device list
        if (document.getElementById('devices').classList.contains('active')) {
            await loadDevicesQuickStats();
        }
    } catch (error) {
        console.error('Error updating online count:', error);
    }
}

// ===== UPLOAD IMAGES AFTER POI CREATION =====
async function uploadImagesForPOI(poiId) {
    if (selectedImages.length === 0) return;

    const formData = new FormData();
    selectedImages.forEach((file, index) => {
        formData.append('files', file);
    });
    formData.append('setMainImage', 'true'); // Set first image as main

    try {
        const response = await fetch(`/api/pois/${poiId}/images/upload`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error('Failed to upload images');
        }

        const result = await response.json();
        console.log(`✅ Uploaded ${result.images.length} images for POI ${poiId}`);
        
        // Clear selected images
        selectedImages = [];
        document.getElementById('imagePreviewGrid').style.display = 'none';
        document.getElementById('poiImages').value = '';
        
        return result;
    } catch (error) {
        console.error('Error uploading images:', error);
        alert('Lỗi khi upload ảnh: ' + error.message);
    }
}

// ===== DRAG & DROP SUPPORT =====
function initImageDragDrop() {
    const uploadArea = document.getElementById('imageUploadArea');
    if (!uploadArea) return;

    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        uploadArea.addEventListener(eventName, preventDefaults, false);
    });

    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    ['dragenter', 'dragover'].forEach(eventName => {
        uploadArea.addEventListener(eventName, () => {
            uploadArea.classList.add('drag-over');
        });
    });

    ['dragleave', 'drop'].forEach(eventName => {
        uploadArea.addEventListener(eventName, () => {
            uploadArea.classList.remove('drag-over');
        });
    });

    uploadArea.addEventListener('drop', (e) => {
        const dt = e.dataTransfer;
        const files = dt.files;
        document.getElementById('poiImages').files = files;
        handleImageSelect({ target: { files } });
    });
}

// ===== BADGE HELPERS =====
function createStatusBadge(isOnline) {
    return `<span class="badge ${isOnline ? 'online' : 'offline'}">
        ${isOnline ? '🟢 Online' : '🔴 Offline'}
    </span>`;
}

function createPaymentBadge(isPremium) {
    return `<span class="badge ${isPremium ? 'premium' : 'free'}">
        ${isPremium ? '💳 Premium' : '🆓 Free'}
    </span>`;
}

function formatDateTime(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN');
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
}

// ===== INITIALIZE ON PAGE LOAD =====
document.addEventListener('DOMContentLoaded', () => {
    initImageDragDrop();
    startOnlineTracking();
    
    // Clean up on page unload
    window.addEventListener('beforeunload', () => {
        stopOnlineTracking();
    });
});
