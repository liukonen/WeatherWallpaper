# WeatherWallpaper

Tags: `C#` `WinForms` `MEF Plugin Architecture` `Windows Registry` `REST APIs'`

Dynamic wallpaper daemon that responds to weather conditions and time of day, with multi-source weather resilience and extensible plugin architecture.

## Context

Desktop wallpaper was a static asset. This project inverts that—making it reactive to live weather data and diurnal cycles. Originally mono-source (Yahoo), the system evolved to support multiple weather providers (NOAA, Microsoft Weather, OpenWeather) to eliminate single-point failure and provide failover robustness. The plugin model enables developers to contribute new weather sources or sunrise/set implementations without forking.

## Architecture & Decisions

- MEF (Managed Extensibility Framework) — Lazy composition of weather and sun-rise/set providers. Plugins load only when needed; zero coupling between the core and implementations.
- Multi-source weather resilience — Three independent REST API integrations (NOAA/Gov3, Microsoft Weather, OpenWeather). Falls back gracefully if a source is unreachable.
- Time-based scheduling — 3-second polling loop with per-hour and per-day exclusion masks (BitArray optimization). Users can block updates during business hours or specific weekdays.
- System tray integration — Runs quietly as a notification icon; zero visual footprint until user interaction.
- Windows Registry + P/Invoke — Direct registry writes and low-level DLL calls (SystemParametersInfo) for atomic wallpaper changes. Avoids external tools.
- Custom theme compression — Wallpaper sets packaged as ZipArchiveEntry archives for portability.

## Key Features

- Day/night wallpaper switching — Separate imagery for daylight and night hours based on computed sunrise/sunset.
- Weather-aware imagery — Maps current conditions (clear, cloudy, rain, snow, etc.) to corresponding wallpaper sets.
- Plugin extensibility — Implement ISharedWeatherInterface, ISharedSunRiseSetInterface, or ILatLongInterface to add custom providers.
- Granular scheduling control — Exclude specific hours or weekdays from wallpaper updates via BitArray masks.
- Configurable providers — Users select active weather source; fallback chain executes on failure.
- Custom theme support — Load wallpaper themes from compressed archives; zip-based distribution.

## Additional Credit

- Weather icons provided by [weather-iconic](https://github.com/jackd248/weather-iconic/)
- Main icon provided by Martz90, [IconArchive](http://www.iconarchive.com/artist/martz90.html)
- Stock sunrise/set service is from [sunrise-sunset.org](https://sunrise-sunset.org)
- Stock weather data is from [weather.gov](https://www.weather.gov/)

## Quick Start

1. Build: Open WeatherDesktop.sln in Visual Studio (requires .NET Framework 4.8+).
2. Run: WeatherDesktop.exe launches as a system tray application.
3. Configure: Right-click tray icon → Settings. Select weather provider and exclusion hours.
4. Verify: Check wallpaper changes on the hour (or as configured).