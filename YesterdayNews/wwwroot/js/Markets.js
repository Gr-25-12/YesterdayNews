document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/stockHub")
        .build();

    connection.on("ReceiveStockUpdates", (stockQuotes) => {
        for (const symbol in stockQuotes) {
            const li = document.querySelector(`li[data-symbol='${symbol}']`);
            if (!li) continue;

            const quote = stockQuotes[symbol];
            console.log(quote);
            const priceSpan = li.querySelector(".price");
            const percentageSpan = li.querySelector(".percentage");

            if (priceSpan) priceSpan.textContent = quote.CurrentPrice.toFixed(2);
            if (percentageSpan) {
                percentageSpan.textContent = `(${quote.PercentageChange.toFixed(2)}%)`;
                percentageSpan.className = `percentage ${quote.PercentageChange >= 0 ? "up" : "down"}`;
            }
        }
    });

    connection.start().catch(err => console.error(err));
});
