// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Theme Toggle Functionality

// File Preview Functionality
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
    preview.innerHTML = '';
    
    Array.from(files).forEach((file, index) => {
        if (validateFile(file)) {
            createFilePreview(file, index);
        }
    });
    
    // Update the file input
    const dataTransfer = new DataTransfer();
    Array.from(document.querySelectorAll('.file-preview-item')).forEach(item => {
        dataTransfer.items.add(item.file);
    });
    document.getElementById('fileInput').files = dataTransfer.files;
}

function validateFile(file) {
    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = ['image/', 'application/pdf', 
                         'application/vnd.openxmlformats-officedocument.'];
    
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
        previewItem.className = 'file-preview-item glass-card p-3 m-2 d-inline-block';
        previewItem.style.width = '120px';
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
            <button type="button" class="btn-close btn-close-white position-absolute top-0 end-0 m-1" 
                    onclick="removeFilePreview(this)"></button>
        `;
        
        preview.appendChild(previewItem);
    };
    
    if (file.type.includes('image')) {
        reader.readAsDataURL(file);
    } else {
        reader.readAsText(file.slice(0, 100)); // Read first 100 chars for text preview
    }
}

function removeFilePreview(button) {
    button.closest('.file-preview-item').remove();
    updateFileInput();
}

function updateFileInput() {
    const dataTransfer = new DataTransfer();
    document.querySelectorAll('.file-preview-item').forEach(item => {
        dataTransfer.items.add(item.file);
    });
    document.getElementById('fileInput').files = dataTransfer.files;
}