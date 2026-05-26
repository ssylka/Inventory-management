import { marked } from "https://cdn.jsdelivr.net/npm/marked/lib/marked.esm.js";
import DOMPurify from "https://cdn.jsdelivr.net/npm/dompurify@3/+esm";
document.querySelectorAll(".markup").forEach(t => {
    t.innerHTML = DOMPurify.sanitize(marked.parse(t.textContent.trim()));
});