import * as pdfjsLib from 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/4.9.155/pdf.min.mjs';

let pdfDoc = null;
let pageNum = 1;
let pageRendering = false;
let pageNumPending = null;
let currentScale = 1.0; // Default scale

function renderPdfInDiv(container, pdfData) {
    // Clear any existing content in the container
    container.innerHTML = "";

    // Decode the base64 PDF data
    const pdfBinary = atob(pdfData);

    // Initialize PDF.js
    const loadingTask = pdfjsLib.getDocument({ data: pdfBinary });

    loadingTask.promise.then(pdf => {
        pdfDoc = pdf;
        document.getElementById('page_count').textContent = pdfDoc.numPages;

        // Render the first page
        renderPage(pageNum, container);
    }).catch(error => {
        console.error("Error rendering PDF:", error);
        container.innerHTML = "<p>Failed to load PDF.</p>";
    });
}

function renderPage(num, container) {
    pageRendering = true;

    // Get the page
    pdfDoc.getPage(num).then(page => {
        const canvas = document.createElement("canvas");
        const context = canvas.getContext("2d");
        const viewport = page.getViewport({ scale: currentScale });

        canvas.width = viewport.width;
        canvas.height = viewport.height;

        container.innerHTML = ""; // Clear previous content
        container.appendChild(canvas);

        const renderContext = {
            canvasContext: context,
            viewport: viewport
        };

        const renderTask = page.render(renderContext);

        renderTask.promise.then(() => {
            pageRendering = false;

            if (pageNumPending !== null) {
                renderPage(pageNumPending, container);
                pageNumPending = null;
            }
        });
    });
}

function queueRenderPage(num, container) {
    if (pageRendering) {
        pageNumPending = num;
    } else {
        renderPage(num, container);
    }
}

function onPrevPage(container) {
    if (pageNum <= 1) {
        return;
    }
    pageNum--;
    queueRenderPage(pageNum, container);
}

function onNextPage(container) {
    if (pageNum >= pdfDoc.numPages) {
        return;
    }
    pageNum++;
    queueRenderPage(pageNum, container);
}

function onZoomIn(container) {
    currentScale += 0.2;
    renderPage(pageNum, container);
}

function onZoomOut(container) {
    if (currentScale > 0.4) {
        currentScale -= 0.2;
        renderPage(pageNum, container);
    }
}

function onPrintPdf() {
    if (!pdfDoc) {
        console.error("PDF document is not loaded.");
        return;
    }

    const printContainer = document.createElement("div");
    document.body.appendChild(printContainer);

    pdfDoc.getPage(pageNum).then(page => {
        const canvas = document.createElement("canvas");
        const context = canvas.getContext("2d");
        const viewport = page.getViewport({ scale: 1.5 });

        canvas.width = viewport.width;
        canvas.height = viewport.height;

        const renderContext = {
            canvasContext: context,
            viewport: viewport
        };

        page.render(renderContext).promise.then(() => {
            const img = document.createElement("img");
            img.src = canvas.toDataURL();
            printContainer.appendChild(img);

            const printWindow = window.open("", "_blank");
            if (printWindow) {
                // Use a delay to ensure the print dialog works consistently in Chromium browsers
                printWindow.document.open();
                printWindow.document.write(`
                    <html>
                        <head>
                            <title>Print PDF</title>
                            <style>
                                body { margin: 0; display: flex; justify-content: center; align-items: center; height: 100vh; }
                                img { max-width: 100%; max-height: 100%; }
                            </style>
                        </head>
                        <body>${printContainer.innerHTML}</body>
                    </html>
                `);
                printWindow.document.close();

                // Wait for the content to fully load before triggering the print dialog
                setTimeout(() => {
                    printWindow.focus();
                    printWindow.print();
                    printWindow.close();

                    // Cleanup after printing
                    document.body.removeChild(printContainer);
                }, 500); // Adjust the delay if needed
            } else {
                console.error("Failed to open a new window. Please check your browser settings.");
            }
        });
    }).catch(error => {
        console.error("Error while rendering page for printing:", error);
    });
}

// Expose the functions to the global scope
window.renderPdfInDiv = renderPdfInDiv;
window.onPrevPage = onPrevPage;
window.onNextPage = onNextPage;
window.onZoomIn = onZoomIn;
window.onZoomOut = onZoomOut;
window.onPrintPdf = onPrintPdf;

window.pdfDoc = pdfDoc;
window.pageNum = pageNum;
window.pageRendering = pageRendering;
window.pageNumPending = pageNumPending;
window.currentScale = currentScale;