document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/financeHub")
        .build();

    function updateStockDescription(symbol, description) {
        const markets = document.querySelector("#marketTable");
        if (!markets) return;

        const row = document.querySelector(`tr[data-symbol='${symbol}']`);
        if (row) {
            const descCell = row.querySelector(".description");
            if (descCell) {
                descCell.textContent = description || "(Missing data)";
            }
        }
    }

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
                }
                else {
                    priceCell.textContent = " (Error)";
                    priceCell.className = "text-danger";
                }

            }
            if (changeCell) {
                if (trade.ClosingPrice != 0) {
                    changeCell.textContent = `${trade.Change >= 0 ? "+" : ""}${trade.Change.toFixed(2)}`;
                }
                else {
                    changeCell.textContent = " (Error)";
                    changeCell.className = "text-danger";
                }
            } 
            if (percentageCell) {
                if (trade.ClosingPrice != 0) {
                    percentageCell.textContent = `(${trade.PercentageChange.toFixed(2)}%)`;
                    percentageCell.className = `percentage ${trade.PercentageChange >= 0 ? "text-success" : "text-danger"}`;
                }
                else {
                    percentageCell.textContent = " (Error)";
                    percentageCell.className = "text-danger";
                }
            }
        }

    });

    connection.on("NoMarketStatus", (error) => {
        console.warn("MarketStatus API error:", error);

        const statusBars = document.querySelectorAll(".status-bar");
        statusBars.forEach(bar => {
            bar.textContent = "(Missing data)";
            bar.className = "text-danger";
        });

    });

    connection.start().catch(err => console.error(err));
});
