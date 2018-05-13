namespace Bascplain

module Matchers =

    open Classification
    open Portfolio

    type Matcher = WholePortfolio
                    | MatchIsin of string
                    | MatchClass of AssetClass
                    | MatchRegion of Region
                    | MatchClassifier of Classifier
                    | NegateMatch of Matcher
                    | UnionMatch of Matcher * Matcher
                    | UnknownMatcher of string * string

    let rec getName m =
        match m with
            | WholePortfolio -> "portfolio"
            | MatchIsin i -> sprintf "isin:%s" i
            | MatchClass c -> sprintf "class:%A" c
            | MatchRegion r -> sprintf "region:%A" r
            | MatchClassifier c -> sprintf "class-region:%A.%s" c.What (c.Where |> Option.map (sprintf "%A") |> Option.defaultValue "*")
            | NegateMatch m' -> sprintf "!%s" (getName m')
            | UnionMatch (m',n) -> sprintf "%s+%s" (getName m') (getName n)
            | UnknownMatcher (e,i) -> sprintf "(!) '%s' invalid: %s" i e

    let rec getPredicate m =
        let classifyFlow f = classifyIsin f.Snapshot.Isin
        
        match m with
            | WholePortfolio -> (fun _ -> true)
            | MatchIsin i -> (fun f -> f.Snapshot.Isin.ToUpperInvariant() = i.ToUpperInvariant())
            | MatchClass c -> fun f -> (classifyFlow f) |> Option.map (fun l -> l.What = c) |> Option.defaultValue false
            | MatchRegion r -> fun f -> (classifyFlow f) |> Option.map (fun l -> l.Where |> Option.map (fun r' -> r' = r) |> Option.defaultValue false ) |> Option.defaultValue false
            | MatchClassifier l -> fun f -> classifyFlow f |> Option.map (fun l' -> l' = l) |> Option.defaultValue false
            | NegateMatch m' -> fun f -> not <| getPredicate m' f
            | UnionMatch (m',n) -> fun f -> getPredicate m' f || getPredicate n f
            | UnknownMatcher _ -> fun _ -> false

    let (<&>) m1 m2 =
        match m1 with
            | NegateMatch WholePortfolio -> m2
            | WholePortfolio -> WholePortfolio
            | _ -> match m2 with
                    | NegateMatch WholePortfolio -> m1
                    | WholePortfolio -> WholePortfolio
                    | _ -> UnionMatch (m1,m2)

    let parseMatchExpression (e : string) =
        let parsePositiveMatchExpression (e' : string) =
            match e'.Split [|':'|] with
                | [|"isin";i|] -> MatchIsin i
                | [|"portfolio"|] -> WholePortfolio
                | [|"region";r|] -> parseRegion r |> Option.map MatchRegion |> Option.defaultValue (UnknownMatcher (sprintf "'%s' is not a valid region" r,e'))
                | [|"class";c|] -> parseAssetClass c |> Option.map MatchClass |> Option.defaultValue (UnknownMatcher (sprintf "'%s' is not a valid asset class" c,e'))
                | [|"class-region";x|] -> match x.Split [|'.'|] with
                                            | [|c;r|] -> parseAssetClass c |> Option.map (fun c' -> MatchClassifier { What = c'; Where = parseRegion r; } ) |> Option.defaultValue (UnknownMatcher (sprintf "'%s' is not a valid asset class" c,e'))
                                            | _ -> UnknownMatcher (sprintf "'%s' is not a valid class/region combination, use format 'class.region'" x, e')
                | _ -> (UnknownMatcher (sprintf "'%s' is not a known matcher, use 'isin:<isin>', 'class:<class>', 'region:<region>', 'class-region:<class>.<region>' or 'portfolio'" e',e'))
        let parseStandaloneMatchExpression (e' : string) =
            if e'.StartsWith '!' then
                NegateMatch (parsePositiveMatchExpression <| e'.Substring 1)
            else
                parsePositiveMatchExpression e'

        e.Split [|'+'|] |> Seq.fold (fun m x -> m <&> parseStandaloneMatchExpression x) (NegateMatch WholePortfolio)