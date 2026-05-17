const searchInput = document.getElementById('searchInput');

let timeout;

searchInput.addEventListener("input", function () {

    clearTimeout(timeout);

    timeout = setTimeout(() => {

        const text = searchInput.value;

        window.location.href =
            `/Inventory/Searching?searchText=${encodeURIComponent(text)}`;

    }, 500);
});