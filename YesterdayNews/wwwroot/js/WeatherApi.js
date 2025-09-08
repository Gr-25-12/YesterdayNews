(async function () {
    if (!navigator.geolocation) {
        console.log('Geolocation not supported');
        return;
    }

    const url = new URL(window.location.href);
    const latParam = url.searchParams.get('lat');
    const lonParam = url.searchParams.get('lon');

    try {
        const position = await new Promise((resolve, reject) =>
            navigator.geolocation.getCurrentPosition(resolve, reject)
        );

        const lat = position.coords.latitude.toFixed(2);
        const lon = position.coords.longitude.toFixed(2);

        // On main weather page without lat/lon -> redirect with lat/lon in URL
        if (window.location.pathname === '/Weather' && (!latParam || !lonParam)) {
            url.searchParams.set('lat', lat);
            url.searchParams.set('lon', lon);
            window.location.href = url.toString();
            return; // stop script after redirect
        }

        // On other pages or when lat/lon are present, update weather widget dynamically
        const widgetElement = document.getElementById('weather-widget');
        if (widgetElement) {
            const response = await fetch(`/Weather/component?lat=${lat}&lon=${lon}`);
            if (response.ok) {
                const html = await response.text();
                widgetElement.innerHTML = html;
            } else {
                console.warn('Failed to fetch weather component');
            }
        }
    } catch (error) {
        console.warn('Geolocation error:', error.message);
    }
})();
