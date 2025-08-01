let createdInput;
let expiresText;

$(document).ready(function () {
    createdInput = document.getElementById("createdDate");
    expiresText = document.getElementById("expiresText");

    if (createdInput && expiresText) {
        createdInput.addEventListener("change", updateExpiresText);
        window.addEventListener("load", updateExpiresText);
        updateExpiresText()
    }

    userSelect();
    setupPriceDisplay();

});

function userSelect() {
    const $userSelect = $('#UserId');
    if ($userSelect.length === 0) return;

    console.log("UserId select found:", $('#UserId').length);
    $userSelect.select2({
        placeholder: "Search for a user",
        minimumInputLength: 2,
        ajax: {
            url: '/Subscription/Search',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { searchTerm: params.term };
            },
            processResults: function (data) {
                return {
                    results: data.map(user => ({
                        id: user.id,
                        text: user.name
                    }))
                };
            },
            cache: true
        }
    });
}


function updateExpiresText() {

    const date = new Date(createdInput.value);
    if (!isNaN(date)) {
        date.setFullYear(date.getFullYear() + 1);
        const expiresValue = date.toISOString().split('T')[0]; 
        expiresText.value = expiresValue;
        document.getElementById('Expires').value = expiresValue;
    } else {
        expiresText.value = "";
        document.getElementById('Expires').value = "";
    }
}

function setupPriceDisplay() {
    const subscriptionTypeSelect = document.getElementById("SubscriptionTypeId");
    const priceRow = document.getElementById("subscriptionPriceRow");
    const planSpan = document.getElementById("planText");
    const priceSpan = document.getElementById("priceText");
    const accessSpan = document.getElementById("accessLevelText");

    if (!subscriptionTypeSelect || !priceRow || !planSpan || !priceSpan || !accessSpan) return;

    subscriptionTypeSelect.addEventListener("change", function () {
        const selectedOption = subscriptionTypeSelect.options[subscriptionTypeSelect.selectedIndex];
        const plan = selectedOption.getAttribute("data-plan");
        const price = selectedOption.getAttribute("data-price");
        const access = selectedOption.getAttribute("data-accesslevel");

        if (plan && price && access) {
            planSpan.textContent = plan;
            priceSpan.textContent = price + " kr";
            accessSpan.textContent = access;

            priceRow.style.display = "block";
        } else {
            priceRow.style.display = "none";
        }
    });
}