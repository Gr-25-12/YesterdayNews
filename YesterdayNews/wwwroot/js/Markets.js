document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/financeHub")
        .build();

    connection.on("updateDescription", (symbol, description) => {
        const markets = document.querySelector("#marketTable");
        if (!markets) return;

        const row = document.querySelector(`tr[data-symbol='${symbol}']`);
        if (row) {
            const descCell = row.querySelector(".description");
            if (descCell) {
                if (description) {
                    descCell.textContent = description;
                    descCell.classList.remove("text-danger");
                } 
            }
        }
    });

    //sends realtime data from websocket to update the elements in the view
    connection.on("ReceivePriceUpdates", (priceData) => {
        for (const symbol in priceData) {
            if (!priceData.hasOwnProperty(symbol)) continue;
            const trade = priceData[symbol];

            const row = document.querySelector(`tr[data-symbol='${symbol}']`);
            if (!row) continue;

            const priceCell = row.querySelector(".price");
            const changeCell = row.querySelector(".change");
            const percentageCell = row.querySelector(".percentage");
            if (priceCell) {
                if (trade != null && trade.CurrentPrice != null && trade.CurrentPrice != 0) {
                    priceCell.textContent = trade.CurrentPrice.toFixed(2);
                    priceCell.classList.remove("text-danger");
                }
                else {
                    priceCell.textContent = "(Error)";
                    priceCell.classList.add("text-danger");
                }

            }
            if (changeCell) {
                if (trade.ClosingPrice != 0) {
                    changeCell.textContent = `${trade.Change >= 0 ? "+" : ""}${trade.Change.toFixed(2)}`;
                    changeCell.className = `change ${ trade.Change >= 0 ? "text-success" : "text-danger" }`;
                }
                else {
                    changeCell.textContent = "(Error)";
                    changeCell.className = "change text-danger";
                }
            } 
            if (percentageCell) {
                if (trade.ClosingPrice != 0) {
                    percentageCell.textContent = `${trade.PercentageChange.toFixed(2)}%`;
                    percentageCell.className = `percentage ${trade.PercentageChange >= 0 ? "text-success" : "text-danger"}`;
                }
                else {
                    percentageCell.textContent = "(Error)";
                    percentageCell.className = "percentage text-danger";
                }
            }
        }

    });

    connection.on("NoMarketStatus", (error) => {
        console.warn("MarketStatus API error:", error);

        const statusBars = document.querySelectorAll(".status-bar");
        statusBars.forEach(bar => {
            bar.textContent = "(Missing data)";
            bar.classList.add("text-danger");
        });

    });

    connection.start().catch(err => console.error(err));
});
