// Clipboard copy
window.copiarTexto = async function (texto) {
    try {
        await navigator.clipboard.writeText(texto);
        return true;
    } catch {
        return false;
    }
};

// Download file from base64
window.downloadArquivo = function (nomeArquivo, base64) {
    var link = document.createElement('a');
    link.href = 'data:application/octet-stream;base64,' + base64;
    link.download = nomeArquivo;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Highlight.js: re-highlight after Blazor render
window.aplicarHighlight = function () {
    document.querySelectorAll('pre code.language-sql').forEach(el => {
        el.removeAttribute('data-highlighted');
        hljs.highlightElement(el);
    });
};
