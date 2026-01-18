// LoadMore in HomeController

let loadMore = document.querySelector(".load-more");

loadMore.addEventListener("click", function () {
    let htmlProductCount = document.querySelectorAll(".product").length;

    let dbProductCount = document.querySelector(".product-count").value;

    console.log(dbProductCount);

    fetch(`Home/LoadMore?skip=${htmlProductCount}`)
        .then(response => response.text())
        .then(response => {
            let parent = document.querySelector(".load-products");

            parent.innerHTML += response;

            let htmlProductCount = document.querySelectorAll(".product").length;

            if (htmlProductCount >= dbProductCount) {
                this.style.display = "none";
            }
        })
});

// LoadMore button in Product page

$(document).on("click", ".load-more", function () {
    let skip = $("#product-container .product").length; // Count existing products
    let totalCount = $(".product-count").val(); // Get total from hidden input
    let parent = $("#product-container");

    $.ajax({
        url: "/Product/LoadMore", // Matches the controller/action
        type: "Get",
        data: {
            skip: skip
        },
        success: function (res) {
            // Append the new partial HTML to the row
            $(parent).append(res);

            // Check if we should hide the button
            let newCount = $("#product-container .product").length;
            if (newCount >= totalCount) {
                $(".load-more").addClass("d-none"); // Hide button if no more products
            }
        },
        error: function (err) {
            console.log("Error loading more products", err);
        }
    });
});
