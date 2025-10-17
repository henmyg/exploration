# Price Graph
```
As a user
I want to see the electricity prices in the near future
So I can plan when to start my electricity hungry appliances
```

```
As a user
I want to see 'now'
So it's easy to see how far from low/high prices I am
```

## Detailed Requirements

### Display all available electricity prices (15-minute intervals) from today and tomorrow
- **AC**: `PriceGraph_ShowsAllAvailablePricesFromTodayAndTomorrow`
- **AC**: `PriceGraph_Displays15MinuteIntervalPrices`
- **AC**: `PriceGraph_UpdatesWhenNewPriceDataAvailable`

### Visual indicator shows current time on the graph
- **AC**: `PriceGraph_ShowsNowMarker`
- **AC**: `PriceGraph_NowMarkerPosition_UpdatesEveryMinute`
