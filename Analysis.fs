namespace Bascplain

module Analysis =

    open Matchers
    open Portfolio

    type PerformanceInfo = { Return : decimal; AccumulatedPerformance : decimal; } 
    type AnalysisItem = { Nav : decimal; PerformanceNext : PerformanceInfo; PerformanceNow : PerformanceInfo; Weight: decimal; }

    let analyzeSingleDaySingleMatcher fs (m, (n,a)) = 
        let fs' = fs |> Seq.filter (getPredicate m)

        let numerator = fs' |> Seq.sumBy (fun f -> f.Snapshot.TotalValue * f.ReturnOnNext)
        let nav = fs' |> Seq.sumBy (fun f -> f.Snapshot.TotalValue)
        let totalNav = fs |> Seq.sumBy (fun f -> f.Snapshot.TotalValue)
        let ret = if nav = 0.0m then 0.0m else numerator / nav

        let performance = { Return = ret; AccumulatedPerformance = (1.0m + a.PerformanceNext.AccumulatedPerformance) * (1.0m + ret) - 1.0m; }

        n,{ Nav = nav;  Weight = nav/totalNav; PerformanceNext = performance; PerformanceNow = a.PerformanceNext; }

    
    /// **Description**
    /// Performs the analysis on the given granular flow position time series on the groups created by the matchers.
    /// 
    /// **Parameters**
    ///   * `ms` - The matchers dividing the portfolio into (not necessarily disjunct) groups
    ///   * `gs` - Portfolio flow positions by instrument/finest granualarity by timepoints
    ///
    /// **Output Type**
    ///   * `(string * AnalysisItem) [] list` - A list representing the time series of analysis results together with the matcher name
    let analyzeOnMatches ms (gs : PortfolioFlowPosition [] []) =
        let emptyPerformance = { Return = 0.0m; AccumulatedPerformance = 0.0m; }
        let emptyAnalysis = { Nav = 0.0m; PerformanceNext = emptyPerformance; PerformanceNow = emptyPerformance; Weight = 0.0m; }

        //This is a part where using arrays over sequences/lists is good performance-wise
        //bs: Time points that have been already analyzed
        let foldAnalysis bs fs = 
            let az = match bs with
                        | b :: _ -> b //previous analysis time
                        | [] -> ms |> Array.map (fun p -> getName p,emptyAnalysis)
            
            let thisDayAnalyses = Array.zip ms az |> Array.map (analyzeSingleDaySingleMatcher fs)
            thisDayAnalyses :: bs
        gs |> Seq.fold foldAnalysis []

    let analyzeOnMatchesT ms ts = ts |> Array.map snd |> analyzeOnMatches ms |> Seq.rev |> Seq.zip (ts |> Seq.map fst)

    let analyzeIntoMatcherGroups ms gs =
        analyzeOnMatches ms gs |> Seq.collect id |> Seq.groupBy fst |> Seq.map (fun (n,ts) -> n, ts |> Seq.map snd)

    let analyzeIntoMatcherGroupsT ms gs =
        let groupByInnerFst xs = xs |> Seq.collect (fun (d,ts) -> ts|> Seq.map (fun t -> fst t, (snd t, d))) |> Seq.groupBy fst
        let groupBySndNicely xs = xs |> Seq.groupBy snd |> Seq.map (fun (k,is) -> k, is |> Seq.map fst)
        let takeFirstOfSecond (d,xs) = Seq.first xs |> Option.map (fun x -> d,x)
        analyzeOnMatchesT ms gs |> groupByInnerFst |> Seq.map (fun (n,us) -> n, us |> Seq.map snd |> groupBySndNicely |> Seq.choose takeFirstOfSecond)