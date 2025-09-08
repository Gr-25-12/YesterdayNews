// Cookie Consent Banner
document.addEventListener("DOMContentLoaded", function () {
    const banner = document.getElementById("cookieConsent");
    const acceptBtn = document.getElementById("acceptCookies");
    const necessaryBtn = document.getElementById("necessaryCookies");

    // Check if consent is already given  // Show banner only if no consent is saved
    if (!localStorage.getItem("cookieConsent")) {
        setTimeout(() => {
            banner.classList.add("show");
        }, 500);
    }

    // Accept all cookies
    acceptBtn.addEventListener("click", function () {
        localStorage.setItem("cookieConsent", "all");
        banner.classList.remove("show");
    });

    // Only necessary cookies
    necessaryBtn.addEventListener("click", function () {
        localStorage.setItem("cookieConsent", "necessary");
        banner.classList.remove("show");
    });
});
