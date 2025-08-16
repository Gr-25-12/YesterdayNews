function fetchWeather(lat, lon) {
    fetch(`/api/weather?lat=${lat}&lon=${lon}`)
        .then(res => res.json())
        .then(data => renderWeatherData(data))
        .catch(err => {
            console.error("Error:", err);
            fetchDefaultWeather(); // e.g., for Stockholm
        });
}

function fetchDefaultWeather() {
    fetch(`/api/weather?city=Stockholm`)
        .then(res => res.json())
        .then(data => renderWeatherData(data))
        .catch(err => {
            document.getElementById("weather-output").innerText = "Unable to fetch weather.";
        });
}

function renderWeatherData(data) {
    if (!data || data.length === 0) {
        document.getElementById("weather-output").innerText = "No weather data.";
        return;
    }

    // Render JSON data (loop through weather items etc.)
}
