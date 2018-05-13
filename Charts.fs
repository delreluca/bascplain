namespace Bascplain

module Charts =

    open Analysis
    open System
    open XPlot.Plotly

    let getArbitraryTimeSeriesChart g (ys : (string * (DateTime * AnalysisItem) seq) seq) =
        let ls = ys |> Seq.map fst
        let mapSnd f us = us |> Seq.map (fun (x,y) -> x, f y)
        let ts = ys |> Seq.map (snd >> mapSnd g) |> Chart.Line |> Chart.WithLabels ls

        ts.GetInlineHtml()

    let getNavChart ys = getArbitraryTimeSeriesChart (fun a -> a.Nav) ys

    let getWeightChart ys = getArbitraryTimeSeriesChart (fun a -> a.Weight * 100m) ys

    let getDailyReturnChart ys = getArbitraryTimeSeriesChart (fun a -> a.PerformanceNow.Return * 100m) ys

    let getPerformanceChart ys = getArbitraryTimeSeriesChart (fun a -> a.PerformanceNow.AccumulatedPerformance * 100m) ys