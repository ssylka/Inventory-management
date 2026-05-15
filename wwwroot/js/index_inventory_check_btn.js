document.getElementById('btn-check-outlined')?.addEventListener('change', function () {
    document.querySelectorAll('.form-check-input').forEach(cb => cb.checked = this.checked);
});

function getSelectedUserIds() {
    return Array.from(document.querySelectorAll(".form-check-input:checked"))
        .map(cb => Number(cb.value));
}

document.getElementById("btnDelete")?.addEventListener("click", async () => {
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
        location.reload();
    }
});

document.querySelectorAll(".inventory-row").forEach(row => {
    row.addEventListener("click", function () {
        window.location.href = `/Inventory/Details/${this.dataset.id}#tab-items`;
    });
});
