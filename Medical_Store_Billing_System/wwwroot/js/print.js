// Print any report table
function printReport(title) {
    var printContents = document.getElementById('reportTable').outerHTML;
    var win = window.open('', '_blank');
    win.document.write(`
        <html><head><title>${title}</title>
        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css">
        <style>
            body { margin: 20px; font-size: 13px; }
            @media print { .no-print { display: none; } }
        </style>
        </head><body>
        <h4 class="mb-3">${title}</h4>
        ${printContents}
        <script>window.onload = function(){ window.print(); window.close(); }<\/script>
        </body></html>`);
    win.document.close();
}