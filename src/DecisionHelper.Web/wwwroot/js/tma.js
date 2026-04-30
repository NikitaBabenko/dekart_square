(function () {
  if (!window.Telegram || !window.Telegram.WebApp) return;
  const tg = window.Telegram.WebApp;
  if (!tg.initData) return;

  fetch('/api/auth/tma', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'same-origin',
    body: JSON.stringify({ initData: tg.initData })
  }).then(function (res) {
    if (res.ok && !sessionStorage.getItem('dh_tma_synced')) {
      sessionStorage.setItem('dh_tma_synced', '1');
      window.location.reload();
    }
  });

  try { tg.ready(); tg.expand(); } catch (e) {}
})();
