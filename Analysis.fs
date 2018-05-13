namespace Bascplain

module Analysis =

    open Classification
    open Portfolio

    type PerformanceInfo = { Return : decimal; AccumulatedPerformance : decimal; } 
    type AnalysisItem = { Nav : decimal; PerformanceNext : PerformanceInfo; PerformanceNow : PerformanceInfo; Weight: decimal; }
    type Matcher = { Name : string; Predicate: PortfolioFlowPosition -> bool; }

    let (<&>) m1 m2 =
        {
            Name = sprintf "%s+%s" m1.Name m2.Name;
            Predicate = (fun f -> m1.Predicate f || m2.Predicate f);
        }

    let matchAll =
        {
            Name = "portfolio";
            Predicate = (fun _ -> true);
        }

    let matchIsin i =
        {
            Name = sprintf "isin:%s" i;
            Predicate = (fun f -> f.Snapshot.Isin.ToUpperInvariant() = i.ToUpperInvariant());
        }

    let matchClassifier c =
        {
            Name = sprintf "cr:%A.%s" c.What (c.Where |> Option.map (sprintf "%A") |> Option.defaultValue "*");
            Predicate = (fun f -> classifyIsin f.Snapshot.Isin |> Option.map (fun d -> d = c) |> Option.defaultValue false);
        }

    let matchClassifierWhat a =
        {
            Name = sprintf "class:%A" a;
            Predicate = (fun f -> classifyIsin f.Snapshot.Isin |> Option.map (fun c -> c.What = a) |> Option.defaultValue false);
        }

    let analyzeSingleDaySingleMatcher fs (m, (n,a)) = 
        let fs' = Seq.filter m.Predicate fs

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
                        | [] -> ms |> Seq.map (fun p -> p.Name,emptyAnalysis)
            
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