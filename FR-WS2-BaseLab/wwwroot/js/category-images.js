document.addEventListener('DOMContentLoaded', () => {
    const section = document.getElementById('category-images-section');
    if (!section) return;

    const categoryId = section.dataset.categoryId;
    const form = document.getElementById('image-upload-form');
    const gallery = document.getElementById('category-images-gallery'); 

    const status = document.getElementById('image-upload-status');

    const token = document.querySelector(
        'input[name="__RequestVerificationToken"]')?.value;

    async function loadImages() {
        const response = await fetch(`/categories/${categoryId}/images`);
        const images = await response.json();
        gallery.innerHTML = '';
        images.forEach(addImageCard);
    }

    function addImageCard(image) {
        const col = document.createElement('div');
        const sizeMb = (image.sizeBytes / 1024 / 1024).toFixed(2);
        col.className = 'col-md-4';

        col.innerHTML = ` 
           <div class="card h-100">
                <img src="${escapeHtml(image.url)}"
                     class="card-img-top"
                     alt="${escapeHtml(image.altText ?? '')}"
                     style="object-fit: cover; max-height: 200px;">
                <div class="card-body">
                    <p class="card-text small text-truncate" title="${escapeHtml(image.originalFileName)}">
                        ${escapeHtml(image.originalFileName)}
                    </p>
                    <p class="card-text small text-muted">${sizeMb} Mo</p>
                    <button class="btn btn-sm btn-outline-danger btn-delete"
                            data-image-id="${image.id}">
                        Supprimer
                    </button>
                </div>
            </div>`;

        col.querySelector('.btn-delete').addEventListener('click', async () => {
            await deleteImage(image.id);
            col.remove();
        });

        gallery.prepend(col);
    }

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    async function deleteImage(imageId) {
        const response = await fetch(
            `/categories/${categoryId}/images/${imageId}`,
            {
                method: 'DELETE',
                headers: { 'RequestVerificationToken': token }
            });

        if (!response.ok) {
            status.textContent = 'Suppression impossible.';
        }
    }

    document.getElementById('image-file').addEventListener('change', function () {
        const preview = document.getElementById('image-preview');
        const file = this.files[0];
        if (!file) { preview.src = ''; preview.hidden = true; return; }
        preview.src = URL.createObjectURL(file);
        preview.hidden = false;
    });
    form.addEventListener('submit', event => {
        event.preventDefault();
        status.textContent = 'Téléversement en cours...';

        const formData = new FormData(form);

        const xhr = new XMLHttpRequest();

        xhr.upload.addEventListener('progress', e => {
            if (e.lengthComputable) {
                const pct = Math.round((e.loaded / e.total) * 100);
                status.textContent = `Téléversement : ${pct}%`;
            }
        });

        xhr.addEventListener('load', async () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                const image = JSON.parse(xhr.responseText);
                addImageCard(image);
                form.reset();
                const preview = document.getElementById('image-preview');
                if (preview) { preview.src = ''; preview.hidden = true; }
                status.textContent = 'Image téléversée.';
            } else {
                try {
                    const error = JSON.parse(xhr.responseText);
                    status.textContent = error.error ?? 'Erreur de téléversement.';
                } catch {
                    status.textContent = 'Erreur de téléversement.';
                }
            }
        });

        // Erreur réseau
        xhr.addEventListener('error', () => {
            status.textContent = 'Erreur réseau inattendue.';
        });

        xhr.open('POST', `/categories/${categoryId}/images`);
        xhr.setRequestHeader('RequestVerificationToken', token);
        xhr.send(formData);
    });

    loadImages();
});

