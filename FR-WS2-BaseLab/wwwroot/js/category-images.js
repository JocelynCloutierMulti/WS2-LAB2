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
        col.className = 'col-md-4';

        col.innerHTML = `
            <div class="card h-100">
                <img src="${image.url}" class="card-img-top"
                     alt="${image.altText ?? ''}">
                <div class="card-body">
                    <p class="card-text small">${image.originalFileName}</p>
                    <button class="btn btn-sm btn-danger"
                            data-image-id="${image.id}">
                        Supprimer
                    </button>
                </div>
            </div>`;

        col.querySelector('button').addEventListener('click', async () => {
            await deleteImage(image.id);
            col.remove();
        });

        gallery.prepend(col);
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

    form.addEventListener('submit', async event => {
        event.preventDefault();
        status.textContent = 'Téléversement en cours...';

        const formData = new FormData(form);

        const response = await fetch(`/categories/${categoryId}/images`, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token },
            body: formData
        });

        if (!response.ok) {
            const error = await response.json();
            status.textContent = error.error ?? 'Erreur de téléversement.';
            return;
        }

        const image = await response.json();
        addImageCard(image);
        form.reset();
        status.textContent = 'Image téléversée.';
    });

    loadImages();
});
