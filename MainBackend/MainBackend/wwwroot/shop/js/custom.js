var searchBtn = document.querySelector(".search-btn");

searchBtn.addEventListener("click", function () {
    var searchText = document.querySelector(".input").value;

    fetch(`Product/Search?searchText=${searchText}`)
        .then(response => response.text())
        .then(response => {

            var productList = document.querySelector(".product-list");

            productList.innerHTML = response;
        });
})

var categoryBtns = document.querySelectorAll(".category-btn");

categoryBtns.forEach(function (item) {
    item.addEventListener("click", function () {

        var categoryId = this.getAttribute("category-id");

        fetch(`Product/FilterCategory?categoryId=${categoryId}`)
            .then(response => response.text())
            .then(response => {
                var productList = document.querySelector(".product-list");

                productList.innerHTML = response;
            })
    });
});

var sortings = document.querySelector(".sortings");

sortings.addEventListener("change", function (e) {
    var sorting = e.target.value;

    console.log(sorting);

    fetch(`Product/Sorting?sortingType=${sorting}`)
        .then(response => response.text())
        .then(response => {
            var productList = document.querySelector(".product-list");

            productList.innerHTML = response;
        });
});

var inputRange = document.querySelector(".input-range");

var products = document.querySelectorAll(".product-item-input");

inputRange.addEventListener("input", function (e) {
    var selectedPrice = parseFloat(e.target.value);

    products.forEach(function (item) {
        var productPrice = parseFloat(item.getAttribute("data-price"));

        if (productPrice <= selectedPrice) {
            item.style.display = "block";
        }
        else {
            item.style.display = "none";
        }
    })
})