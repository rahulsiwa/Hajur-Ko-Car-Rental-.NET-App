// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

    $(".custom-file-input").on("change", function () {

            var fileName = $(this).val().split("\\").pop();

    document.getElementById('PreviewPhoto').src = window.URL.createObjectURL(this.files[0]);

    document.getElementById('PhotoUrl').value = fileName;

        });
