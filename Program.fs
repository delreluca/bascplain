namespace Bascplain

module Program =

    open Suave
    open Suave.DotLiquid
    open Suave.Filters
    open Suave.Operators

    let webApp = choose [
                    path "/" >=> page "home.htm" null
                        ]

    [<EntryPoint>]
    let main _ =
        setTemplatesDir "./liquid"
        printfn "Hello World from F#!"
        //analyzeDirectory [matchClassifierWhat Equity <&> matchClassifierWhat Commodities; matchAll]
        startWebServer defaultConfig webApp
        0 // return an integer exit code
