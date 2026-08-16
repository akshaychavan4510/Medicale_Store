// sale-entry.js
const GST_RATE = 5; // 5%
let rowIndex = 0;

function getMedicineOptions() {
    // Grab options from the first existing select or the template
    const template = document.getElementById('rowTemplate');
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = template.innerHTML.replace(/__IDX__/g, '0');
    return tempDiv.querySelector('.med-select').innerHTML;
}

function addRow() {
    const tbody = document.getElementById('detailsBody');
    const template = document.getElementById('rowTemplate');
    let html = template.innerHTML.replace(/__IDX__/g, rowIndex);
    const tr = document.createElement('tr');
    tr.innerHTML = html.replace(/<tr[^>]*>|<\/tr>/gi, '');

    // Re-build from template cleanly
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = template.innerHTML.replace(/__IDX__/g, rowIndex);
    const newRow = tempDiv.querySelector('tr');

    tbody.appendChild(newRow);
    bindRowEvents(newRow);
    rowIndex++;
    recalcGrandTotal();
}

function bindRowEvents(row) {
    const medSelect = row.querySelector('.med-select');
    const rateInput = row.querySelector('.rate-input');
    const qtyInput = row.querySelector('.qty-input');
    const removeBtn = row.querySelector('.remove-row');

    // On medicine change: fetch rate via AJAX
    medSelect.addEventListener('change', function () {
        const medId = this.value;
        if (!medId) {
            rateInput.value = '';
            calcLine(row);
            return;
        }
        fetch('/Sale/GetMedicineInfo/' + medId)
            .then(r => r.json())
            .then(data => {
                rateInput.value = parseFloat(data.rate).toFixed(2);
                // Show stock hint
                medSelect.title = 'Available stock: ' + data.stock;
                calcLine(row);
            })
            .catch(() => alert('Could not fetch medicine info.'));
    });

    // On qty change: recalc
    qtyInput.addEventListener('input', () => calcLine(row));

    // Remove row
    removeBtn.addEventListener('click', function () {
        row.remove();
        recalcGrandTotal();
    });
}

function calcLine(row) {
    const rate = parseFloat(row.querySelector('.rate-input').value) || 0;
    const qty = parseInt(row.querySelector('.qty-input').value) || 0;
    const amt = rate * qty;
    const gst = parseFloat((amt * GST_RATE / 100).toFixed(2));
    const total = parseFloat((amt + gst).toFixed(2));

    row.querySelector('.amt-input').value = amt.toFixed(2);
    row.querySelector('.gst-input').value = gst.toFixed(2);
    row.querySelector('.total-input').value = total.toFixed(2);

    recalcGrandTotal();
}

function recalcGrandTotal() {
    let grand = 0;
    document.querySelectorAll('.total-input').forEach(el => {
        grand += parseFloat(el.value) || 0;
    });
    document.getElementById('grandTotalDisplay').textContent = grand.toFixed(2);
    document.getElementById('grandTotalHidden').value = grand.toFixed(2);
}

// Form submit validation
document.getElementById('saleForm').addEventListener('submit', function (e) {
    const rows = document.querySelectorAll('#detailsBody tr');
    if (rows.length === 0) {
        e.preventDefault();
        alert('Please add at least one medicine row before saving.');
        return;
    }

    let valid = true;
    rows.forEach(row => {
        const med = row.querySelector('.med-select').value;
        const qty = parseInt(row.querySelector('.qty-input').value) || 0;
        const rate = parseFloat(row.querySelector('.rate-input').value) || 0;
        if (!med || qty < 1 || rate <= 0) {
            valid = false;
        }
    });

    if (!valid) {
        e.preventDefault();
        alert('Each row must have a medicine selected, a valid rate, and quantity >= 1.');
    }
});

// Add first row automatically on page load
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('addRowBtn').addEventListener('click', addRow);
    addRow(); // start with one blank row
});
