namespace Bascplain

module Classification =

    type Region = Europe
                | Germany
                | USA
                | Japan
                | PacificExJapan
                | EmergingMarkets
                | DevelopedMarkets
                | Eurozone

    let generalizeRegion region = match region with
                                  | Eurozone -> Europe
                                  | Germany -> Europe
                                  | _ -> region

    let generalizeRegion2 region = match region with
                                   | EmergingMarkets -> EmergingMarkets
                                   | _ -> DevelopedMarkets

    type FixedIncomeNature = Sovereign
                            | Corporate
                            | Covered

    type AssetClass = Equity
                    | FixedIncome of FixedIncomeNature
                    | Commodities
                    | RealEstate
                    | Cash

    type Classifier = { What: AssetClass; Where: Region option; }

    open System
    type ClassifiedDividends = { Today: decimal; AccumulatedYtd: decimal; }
    type ClassifiedAsset = { Class: Classifier; Weight: decimal; Value: decimal; Return: decimal; Dividend: ClassifiedDividends; Date: DateTime }

    let classifyIsin isin = match isin with
                            | "DE0002635307" -> Some { What = Equity; Where = Some Europe }
                            | "DE0005933931" -> Some { What = Equity; Where = Some Germany }
                            | "DE000A1C22M3" -> Some { What = Equity; Where = Some USA }
                            | "FR0010270033" -> Some { What = Commodities; Where =  None }
                            | "IE0032895942" -> Some { What = FixedIncome Corporate; Where =  Some USA }
                            | "IE00B1FZS350" -> Some { What = RealEstate; Where =  Some DevelopedMarkets }
                            | "IE00B1FZS798" -> Some { What = FixedIncome Sovereign; Where =  Some USA }
                            | "IE00B2NPKV68" -> Some { What = FixedIncome Sovereign; Where =  Some EmergingMarkets }
                            | "IE00B3B8Q275" -> Some { What = FixedIncome Covered; Where =  Some Eurozone }
                            | "IE00B3F81R35" -> Some { What = FixedIncome Corporate; Where =  Some Eurozone }
                            | "IE00B3XXRP09" -> Some { What = Equity; Where =  Some USA }
                            | "IE00B4WXJJ64" -> Some { What = FixedIncome Sovereign; Where =  Some Eurozone }
                            | "IE00B5BMR087" -> Some { What = Equity; Where = Some USA }
                            | "IE00BTJRMP35" -> Some { What = Equity; Where = Some EmergingMarkets }
                            | "LU0136240974" -> Some { What = Equity; Where = Some Japan }
                            | "LU0446734526" -> Some { What = Equity; Where = Some PacificExJapan }
                            | "LU0480132876" -> Some { What = Equity; Where = Some EmergingMarkets }
                            | "LU0839027447" -> Some { What = Equity; Where = Some Japan }
                            | _ -> None