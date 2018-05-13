namespace Bascplain

module Analysis =

    open Matchers
    open Portfolio

    type PerformanceInfo = { Return : decimal; AccumulatedPerformance : decimal; } 
    type AnalysisItem = { Nav : decimal; PerformanceNext : PerformanceInfo; PerformanceNow : PerformanceInfo; Weight: decimal; }

    let analyzeSingleDaySingleMatcher fs (m, (n,a)) = 
        let fs' = fs |> Seq.filter (getPredicate m)

        let nominator = fs' |> Seq.sumBy (fun f -> f.Snapshot.TotalValue * f.ReturnOnNext)
        let nav = fs' |> Seq.sumBy (fun f -> f.Snapshot.TotalValue)
        let totalNav = fs |> Seq.sumBy (fun f -> f.Snapshot.TotalValue)
        let ret = nominator / nav

        let performance = { Return = ret; AccumulatedPerformance = (1.0m + a.PerformanceNext.AccumulatedPerformance) * (1.0m + ret) - 1.0m; }

        n,{ Nav = nav;  Weight = nav/totalNav; PerformanceNext = performance; PerformanceNow = a.PerformanceNext; }

    let analyzeOnMatches ms (gs : PortfolioFlowPosition seq seq) =
        let emptyPerformance = { Return = 0.0m; AccumulatedPerformance = 0.0m; }
        let emptyAnalysis = { Nav = 0.0m; PerformanceNext = emptyPerformance; PerformanceNow = emptyPerformance; Weight = 0.0m; }

        let foldAnalysis bs fs = 
            let az = match bs with
                        | b :: _ -> b
                        | [] -> ms |> Seq.map (fun p -> getName p,emptyAnalysis)
            
            let thisDayAnalyses = Seq.zip ms az |> Seq.map (analyzeSingleDaySingleMatcher fs)
            thisDayAnalyses :: bs
        gs |> Seq.fold foldAnalysis []

    let analyzeOnMatchesT ms ts = ts |> Seq.map snd |> analyzeOnMatches ms |> Seq.rev |> Seq.zip (ts |> Seq.map fst)

    let analyzeIntoMatcherGroups ms gs =
        analyzeOnMatches ms gs |> Seq.collect id |> Seq.groupBy fst |> Seq.map (fun (n,ts) -> n, ts |> Seq.map snd)

    let analyzeIntoMatcherGroupsT ms gs =
        let groupByInnerFst xs = xs |> Seq.collect (fun (d,ts) -> ts|> Seq.map (fun t -> fst t, (snd t, d))) |> Seq.groupBy fst
        let groupBySndNicely xs = xs |> Seq.groupBy snd |> Seq.map (fun (k,is) -> k, is |> Seq.map fst)
        analyzeOnMatchesT ms gs |> groupByInnerFst |> Seq.map (fun (n,us) -> n, us |> Seq.map snd |> groupBySndNicely)