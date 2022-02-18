Dropzone.autoDiscover = false;

// Dropzone class:
var myDropzone = new Dropzone("div#mydropzone", { url: '@Url.Action("Upload", "clip")' });