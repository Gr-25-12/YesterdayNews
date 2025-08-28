document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/financeHub")
        .build();

    connection.on("ReceivePriceUpdates", (priceData) => {
        for (const symbol in priceData) {
            if (!priceData.hasOwnProperty(symbol)) continue;
            const trade = priceData[symbol]; 

            const row = document.querySelector(`tr[data-symbol='${symbol}']`);
            if (!row) continue;

            const priceCell = row.querySelector(".price");
            const changeCell = row.querySelector(".change");
            const percentageCell = row.querySelector(".percentage");
            if (priceCell) priceCell.textContent = trade.CurrentPrice.toFixed(2);
            if (changeCell) changeCell.textContent = `${trade.Change >= 0 ? "+" : ""}${trade.Change.toFixed(2)}`;
            if (percentageCell) {
                percentageCell.textContent = `(${trade.PercentageChange.toFixed(2)}%)`;
                percentageCell.className = `percentage ${trade.PercentageChange >= 0 ? "text-success" : "text-danger"}`;
            }
        }
       
    });

    connection.start().catch(err => console.error(err));
});
