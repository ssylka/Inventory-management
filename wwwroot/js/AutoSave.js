document.addEventListener("DOMContentLoaded", () => {
    let hasChanges = false;
    let currentOrders = [];

    const container = document.getElementById("elements");
    const status = document.getElementById("saveStatus");

    function setStatus(text, color) {
        status.innerHTML = `<span class="badge ${color}">${text}</span>`;
    }

    new Sortable(container, {
        animation: 150,
        handle: ".drag-handle",
        onEnd: () => {

            currentOrders = [...container.querySelectorAll("tr")].map((row, index) => ({
                Id: Number(row.dataset.id),
                Order: index
            }));

            hasChanges = true;
            setStatus("Unsaved", "bg-warning");
        }
    });

    setInterval(async () => {
        if (!hasChanges) return;
        console.log("Sending:", currentOrders);
        setStatus("Saving...", "bg-info");

        const response = await fetch('/Inventory/UpdateFieldOrder', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(currentOrders)
        });

        if (response.ok) {
            hasChanges = false;
            setStatus("Saved", "bg-success");
        } else {
            setStatus("Error", "bg-danger");
        }

    }, 10000);

    window.addEventListener("beforeunload", async (e) => {
        if (!hasChanges) return;

        await fetch('/Inventory/UpdateFieldOrder', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(currentOrders)
        });
    });
});