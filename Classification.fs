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

    let parseRegion (s : string) =
        match s.ToLowerInvariant() with
            | "de"
            | "germany" -> Some Germany
            | "eu"
            | "europe" -> Some Europe
            | "euro"
            | "eur"
            | "eurozone" -> Some Eurozone
            | "us"
            | "usa" -> Some USA
            | "jp"
            | "japan" -> Some Japan
            | "pacific"
            | "pacificexjapan" -> Some PacificExJapan
            | "em"
            | "emergingmarkets" -> Some EmergingMarkets
            | "developedmarkets" -> Some DevelopedMarkets
            | _ -> None

    type FixedIncomeNature = Sovereign
                            | Corporate
                            | Covered

    type EquityStrategy = LargeCapOrUnspecified
                        | SmallCap
                        | MomentumFactor
                        | ValueFactor

    type AssetClass = Equity of EquityStrategy
                    | FixedIncome of FixedIncomeNature
                    | Commodities
                    | RealEstate
                    | Cash
                    | Unknown

    let parseAssetClass (s : string) =
        match s.ToLowerInvariant() with
            | "lc"
            | "largecap" -> Some <| Equity LargeCapOrUnspecified
            | "sc"
            | "smallcap" -> Some <| Equity SmallCap
            | "value" -> Some <| Equity ValueFactor
            | "momentum" -> Some <| Equity MomentumFactor
            | "gov"
            | "government"
            | "sov"
            | "sovereign" -> Some <| FixedIncome Sovereign
            | "corp"
            | "corporate" -> Some <| FixedIncome Corporate
            | "cov"
            | "covered" -> Some <| FixedIncome Covered
            | "comm"
            | "commodities" -> Some Commodities
            | "re"
            | "realestate" -> Some RealEstate
            | "cash" -> Some Cash
            | _ -> None

    type Classifier = { What: AssetClass; Where: Region option; }

    let classifyIsin isin = match isin with
                            | "DE0002635307" -> Some { What = Equity LargeCapOrUnspecified; Where = Some Europe }
                            | "DE0005933931" -> Some { What = Equity LargeCapOrUnspecified; Where = Some Germany }
                            | "DE000A1C22M3" -> Some { What = Equity LargeCapOrUnspecified; Where = Some USA }
                            | "FR0010270033" -> Some { What = Commodities; Where =  None }
                            | "IE0032895942" -> Some { What = FixedIncome Corporate; Where =  Some USA }
                            | "IE00B1FZS350" -> Some { What = RealEstate; Where =  Some DevelopedMarkets }
                            | "IE00B1FZS798" -> Some { What = FixedIncome Sovereign; Where =  Some USA }
                            | "IE00B1FZSF77" -> Some { What = RealEstate; Where = Some USA } //discovered 18 May 2018
                            | "IE00B2NPKV68" -> Some { What = FixedIncome Sovereign; Where =  Some EmergingMarkets }
                            | "IE00B3B8Q275" -> Some { What = FixedIncome Covered; Where =  Some Eurozone }
                            | "IE00B3F81R35" -> Some { What = FixedIncome Corporate; Where =  Some Eurozone }
                            | "IE00B3XXRP09" -> Some { What = Equity LargeCapOrUnspecified; Where =  Some USA }
                            | "IE00B4WXJJ64" -> Some { What = FixedIncome Sovereign; Where =  Some Eurozone }
                            | "IE00B52MJY50" -> Some { What = Equity LargeCapOrUnspecified; Where = Some PacificExJapan } //discovered 22 Jun 2018
                            | "IE00B5BMR087" -> Some { What = Equity LargeCapOrUnspecified; Where = Some USA }
                            | "IE00BD1F4M44" -> Some { What = Equity ValueFactor; Where = Some USA }
                            | "IE00BD1F4N50" -> Some { What = Equity MomentumFactor; Where = Some USA }
                            | "IE00BJ38QD84" -> Some { What = Equity SmallCap; Where = Some USA }
                            | "IE00BQN1K786" -> Some { What = Equity MomentumFactor; Where = Some Europe }
                            | "IE00BQN1K901" -> Some { What = Equity ValueFactor; Where = Some Europe }
                            | "IE00BTJRMP35" -> Some { What = Equity LargeCapOrUnspecified; Where = Some EmergingMarkets }
                            | "LU0136240974" -> Some { What = Equity LargeCapOrUnspecified; Where = Some Japan }
                            | "LU0290355717" -> Some { What = FixedIncome Sovereign; Where = Some Eurozone } //discovered 4 Jul 2018
                            | "LU0322253906" -> Some { What = Equity SmallCap; Where = Some Europe }
                            | "LU0446734526" -> Some { What = Equity LargeCapOrUnspecified; Where = Some PacificExJapan }
                            | "LU0480132876" -> Some { What = Equity LargeCapOrUnspecified; Where = Some EmergingMarkets }
                            | "LU0489337690" -> Some { What = RealEstate; Where = Some Europe } //discovered 18 May 2018
                            | "LU0839027447" -> Some { What = Equity LargeCapOrUnspecified; Where = Some Japan }
                            | "LU0908500753" -> Some { What = Equity LargeCapOrUnspecified; Where = Some Europe }
                            | "<CASH>" -> Some { What = Cash; Where = None; }
                            | _ -> None