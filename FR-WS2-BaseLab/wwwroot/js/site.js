
/*****************************************************************************
 * GESTION DES IMAGES ASSOCIÉES AUX CATÉGORIES
 * ---------------------------------------------------------------------------
 * Affichage dynamique de l'image principale -> s'ajuste au menu déroulant.
 * Téléverse une nouvelle image pour une catégorie et met à jour l'aperçu.
 * Téléverse l'image sélectionnée après création d'une catégorie.
 *****************************************************************************/
document.addEventListener("DOMContentLoaded", function () {
    const fileInput = document.getElementById("imageFileInput");
    const preview = document.getElementById("imagePreview");
    if (fileInput && preview) {
        /**************************************************************************** 
         * GESTION DE L'APERÇU DE L'IMAGE SÉLECTIONNÉE
         * Lorsqu'un fichier est sélectionné, vérifie s'il s'agit d'une image. 
         * Si oui -> FileReader lit fichier + affiche aperçu de l'image dans <img>.
        *****************************************************************************/
        fileInput.addEventListener("change", function () {
            const file = this.files[0];
            if (file && file.type.startsWith("image/")) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = "block";};
                reader.readAsDataURL(file);}});}

    /**************************************************************************************
     * SELECT IMAGE PRINCIPALE - Aperçu de l'image associée à la catégorie sélectionnée
     * Récupère l'URL de l'image associée à l'option sélectionnée et met à jour l'aperçu.
     **************************************************************************************/
    const selectPrincipale = document.getElementById("imagePrincipaleId");
    if (selectPrincipale && preview) {
        selectPrincipale.addEventListener("change", function () {
            const selectedOption = this.options[this.selectedIndex];
            const imageUrl = selectedOption.dataset.url;
            if (imageUrl) {
                preview.src = imageUrl;
                preview.style.display = "block"; }});}

    /***********************************************************************
     * TÉLÉVERSEMENT NOUVELLE IMAGE => clic sur bouton téléversement
     * Vérifie qu'un fichier est sélectionné.
     * Si oui -> Crée FormData (categoryId, imageFile, token/jeton sécurité).
     * Si non -> Affiche message "Veuillez choisir une image." et arrête.
     * Envoie requête POST à /Categories/UploadImage. 
     * Si succès -> Affiche aperçu nouvelle image et message succès.
     * Si échec -> Affiche message d'erreur.
     ***********************************************************************/
    const btnUploadImage = document.getElementById("btnUploadImage");
    const categoryIdInput = document.getElementById("categoryId");
    if (btnUploadImage && fileInput && categoryIdInput) {
        btnUploadImage.addEventListener("click", async function () {
            if (fileInput.files.length === 0) { alert("Veuillez choisir une image."); return;}
            const formData = new FormData();
            formData.append("categoryId", categoryIdInput.value);
            formData.append("imageFile", fileInput.files[0]);
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            formData.append("__RequestVerificationToken", token);
            const response = await fetch("/Categories/UploadImage", {method: "POST", body: formData});
            const result = await response.json();
            if (!result.success) { alert(result.message); return;}
            preview.src = result.url;
            preview.style.display = "block";
            fileInput.value = "";
            alert("Image téléversée avec succès.");});}

    /****************************************************************************
     * TÉLÉVERSEMENT IMAGE => SOUMMISSION FORMULAIRE CRÉATION CATÉGORIE
     * Récupère données et jeton de sécurité (token).
     * Envoie requête POST pour créer catégorie.
     * Si succès + fichier image sélectionné -> téléverse + redirige vers index.
     * Si échec -> Affiche message d'erreur et arrête.
     ****************************************************************************/
    const createForm = document.getElementById("createCategoryForm");
    if (createForm && fileInput) {
        createForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const categoryData = new FormData();
            categoryData.append("Name", document.getElementById("Name").value);
            categoryData.append("Description", document.getElementById("Description").value);
            categoryData.append("__RequestVerificationToken", token);
            const createResponse = await fetch("/Categories/CreateCategorie", { method: "POST", body: categoryData});
            const createResult = await createResponse.json();
            if (!createResult.success) { alert(createResult.message); return;}
            if (fileInput.files.length > 0) {
                const imageData = new FormData();
                imageData.append("categoryId", createResult.categoryId);
                imageData.append("imageFile", fileInput.files[0]);
                imageData.append("__RequestVerificationToken", token);
                const uploadResponse = await fetch("/Categories/UploadImage", {method: "POST", body: imageData});
                const uploadResult = await uploadResponse.json();
                if (!uploadResult.success) {alert(uploadResult.message); return;}}
            window.location.href = "/Categories/Index";});}
});

/******************************************************************************
 * GESTION DYNAMIQUE DES MESSAGES ASSOCIÉS AUX SUJETS (INDEX)
 * ---------------------------------------------------------------------------
 * Défile messages de chaque sujet dans tableau de l'index Topics.
 * Index pour chaque sujet afin de suivre message affiché.
 * Met à jour le contenu du message dans la page en fonction de l'index.
 ******************************************************************************/
let msgIndex = {};
function changeMsg(id, direction) {
    if (!msgIndex[id]) msgIndex[id] = 0;
    let messages = window.msgData[id];
    let max = messages.length;
    msgIndex[id] = (msgIndex[id] + direction + max) % max;
    let current = messages[msgIndex[id]];
    document.getElementById("msg-content-" + id).innerHTML =
        '<div class=\"msgCarouselTitle\">' +
            '<span class=\"arrow\" onclick="changeMsg(' + id + ', -1)">◀</span>' +
            '<h4><b>Message #' + current.id + '</b></h4>' +
            '<span class=\"arrow\" onclick="changeMsg(' + id + ', 1)">▶</span>' +
        '</div>' +
        "<p class=\"msgText\"><b>" + current.texte + "</b></p>" +
        "<p><small><b>Publié le </b>" + current.date + "</small></p>" + 
        "<p><small><b>Par </b>" + (current.userName || "Anonyme") + "</small></p>";
}