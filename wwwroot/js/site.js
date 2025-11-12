// Dark/Light Theme Toggle
document.addEventListener('DOMContentLoaded', function() {
    initializeThemeToggle();
    initializeFileUpload();
});

function initializeThemeToggle() {
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');
    
    if (!themeToggle || !themeIcon) return;

    // Load saved theme or default to light
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-bs-theme', savedTheme);
    updateThemeIcon(savedTheme);

    // Toggle theme on click
    themeToggle.addEventListener('click', function() {
        const currentTheme = document.documentElement.getAttribute('data-bs-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        document.documentElement.setAttribute('data-bs-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        updateThemeIcon(newTheme);
    });

    function updateThemeIcon(theme) {
        if (theme === 'dark') {
            themeIcon.className = 'fas fa-sun';
            themeIcon.style.color = '#ffc107';
        } else {
            themeIcon.className = 'fas fa-moon';
            themeIcon.style.color = '#7c3aed';
        }
    }
}

// File Upload Functionality
function initializeFileUpload() {
    const dropZone = document.querySelector('.file-drop-zone');
    if (!dropZone) return;

    dropZone.addEventListener('dragover', handleDragOver);
    dropZone.addEventListener('drop', handleFileDrop);
    dropZone.addEventListener('click', () => document.getElementById('fileInput').click());
}

function handleDragOver(e) {
    e.preventDefault();
    e.currentTarget.classList.add('drag-over');
}

function handleFileDrop(e) {
    e.preventDefault();
    e.currentTarget.classList.remove('drag-over');
    handleFileSelect(e.dataTransfer.files);
}

function handleFileSelect(files) {
    const preview = document.getElementById('filePreview');
    if (!preview) return;

    preview.innerHTML = '';
    
    Array.from(files).forEach((file, index) => {
        if (validateFile(file)) {
            createFilePreview(file, index);
        }
    });
    
    updateFileInput();
}

function validateFile(file) {
    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = ['image/', 'application/pdf', 'application/vnd.openxmlformats-officedocument.'];
    
    if (file.size > maxSize) {
        alert(`File ${file.name} is too large. Maximum size is 10MB.`);
        return false;
    }
    
    const isValidType = allowedTypes.some(type => file.type.includes(type));
    if (!isValidType) {
        alert(`File type not allowed for ${file.name}`);
        return false;
    }
    
    return true;
}

function createFilePreview(file, index) {
    const preview = document.getElementById('filePreview');
    const reader = new FileReader();
    
    reader.onload = function(e) {
        const previewItem = document.createElement('div');
        previewItem.className = 'file-preview-item';
        previewItem.file = file;
        
        let content = '';
        if (file.type.includes('image')) {
            content = `<img src="${e.target.result}" class="img-thumbnail mb-2" style="height: 80px; object-fit: cover;">`;
        } else {
            const icon = file.type.includes('pdf') ? 'file-pdf' : 
                        file.type.includes('word') ? 'file-word' : 'file-excel';
            content = `<i class="fas fa-${icon} fa-3x text-primary mb-2"></i>`;
        }
        
        previewItem.innerHTML = `
            ${content}
            <small class="d-block text-truncate" style="max-width: 100px;">${file.name}</small>
            <small class="text-secondary">${(file.size / 1024 / 1024).toFixed(1)}MB</small>
            <button type="button" class="btn-close position-absolute top-0 end-0 m-1" 
                    onclick="removeFilePreview(this)"></button>
        `;
        
        preview.appendChild(previewItem);
    };
    
    if (file.type.includes('image')) {
        reader.readAsDataURL(file);
    } else {
        reader.readAsText(file.slice(0, 100));
    }
}

function removeFilePreview(button) {
    button.closest('.file-preview-item').remove();
    updateFileInput();
}

function updateFileInput() {
    const fileInput = document.getElementById('fileInput');
    if (!fileInput) return;

    const dataTransfer = new DataTransfer();
    document.querySelectorAll('.file-preview-item').forEach(item => {
        dataTransfer.items.add(item.file);
    });
    fileInput.files = dataTransfer.files;
}