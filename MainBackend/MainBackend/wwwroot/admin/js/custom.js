// Bütün silmə butonlarını seçirik
const deleteButtons = document.querySelectorAll(".delete-tag");

deleteButtons.forEach(btn => {
    btn.addEventListener("click", function (e) {
        e.preventDefault(); // Linkin birbaşa keçid etməsini dayandırırıq

        const url = this.getAttribute("href");
        const row = this.closest("li"); // Silinən elementi vizual olaraq itirmək üçün
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        Swal.fire({
            title: "Əminsiniz?",
            text: "Bu taq silinəcək!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Bəli, sil!",
            cancelButtonText: "Xeyr",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                // Fetch vasitəsilə POST sorğusu
                fetch(url, {
                    method: 'POST',
                    headers: {
                        "RequestVerificationToken": token
                    }
                }).then(response => {
                    if (response.ok) {
                        Swal.fire("Silindi!", "Taq uğurla silindi.", "success");
                        row.remove(); // Səhifəni yeniləmədən elementi ekrandan silirik
                    } else {
                        Swal.fire("Xəta!", "Bir problem baş verdi.", "error");
                    }
                });
            }
        });
    });
});

$(document).on("click", ".delete-category", function (e) {
    // Formun avtomatik göndərilməsini (səhifənin yenilənməsini) dayandırırıq
    e.preventDefault();

    let btn = $(this);
    let form = btn.closest("form"); // Butonun aid olduğu formu tapırıq
    let url = form.attr("action"); // Formun asp-action-dan gələn URL-ni oxuyuruq
    let token = $('input[name="__RequestVerificationToken"]').val(); // Formdakı gizli tokeni götürürük
    let row = btn.closest("tr"); // Silinəcək cədvəl sətrini tapırıq

    Swal.fire({
        title: "Kateqoriyanı silmək istədiyinizə əminsiniz?",
        text: "Bu əməliyyat geri qaytarıla bilməz!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Bəli, sil!",
        cancelButtonText: "Ləğv et",
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            // AJAX sorğusu
            $.ajax({
                url: url,
                type: "POST",
                data: {
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    Swal.fire(
                        "Silindi!",
                        "Kateqoriya uğurla silindi.",
                        "success"
                    );

                    // Sətri vizual olaraq yox edirik
                    row.fadeOut(600, function () {
                        $(this).remove();
                    });
                },
                error: function (xhr) {
                    Swal.fire(
                        "Xəta!",
                        "Silinmə zamanı problem baş verdi və ya icazəniz yoxdur.",
                        "error"
                    );
                    console.log(xhr.responseText);
                }
            });
        }
    });
});

$(document).on("click", ".delete-product", function (e) {
    e.preventDefault();

    let btn = $(this);
    let url = btn.data("url"); // Butondan URL-i götürürük
    let row = btn.closest("tr"); // Silinəcək cədvəl sətri

    Swal.fire({
        title: "Məhsulu silmək istəyirsiniz?",
        text: "Bu geri qaytarıla bilməyən bir əməliyyatdır!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Bəli, sil!",
        cancelButtonText: "Xeyr",
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "POST", // Controller-də HttpPost etdinizsə POST yazın
                success: function (response) {
                    Swal.fire(
                        "Silindi!",
                        "Məhsul uğurla bazadan silindi.",
                        "success"
                    );

                    // Sətri vizual olaraq itiririk
                    row.fadeOut(600, function () {
                        $(this).remove();
                    });
                },
                error: function () {
                    Swal.fire(
                        "Xəta!",
                        "Məhsul silinərkən xəta baş verdi.",
                        "error"
                    );
                }
            });
        }
    });
});


$(document).on("click", ".delete-image", function () {
    let imageId = $(this).data("id");
    let wrapper = $("#img-wrapper-" + imageId);

    if (confirm("Bu şəkli silmək istədiyinizdən əminsiniz?")) {
        $.ajax({
            url: "/Admin/Product/DeleteImage/" + imageId, // Metodun yolu
            type: "POST",
            success: function () {
                wrapper.fadeOut(300, function () { $(this).remove(); });
            },
            error: function () {
                alert("Xəta baş verdi! Şəkil silinmədi.");
            }
        });
    }
});