const fileInput = document.getElementById("imageFileInput");
const preview = document.getElementById("imagePreview");

if (fileInput) {
    fileInput.addEventListener("change", function () {
    const file = this.files[0];
    if (file && file.type.startsWith("image/")) {
        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = "block";
        };
        reader.readAsDataURL(file);
    }});}