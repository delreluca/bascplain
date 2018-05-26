namespace Bascplain

module Program =

    open Analysis
    open Charts
    open Matchers
    open Loader

    open System

    open Suave
    open Suave.DotLiquid
    open Suave.Filters
    open Suave.Operators

    type LineChartModel = { Property: string; Expressions: string[]; ChartHtml: string; }

    let mkAnalysisEngine rs es =
        let ms = es |> Array.map parseMatchExpression
        analyzeIntoMatcherGroupsT ms rs

    let mkLineChartPage p k z (x : string) =
        let mkPageWithExpressions es = page "lines.htm" { Property = p; Expressions = es; ChartHtml = k <| z es; }

        let mkPageFromForm (req:HttpRequest) =
            let getNthFormData n = match req.formData(sprintf "expression%d" n) with
                                    | Choice2Of2 _ -> None
                                    | Choice1Of2 a -> Some a

            let es = Seq.initInfinite id |> Seq.map getNthFormData |> Seq.takeWhile Option.isSome |> Seq.choose id |> Seq.toArray

            mkPageWithExpressions <| es

        choose [
                GET >=> (mkPageWithExpressions <| x.Split[|';'|])
                POST >=> request mkPageFromForm
        ]

    let webApp z =
        choose [
            path "/" >=> page "home.htm" null
            pathScan "/nav/%s" <| mkLineChartPage "NAV" getNavChart z
            pathScan "/weight/%s" <| mkLineChartPage "Weight" getWeightChart z
            pathScan "/return/%s" <| mkLineChartPage "Daily returns" getDailyReturnChart z
            pathScan "/performance/%s" <| mkLineChartPage "Total performance" getPerformanceChart z
        ]

    [<EntryPoint>]
    let main argv =
        let d = match argv with
                | [|d'|] -> d'
                | _ -> printf "Enter CSV directory: "; Console.ReadLine()
        setTemplatesDir "./liquid"
        let r =  loadDirectoryForAnalysis d
        startWebServer defaultConfig (webApp (mkAnalysisEngine r))
        0 // return an integer exit code
