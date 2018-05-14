namespace Bascplain

module Portfolio =
    open Cash
    open FSharp.Data
    open System

    type WdpCsv = CsvProvider<"./csv-samples/wdp.CSV", ";", Culture = "de", Schema = "XXX-DATUM=string">

    type PortfolioSnapshotPosition = { Isin: string; Quantity: decimal; Quote: decimal; TotalValue: decimal; Date: DateTime; }
    type PortfolioFlowPosition = { Snapshot: PortfolioSnapshotPosition; ReturnOnNext: decimal; }

    let getDateOfSnapshot p = Option.map (fun s -> s.Date) (Seq.first p)

    let wdp2Snapshot ws =
        let makePos (w : WdpCsv.Row) =  {   
                                            Isin = w.``XXX-WPNR``;
                                            Quantity = w.``XXX-NW-M``;
                                            Quote = w.``XXX-AKTKURS-M``;
                                            TotalValue = w.``XXX-KW-M``;
                                            Date = parseDate w.``XXX-DATUM``
                                        }
        ws |> Seq.map makePos |> Seq.filter (fun s -> s.Quantity > 0.0m) |> Seq.groupBy (fun s -> s.Isin) |> Seq.choose (Seq.first << snd)

    let cashInfo2Snapshot c = { Isin = "<CASH>"; Quantity = c.Balance; Quote = 1.0m; TotalValue = c.Balance; Date = c.Date; }

    let makeFlowForSecurity (xs : PortfolioSnapshotPosition seq) =
        let foldReturns fs (s) = match fs with
                                  | f :: fs' -> { Snapshot = s; ReturnOnNext = 0.0m; } :: { f with ReturnOnNext = s.Quote / f.Snapshot.Quote - 1.0m } :: fs'
                                  | [] -> [{Snapshot = s; ReturnOnNext = 0.0m }]
        xs |> Seq.fold foldReturns [] |> Seq.rev