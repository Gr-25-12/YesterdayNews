document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/stockHub")
        .build();

    connection.on("ReceivePriceUpdates", (priceData) => {
        for (const symbol in priceData) {
            if (!priceData.hasOwnProperty(symbol)) continue;
            const trade = priceData[symbol]; 
            const li = document.querySelector(`li[data-symbol='${symbol}']`);
            if (!li) continue;

            const priceSpan = li.querySelector(".price");
            const changeSpan = li.querySelector(".change");
            const percentageSpan = li.querySelector(".percentage");
            if (priceSpan) priceSpan.textContent = trade.CurrentPrice.toFixed(2);
            if (changeSpan) changeSpan.textContent = trade.Change.toFixed(2);
            if (percentageSpan) {
                percentageSpan.textContent = `(${trade.PercentageChange.toFixed(2)}%)`;
                percentageSpan.className = `percentage ${trade.PercentageChange >= 0 ? "up" : "down"}`;
            }
        }
    });

    connection.start().catch(err => console.error(err));
});
//for (const symbol in priceData) {
//    const li = document.querySelector(`li[data-symbol='${symbol}']`);
//    if (!li) continue;

//    const quote = priceData[symbol];
//    console.log(quote);
//    const priceSpan = li.querySelector(".price");
//    const percentageSpan = li.querySelector(".percentage");

//    if (priceSpan) priceSpan.textContent = quote.CurrentPrice.toFixed(2);
//    if (percentageSpan) {
//        percentageSpan.textContent = `(${quote.PercentageChange.toFixed(2)}%)`;
//        percentageSpan.className = `percentage ${quote.PercentageChange >= 0 ? "up" : "down"}`;
//    }
//}
