// ===== Balut AJAX Core - عملیات بدون رفرش صفحه =====

function getAntiforgeryToken() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

async function ajaxGet(url) {
    const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
    if (!res.ok) throw new Error('خطا در ارتباط با سرور');
    return await res.json();
}

async function ajaxPost(url, data) {
    const res = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiforgeryToken()
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error('خطا در ارتباط با سرور');
    return await res.json();
}

// نمایش پیام موفقیت / خطا بدون رفرش
function showToast(message, success) {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'position-fixed bottom-0 start-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    const div = document.createElement('div');
    div.className = `alert ${success ? 'alert-success' : 'alert-danger'} shadow py-2 px-3`;
    div.textContent = message;
    container.appendChild(div);

    setTimeout(() => div.remove(), 4000);
}