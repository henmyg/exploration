# Background Price Sync

```
As a user
I want electricity prices to be automatically fetched and updated
So that I always have the latest price information without manual intervention
```

## Detailed Requirements

### Sync prices immediately on app startup
- **AC**: `BackgroundPriceSync_SyncsImmediatelyOnStartup`

### Sync tomorrow's prices at 13:05 every day
- **AC**: `BackgroundPriceSync_SyncsAt1305Daily`
- **AC**: `BackgroundPriceSync_WaitsUntil1305_WhenStartedEarlier`

### Retry every hour if tomorrow's prices are not available
- **AC**: `BackgroundPriceSync_RetriesInOneHour_WhenTomorrowPricesMissing`
- **AC**: `BackgroundPriceSync_StopsRetrying_WhenTomorrowPricesReceived`
- **AC**: `BackgroundPriceSync_StopsOldRetryLoop_When1305Arrives`
