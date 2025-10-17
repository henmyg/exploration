# Current Price
```
As a User
I want to see the current electricity price
So that I can decide if I want to start electricity hungry machines
```

## Detailed Requirements

### "Current" means the electricity price for the current 15-minute quarter
- **AC**: `CurrentPrice_At1417_ShowsPriceFor1415Quarter`
- **AC**: `CurrentPrice_At1430_ShowsPriceFor1430Quarter`
- **AC**: `CurrentPrice_VariousTimes_ReturnsCorrectQuarter`

### Price updates every 15 minutes to stay synchronized
- **AC**: `CurrentPrice_UpdatesEvery15Minutes`
- **AC**: `CurrentPrice_CalculatesCorrectDelayToNextQuarterBoundary`

### Display placeholder/error state when data unavailable
- **AC**: `CurrentPrice_NoDataAvailable_ShowsPlaceholder`