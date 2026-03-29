// Clipboard copy
window.copiarTexto = async function (texto) {
    try {
        await navigator.clipboard.writeText(texto);
        return true;
    } catch {
        return false;
    }
};

// Highlight.js: re-highlight after Blazor render
window.aplicarHighlight = function () {
    document.querySelectorAll('pre code.language-sql').forEach(el => {
        el.removeAttribute('data-highlighted');
        hljs.highlightElement(el);
    });
};
