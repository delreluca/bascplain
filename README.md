# bascplain
*bascplain* (Baader Bank/Scalable Capital explain) is an F# analysis tool for the Scalable Capital robo-advisor using the Baader Bank costumer reporting files.

## Does it work already?

The calculations seem to work with my personal CSV files.

Although the Suave web server is already integrated into the F# project it does not serve charts or figures yet. This will be the next step.

## What is it supposed to do?

Although Scalable Capital offers a performance chart no data about the history of the asset allocation is available on their site.
So the main aim of this tool is to show the weights of the individual asset classes and regions over time. This is possible through the CSV files provided on their broker's web interface, provided you use Baader Bank as their broker.

Other possible interesting features could be:
- Performance comparison between buy-and-hold and/or fixed asset allocations and the dynamic risk management of the robo-advisor
- Retrospective risk measures (maximum drawdown, volatility, sample VaR) comparison
- VaR exceedence test, once the time series is long enough (one could use the squarr-root rule to approximate the daily VaR but this would probably contradict the underlying risk model)
- Compare the paid fees to a risk-adjusted return

## Technology

- F# on .NET Core 
- CSV type providers are used for CSV reading
- Suave will server web pages
