namespace StockMarketGame

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let inputBuy = Var.Create 0
    let inputSell = Var.Create 0

    let money = Var.Create 100000
    let selectedStock = Var.Create "NASDAQ"
    let stocksList =
        ListModel.Create fst
            [
                ("NASDAQ", 0)
                ("RUSSEL", 0)
                ("IBEX", 0)
                ("NIKKEI", 0)
            ]

    let stocksPrice =
        ListModel.Create fst
            [
                ("NASDAQ", 16300)
                ("RUSSEL", 1890)
                ("IBEX", 13000)
                ("NIKKEI", 34800)
            ]

    let lastDayStocksPrice =
        ListModel.Create fst
            [
                ("NASDAQ", 16300)
                ("RUSSEL", 1890)
                ("IBEX", 13000)
                ("NIKKEI", 34800)
            ]

    [<SPAEntryPoint>]
    let Game () =
        let rnd = System.Random();

        IndexTemplate.Game()
            .StockPriceContainer(
                stocksPrice.View.DocSeqCached(fun (name : string, price : int) ->
                    IndexTemplate.StockPriceItem()
                        .StockName(name)
                        .StockPrice(price.ToString())
                        .PricePercentage((snd (lastDayStocksPrice.Find(fun (x, _) -> x = name))).ToString())
                        .Doc()
                )
            )
            .MoneyOwned(money.Value.ToString())
            .StockContainer(
                stocksList.View.DocSeqCached(fun (name : string, value : int) ->
                    IndexTemplate.StockItem()
                        .StockName(name)
                        .StockAmount(value.ToString())
                        .Doc()
                )
            )
            .NextDayButton(fun _ ->
                let posOrNeg = rnd.Next(0, 2) * 2 - 1
                
                let arrayValues =
                    [|
                        snd (stocksPrice.Find(fun (x, _) -> x = "NASDAQ"))
                        snd (stocksPrice.Find(fun (x, _) -> x = "RUSSEL"))
                        snd (stocksPrice.Find(fun (x, _) -> x = "IBEX"))
                        snd (stocksPrice.Find(fun (x, _) -> x = "NIKKEI"))
                    |]

                lastDayStocksPrice.Clear()
                lastDayStocksPrice.AppendMany(stocksPrice)

                stocksPrice.Clear()
                stocksPrice.AppendMany(
                    [
                        ("NASDAQ", arrayValues[0] + (posOrNeg * rnd.Next(0, 1000)))
                        ("RUSSEL", arrayValues[1] + (posOrNeg * rnd.Next(0, 300)))
                        ("IBEX", arrayValues[2] + (posOrNeg * rnd.Next(0, 600)))
                        ("NIKKEI", arrayValues[3] + (posOrNeg * rnd.Next(0, 1500)))
                    ]
                )
            )

            .SelectedStock(selectedStock)
            .InputBuy(inputBuy)
            .InputSell(inputSell)

            .BuyButton(fun t ->
                let amount = snd (stocksList.Find(fun (x, _) -> x = selectedStock.Value))
                let price = snd (stocksPrice.Find(fun (x, _) -> x = selectedStock.Value))

                let a = money.Value
                let b = inputBuy.Value
                let c = b * price
                
                match b with
                | b when b >= 0 ->
                    match (a, c) with
                    | (a, c) when a >= c -> 
                        money.Value <- (a - c)
                        stocksList.Add((selectedStock.Value, amount + inputBuy.Value))
                    | _ -> money.Value <- a
                | _ -> money.Value <- a

                t.Vars.MoneyOwned.Value <- money.Value.ToString()

                inputBuy.Value <- 0
            )
            .SellButton(fun t ->
                let amount = snd (stocksList.Find(fun (x, _) -> x = selectedStock.Value))
                let price = snd (stocksPrice.Find(fun (x, _) -> x = selectedStock.Value))

                let a = amount
                let b = inputSell.Value
                match b with
                | b when b >= 0 ->
                    match (a, b) with
                    | (a, b) when a >= b ->
                        money.Value <- money.Value + (b * price)
                        stocksList.Add((selectedStock.Value, amount - inputSell.Value))
                    | _ -> 
                        stocksList.Add((selectedStock.Value, amount))
                | _ -> stocksList.Add((selectedStock.Value, amount))

                t.Vars.MoneyOwned.Value <- money.Value.ToString()

                inputSell.Value <- 0
            )

            .Doc()       
        |> Doc.RunById "game"