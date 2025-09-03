﻿(async function () {
    //if (!window.location.pathname.startsWith('/weather')) return;
    // Run geolocation on all pages since this is a sidebar component

    const urlParams = new URLSearchParams(window.location.search);
    const currentCityInUrl = urlParams.get('city');
    const normalize = str => str.trim().toLowerCase();

    
    
    if (!navigator.geolocation) {
        console.log('Geolocation not supported');
        return;
    }

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
       

        // Always update widget with geolocation data
        const widgetElement = document.getElementById("weather-widget");
        if (widgetElement) {
            const widgetResp = await fetch(`/Weather/component?lat=${lat}&lon=${lon}`);
            const html = await widgetResp.text();
            widgetElement.innerHTML = html;
        }
        
        // On weather page, also redirect to show full forecast for detected city
        if (window.location.pathname.startsWith('/weather') && detectedCity && 
            normalize(detectedCity) !== normalize(currentCityInUrl || '') && !reloadFlag) {
            // Clean the city name before using it in URL
            const cleanCityName = detectedCity.replace(/ Municipality$/i, '').replace(/ Kommune$/i, '').replace(/ Kommun$/i, '').trim();
            console.log('Redirecting to:', `/weather?city=${encodeURIComponent(cleanCityName)}`);
            sessionStorage.setItem('weatherReloaded', 'true');
            window.location.href = `/weather?city=${encodeURIComponent(cleanCityName)}`;
        }
    } catch (error) {
        console.debug('Geolocation error or denied:', error);
    }
})();
