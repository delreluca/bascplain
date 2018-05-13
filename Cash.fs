namespace Bascplain

module Cash =
    open FSharp.Data
    open System
    open System.Globalization

    type RkkCsv = CsvProvider<"./csv-samples/rkk.CSV", ";", Culture = "de", Schema = "XXX-BUDAT=string">

    type DividendInfo = { Isin : string; Quantity : int; Value : decimal; }
    type CashInfo = { Balance : decimal; Deposit : decimal; Fees : decimal; Dividends : DividendInfo List; Date : DateTime; }

    let parseDate s = DateTime.ParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture)

    let isPendingTransaction (r : RkkCsv.Row) = r.``XXX-BUDAT`` = "00000000"

    let foldDividend c (r : RkkCsv.Row) =
        match r.``XXX-TEXT2``.Split [|' '|] with
        | [|"ISIN";isin|] -> match r.``XXX-TEXT3``.Split [|' '|] |> Array.filter (fun s -> s.Length > 0) with
                                | [|"STK";qty|] ->{ c with Dividends = { Isin = isin; Quantity = int qty; Value = r.``XXX-SALDO`` } :: c.Dividends }
                                | _ -> c //no match on "STK <qty>" text field
        | _ -> c //no match on "ISIN <isin>" text field
              

    let foldRkk (c : CashInfo) (r : RkkCsv.Row) = 
        match r.``XXX-UMARTSCHL`` with
              | "MBCP" -> foldDividend c r
              | "LKCI"
              | "TKPA" -> { c with Fees = c.Fees - r.``XXX-SALDO`` }
              | "LKCF" -> { c with Deposit = c.Deposit + r.``XXX-SALDO`` }
              | "SLDO" -> { c with Balance = r.``XXX-SALDO``; Date = parseDate r.``XXX-BUDAT`` }
              | _ -> c

    let rkk2CashInfo (rs : RkkCsv.Row seq) =
        rs |> Seq.filter (not << isPendingTransaction) |> Seq.fold foldRkk { Balance = 0.0m; Deposit = 0.0m; Fees = 0.0m; Dividends = []; Date = DateTime.Today }