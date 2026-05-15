function toggleTheme() {
    const html = document.documentElement;
    const next = html.dataset.bsTheme == 'dark' ? 'light' : 'dark';
    html.dataset.bsTheme = next;
    document.cookie = `theme=${next};path=/;max-age=31536000`;
    document.getElementById('theme-btn').textContent = next === 'dark' ? '☀️' : '🌙';
}

const savedTheme = document.cookie.split(';').find(c => c.trim().startsWith('theme='));
if (savedTheme?.includes('dark')) {
    document.getElementById('theme-btn').textContent = '☀️';
}