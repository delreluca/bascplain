namespace Bascplain

module Loader =

    open Analysis
    open Cash
    open Portfolio
    open System.IO

    type FileResult = CashResult of CashInfo | PortfolioResult of PortfolioSnapshotPosition seq

    let getDateOfResult f = match f with
                            | CashResult c -> Some c.Date
                            | PortfolioResult p -> getDateOfSnapshot p

    let parseRkkFile (p : string) = rkk2CashInfo (RkkCsv.Load(p)).Rows

    let parseWdpFile (p : string) = wdp2Snapshot (WdpCsv.Load(p)).Rows

    let routePath p = match Path.GetFileName(p).Split [|'_';'.'|] with
                        | [|_;_;_;_;_;"RKK";_;"CSV"|] -> Some <| CashResult (parseRkkFile p)
                        | [|_;_;_;_;_;"WDP";_;"CSV"|] -> Some <| PortfolioResult (parseWdpFile p)
                        | _ -> None

    let loadDirectory d = Directory.GetFiles(d) |> Array.choose routePath

    let makeFlowsFromFileResults rs =
        let bindPortfolioResult r = match r with
                                    | PortfolioResult p -> p |> Seq.toArray
                                    | CashResult c -> [|cashInfo2Snapshot c|]

        rs |> Array.collect bindPortfolioResult |> Array.groupBy (fun p -> p.Isin)
            |> Array.map (fun (i,ps) -> i,makeFlowForSecurity <| Array.sortBy (fun p -> p.Date) ps)

    let regroupFlowsToTimeSeries gs =
        gs |> Seq.map snd |> Seq.collect id |> Seq.groupBy (fun f -> f.Snapshot.Date)

    let analyzeDirectory p d =
        loadDirectory d |> makeFlowsFromFileResults |> regroupFlowsToTimeSeries |> analyzeIntoMatcherGroupsT p