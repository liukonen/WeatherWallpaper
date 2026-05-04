# WeatherWallpaper

Tags: `C#` `WinForms` `MEF Plugin Architecture` `Windows Registry` `REST APIs`

## Dynamic Wallpaper Daemon with Weather Awareness and Plugin Architecture

### Context/Story

WeatherWallpaper was created to transform static desktop wallpapers into dynamic, weather-responsive visuals. This project addresses the limitation of traditional desktop backgrounds by making them reactive to live weather data and the time of day. Originally relying on a single weather source (Yahoo), the system evolved to support multiple weather providers (NOAA, Microsoft Weather, OpenWeather) to ensure robustness and eliminate single-point failures. The plugin model allows developers to contribute new weather sources or sunrise/sunset implementations without forking the project.

### Architecture & Decisions

- **MEF (Managed Extensibility Framework):** Enables lazy composition of weather and sunrise/sunset providers. Plugins load only when needed, ensuring zero coupling between the core and implementations.
- **Multi-source weather resilience:** Integrates with three independent REST API providers (NOAA/Gov3, Microsoft Weather, OpenWeather) and gracefully falls back if a source is unreachable.
- **Time-based scheduling:** Uses a 3-second polling loop with per-hour and per-day exclusion masks (BitArray optimization). Users can block updates during business hours or specific weekdays.
- **System tray integration:** Runs as a quiet notification icon, with zero visual footprint until user interaction.
- **Windows Registry + P/Invoke:** Utilizes direct registry writes and low-level DLL calls (SystemParametersInfo) for atomic wallpaper changes, avoiding the need for external tools.
- **Custom theme compression:** Wallpaper sets are packaged as ZipArchiveEntry archives for portability and ease of distribution.

### Key Features

- **Day/Night Wallpaper Switching:** Separate imagery for daylight and night hours based on computed sunrise/sunset times.
- **Weather-Aware Imagery:** Maps current weather conditions (clear, cloudy, rain, snow, etc.) to corresponding wallpaper sets.
- **Plugin Extensibility:** Developers can implement `ISharedWeatherInterface`, `ISharedSunRiseSetInterface`, or `ILatLongInterface` to add custom providers.
- **Granular Scheduling Control:** Users can exclude specific hours or weekdays from wallpaper updates via BitArray masks.
- **Configurable Providers:** Users select active weather sources, and the fallback chain executes on failure.
- **Custom Theme Support:** Load wallpaper themes from compressed archives; zip-based distribution allows easy theme sharing and installation.

### Quick Start

1. **Build:** Open `WeatherDesktop.sln` in Visual Studio (requires .NET Framework 4.8+).
2. **Run:** Execute `WeatherDesktop.exe`, which launches as a system tray application.
3. **Configure:** Right-click the tray icon → Settings. Select the active weather provider and configure exclusion hours.
4. **Verify:** Observe wallpaper changes on the hour (or as configured in the settings).

### 🚀 Features Overview

WeatherWallpaper is a dynamic wallpaper daemon that transforms your desktop into a live reflection of the weather and time of day. It offers a wide range of features that make it an ideal choice for developers and users who want a personalized and responsive desktop experience. Whether you're a developer looking to contribute new weather sources or a user seeking a visually engaging wallpaper experience, this project provides a seamless and efficient solution.

### 🧱 Technology Stack

WeatherWallpaper is built using the following technologies:

- **Frontend:** C#, WinForms
- **Plugin Architecture:** MEF (Managed Extensibility Framework)
- **Data Integration:** REST APIs (NOAA, Microsoft Weather, OpenWeather)
- **System Integration:** Windows Registry, P/Invoke for low-level system calls
- **File Management:** ZipArchiveEntry for custom theme compression and distribution

### 🤝 Contributing Guidelines

We welcome contributions from the community! To get started, follow these steps:

1. **Fork** the repository on GitHub.
2. **Create a new branch** for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes and commit them**:
   ```bash
   git commit -m "Your commit message"
   ```
4. **Push your changes to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```
5. **Create a pull request** on GitHub.

### 📄 License

WeatherWallpaper is licensed under the Microsoft Public License (Ms-PL) license. See the [LICENSE] file for more information.

## Additional Credit

- Weather icons provided by [weather-iconic](https://github.com/jackd248/weather-iconic/)
- Main icon provided by Martz90, [IconArchive](http://www.iconarchive.com/artist/martz90.html)
- Stock sunrise/set service is from [sunrise-sunset.org](https://sunrise-sunset.org)
- Stock weather data is from [weather.gov](https://www.weather.gov/)