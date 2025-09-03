(async function () {
    //if (!window.location.pathname.startsWith('/weather')) return;
    const path = window.location.pathname;
    if ((path === '/' || path.startsWith('/weather'))) return;

    const urlParams = new URLSearchParams(window.location.search);
    const currentCityInUrl = urlParams.get('city');
    const normalize = str => str.trim().toLowerCase();

    if (!navigator.geolocation) return;

    try {
        const position = await new Promise((res, rej) =>
            navigator.geolocation.getCurrentPosition(res, rej, { timeout: 5000 })
        );

        const lat = position.coords.latitude.toFixed(6);
        const lon = position.coords.longitude.toFixed(6);

        const resp = await fetch(`/api/weather/current?lat=${lat}&lon=${lon}`);
        if (!resp.ok) return;

        const data = await resp.json();

        const detectedCity = data.city;

        const reloadFlag = sessionStorage.getItem('weatherReloaded');

        if (detectedCity && normalize(detectedCity) !== normalize(currentCityInUrl || '') && !reloadFlag) {
            sessionStorage.setItem('weatherReloaded', 'true');
            window.location.href = `/weather?city=${encodeURIComponent(detectedCity)}`;
        } else {
            // Update widget without reload
            const widgetResp = await fetch(`/Weather/component?lat=${lat}&lon=${lon}`);
            const html = await widgetResp.text();
            document.getElementById("weather-widget").innerHTML = html;
        }
    } catch (error) {
        console.debug('Geolocation error or denied:', error);
    }
})();
