// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// --- Debounce helper ---
function debounce(fn, ms) { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; }

// --- Nav search ---
const $q = document.getElementById('navSearch');
const $sg = document.getElementById('navSuggest');

function goSearch() {
    if (!$q || !$q.value.trim()) return false;
    window.location = `/Movies/Search?q=${encodeURIComponent($q.value.trim())}`;
    return false;
}

async function fetchSuggest(q) {
    if (!q || q.length < 2) { $sg.classList.add('d-none'); $sg.innerHTML = ''; return; }
    const res = await fetch(`/api/search/suggest?q=${encodeURIComponent(q)}`);
    if (!res.ok) return;
    const data = await res.json(); // [{id,title,poster}]
    if (!Array.isArray(data) || !data.length) { $sg.classList.add('d-none'); $sg.innerHTML = ''; return; }
    $sg.innerHTML = data.map(x => `
    <a class="list-group-item list-group-item-action d-flex align-items-center bg-dark text-light"
       href="/Movies/Details/${x.id}">
       <img src="${x.poster || ''}" alt="" width="32" height="48" class="me-2 flex-shrink-0" onerror="this.style.display='none'">
       <span>${x.title}</span>
    </a>`).join('');
    $sg.classList.remove('d-none');
}

if ($q) {
    const onType = debounce(e => fetchSuggest(e.target.value.trim()), 250);
    $q.addEventListener('input', onType);
    document.addEventListener('click', (e) => {
        if (!$sg.contains(e.target) && e.target !== $q) { $sg.classList.add('d-none'); }
    });
}
