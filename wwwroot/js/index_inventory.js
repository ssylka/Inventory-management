
function updateButtons() {
    const hasSelected = getSelectedUserIds().length > 0;
    document.getElementById("btnDelete").disabled = !hasSelected;
}
document.getElementById('btn-check-outlined').addEventListener('change', function () {
    var checkboxes = document.querySelectorAll('.form-check-input');
    for (var checkbox of checkboxes) {
        checkbox.checked = this.checked;
    }
});

function getSelectedUserIds() {
    return Array.from(document.querySelectorAll(".form-check-input:checked"))
        .map(cb => Number(cb.value));
}

document.getElementById("btnDelete").addEventListener("click", async () => {
    const ids = getSelectedUserIds();

    const response = await fetch('/Inventory/Delete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(ids)
    });
    if (response.ok) {
        location.reload();
    } else {
        const text = await response.text();
        alert(text);
        location.reload()
    }
});

document.querySelectorAll(".inventory-row").forEach(row => {
    row.addEventListener("click", function () {
        const id = this.dataset.id;
        //window.location.href = `/Item?inventoryId=${id}`;
        window.location.href = `/Inventory/Fields/${id}`;
    });
});

document.querySelectorAll(".row-checkbox").forEach(checkbox => {
    checkbox.addEventListener("click", function (e) {
        e.stopPropagation();
    });
});