# IJS Logger

Advanced Unity logging system with channel filtering, rate limiting, contexts, assertions, and conditional logging.

## Installation

Select "Add package from git URL" in Unity Package Manager and paste:
```
https://github.com/sathyarajshetigar/IJSLogger.git#upm
```

## What's New in v1.1.0

### 🎯 Channel-Based Filtering
Organize logs into channels (Audio, Network, Physics, AI, UI, Gameplay, Performance, etc.) and control them independently. Configure channels to log only in Editor, only in Builds, or Both.

### ⏱️ Smart Rate Limiting
Prevent log spam with automatic rate limiting. Suppressed logs are counted and reported.

### 📝 Log Contexts & Scopes
Add automatic context to your logs using the disposable pattern:
```csharp
using (new LogContext("Level Loading"))
{
    _logger.PrintLog("Loading assets"); // [Level Loading] Loading assets
}
```

### ✅ Assertion System
Fluent assertion API with automatic logging:
```csharp
_logger.Assert(health > 0, "Health must be positive!")
    .OnFailure(() => health = 0)
    .PauseEditor();
```

### 🔍 Conditional Logging
Log only when conditions are met, with lazy evaluation support:
```csharp
_logger.LogIf(() => debugMode, () => $"Debug: {expensiveOperation()}");
```

### 🖥️ Editor Windows
- **Settings Window** (`Window → IJS Logger → Settings`): Configure channels and scopes
- **Log Viewer** (`Window → IJS Logger → Log Viewer`): View all Unity logs with filtering

## Quick Start

### Basic Usage

```csharp
using UnityEngine;
using com.ijs.logger;

public class MyGame : MonoBehaviour
{
    // Create logger with channel
    private readonly IJSLogger _logger = new IJSLogger("MyGame", Color.cyan, true, LogChannel.Gameplay);

    void Start()
    {
        // Simple logging
        _logger.PrintLog("Game started!");

        // Different log types
        _logger.PrintLog("Warning!", LogType.Warning);
        _logger.PrintLog("Error!", LogType.Error);
    }
}
```

### Advanced Usage

```csharp
public class AdvancedExample : MonoBehaviour
{
    private readonly IJSLogger _gameplay = new IJSLogger("Gameplay", Color.cyan, true, LogChannel.Gameplay);
    private readonly IJSLogger _perf = new IJSLogger("Perf", Color.magenta, true, LogChannel.Performance);

    [SerializeField] private float health = 100f;

    void Start()
    {
        // Context-based logging
        using (new LogContext("Initialization"))
        {
            _gameplay.PrintLog("Loading level");
            _gameplay.PrintLog("Spawning player");
        }

        // Assertions
        _gameplay.Assert(health > 0, "Health must be positive!");
        _gameplay.ValidateRange(health, 0, 100, nameof(health));

        // Conditional logging with lazy evaluation
        _gameplay.LogIf(
            () => health < 20,
            () => $"Low health: {health}",
            LogType.Warning
        );
    }

    void Update()
    {
        // Rate-limited logging (prevents spam)
        _perf.LogThrottled($"FPS: {1f / Time.deltaTime:F1}", 1.0f);
    }

    void TakeDamage(float damage)
    {
        using (new LogContext("Combat"))
        {
            health -= damage;
            _gameplay.PrintLog($"Took {damage} damage. Health: {health}");

            // Assert with callback
            _gameplay.Assert(health >= 0, "Health cannot be negative!")
                .OnFailure(() => health = 0);
        }
    }
}
```

## Channel System

### Available Channels

| Channel | Description | Default Scope |
|---------|-------------|---------------|
| `Default` | Always enabled | Both |
| `Audio` | Audio system logs | Both |
| `Network` | Network/multiplayer logs | Both |
| `Physics` | Physics system logs | Both |
| `AI` | AI and pathfinding logs | Both |
| `UI` | User interface logs | Both |
| `Gameplay` | Core gameplay logs | Both |
| `Performance` | Performance metrics | Editor Only |
| `Animation` | Animation system logs | Both |
| `Input` | Input handling logs | Both |
| `Rendering` | Rendering system logs | Both |
| `System` | General system logs | Both |

### Channel Scopes

- **Editor Only**: Logs only in Unity Editor (not in builds)
- **Build Only**: Logs only in builds (not in editor)
- **Both**: Logs in both editor and builds

### Configuring Channels

**In Editor:**
1. Open `Window → IJS Logger → Settings`
2. Enable/disable channels
3. Set channel scope for each
4. Use quick actions (Enable All, Disable All, Reset)

**In Code:**
```csharp
var settings = IJSLoggerSettings.Instance;
settings.SetChannelEnabled(LogChannel.Performance, true);
settings.SetChannelScope(LogChannel.Performance, ChannelScope.EditorOnly);
```

## Key Features

### 1. Rate Limiting

Prevent console spam from tight loops:

```csharp
void Update()
{
    // Only logs once per second, shows suppression count
    _logger.LogThrottled($"Player position: {transform.position}", 1.0f);
    // Output after 1 second: "Player position: (1,2,3) (suppressed 59x)"
}
```

### 2. Log Contexts

Add hierarchical context to logs:

```csharp
using (new LogContext("Level Loading"))
{
    _logger.PrintLog("Loading assets");

    using (new LogContext("Spawning"))
    {
        // Output: [Level Loading > Spawning] Spawned 10 enemies
        _logger.PrintLog("Spawned 10 enemies");
    }
}
```

### 3. Conditional Logging

#### Simple Conditions
```csharp
_logger.LogIf(debugMode, "Debug information");
_logger.LogIf(health < 20, "Low health!", LogType.Warning);
```

#### Lazy Evaluation
Avoid expensive operations when logs are disabled:
```csharp
_logger.LogIf(
    () => debugMode && isInCombat,
    () => $"Stats: Health={health}, Enemies={enemies.Count}, Pos={transform.position}",
    LogType.Log
);
```

### 4. Assertions

#### Basic Assertions
```csharp
_logger.Assert(health > 0, "Health must be positive!");
```

#### With Callbacks
```csharp
_logger.Assert(enemyCount <= 100, "Too many enemies!")
    .OnFailure(() => enemyCount = 100);
```

#### Validation Helpers
```csharp
_logger.ValidateNotNull(player, nameof(player));
_logger.ValidateRange(health, 0, 100, nameof(health));
```

#### Editor Integration
```csharp
_logger.Assert(isInitialized, "Not initialized!")
    .PauseEditor()      // Pauses Unity Editor (Editor only)
    .BreakDebugger();   // Breaks if debugger attached
```

## Editor Windows

### Settings Window
**Window → IJS Logger → Settings**

Features:
- Visual channel configuration with color-coded status
- Enable/disable individual channels
- Set channel scope (Editor/Build/Both)
- Quick actions (Enable All, Disable All, Reset to Defaults)
- About tab with quick start guide

### Log Viewer Window
**Window → IJS Logger → Log Viewer**

Features:
- Captures **all Unity logs** (not just IJSLogger)
- IJSLogger logs highlighted with special background
- Filter by type (Log, Warning, Error)
- Filter to show only IJSLogger logs
- Search functionality
- Timestamps for all logs
- Auto-scroll option
- Export logs to file
- Adjustable max logs limit (100-5000)

## Global Controls

### Enable/Disable All Logging

**Menu:** `IJS → Logger → Enable Logs` / `Disable Logs`

When disabled, the `USE_LOGS` scripting define is removed, and all logging code is completely stripped from builds using `[Conditional("USE_LOGS")]` attributes.

### Filtering Hierarchy

Logs must pass all these checks to be displayed:

1. ✅ **Global USE_LOGS** (build-time via scripting defines)
2. ✅ **Channel Enabled** (runtime via settings)
3. ✅ **Channel Scope** (Editor/Build/Both)
4. ✅ **Instance Enabled** (runtime via constructor or ToggleLogs)
5. ✅ **Rate Limiting** (runtime per-message throttling)

This hierarchy allows fine-grained control:
- Ship builds with only Error/Warning channels enabled
- Enable Performance logging only in Editor
- Disable specific logger instances at runtime
- Prevent spam with rate limiting

## API Reference

### IJSLogger Constructor
```csharp
IJSLogger(
    string prefix = "",
    Color? color = null,
    bool logsEnabled = true,
    LogChannel channel = LogChannel.Default
)
```

### Instance Methods

| Method | Description |
|--------|-------------|
| `PrintLog(message, logType, gameObject)` | Log a message with instance settings |
| `LogIf(condition, message, logType, gameObject)` | Log only if condition is true |
| `LogIf(conditionFunc, messageFunc, logType, gameObject)` | Lazy evaluation conditional logging |
| `LogThrottled(message, minIntervalSeconds, logType, gameObject)` | Rate-limited logging |
| `Assert(condition, message)` | Assert condition, returns fluent LogAssert |
| `ValidateNotNull(obj, paramName)` | Validate object is not null |
| `ValidateRange(value, min, max, paramName)` | Validate value is in range |
| `ToggleLogs(enable)` | Enable/disable this logger instance |
| `ModifyPrefix(prefix)` | Change the log prefix |
| `ModifyColor(color)` | Change the log color |

### Static Methods

| Method | Description |
|--------|-------------|
| `Log(message, type, gameObject, color)` | Static logging method |

### LogContext Class
```csharp
using (new LogContext("ContextName"))
{
    // All logs here will include [ContextName] prefix
}
```

### LogAssert Class
```csharp
logger.Assert(condition, message)
    .OnFailure(() => { /* callback */ })
    .BreakDebugger()
    .PauseEditor();  // Editor only
```

### LogRateLimiter Class
```csharp
// Usually used internally, but available for custom use
LogRateLimiter.ShouldLog(key, intervalSeconds);
LogRateLimiter.GetSuppressedCount(key);
LogRateLimiter.Clear();
```

## Configuration

### Creating Settings Asset

**Option 1:** Via Editor Window
1. Open `Window → IJS Logger → Settings`
2. Click "Create Settings Asset"
3. Configure channels as needed

**Option 2:** Via Asset Menu
1. Create folder: `Assets/_PackageRoot/Runtime/Resources/`
2. Right-click → `Create → IJS → Logger Settings`
3. Name it `IJSLoggerSettings`
4. Configure channels

### Default Settings

If no settings asset exists:
- All channels are enabled by default
- `Performance` channel defaults to **Editor Only**
- All other channels default to **Both**

## Best Practices

1. **Use Channels**: Organize logs by system for better filtering
   ```csharp
   var audioLogger = new IJSLogger("Audio", Color.yellow, true, LogChannel.Audio);
   var networkLogger = new IJSLogger("Network", Color.green, true, LogChannel.Network);
   ```

2. **Use Contexts**: Group related logs together
   ```csharp
   using (new LogContext("Player Spawning"))
   {
       _logger.PrintLog("Creating player");
       _logger.PrintLog("Setting position");
   }
   ```

3. **Rate Limit in Loops**: Always throttle logs in Update/FixedUpdate
   ```csharp
   void Update()
   {
       _logger.LogThrottled("Update info", 1.0f);
   }
   ```

4. **Use Lazy Evaluation**: For expensive string operations
   ```csharp
   _logger.LogIf(() => condition, () => $"Expensive: {CalculateStats()}");
   ```

5. **Configure Scopes**: Set Performance/Debug channels to Editor-only
   ```csharp
   // In Settings Window or:
   settings.SetChannelScope(LogChannel.Performance, ChannelScope.EditorOnly);
   ```

6. **Use Assertions**: Validate assumptions during development
   ```csharp
   _logger.ValidateNotNull(player, nameof(player));
   _logger.ValidateRange(health, 0, 100, nameof(health));
   ```

7. **Color Code Systems**: Use consistent colors for each system
   ```csharp
   var audioLogger = new IJSLogger("Audio", Color.yellow, true, LogChannel.Audio);
   var aiLogger = new IJSLogger("AI", Color.magenta, true, LogChannel.AI);
   ```

## Performance

- **Zero Cost in Production**: When `USE_LOGS` is not defined, all logging code is stripped via `[Conditional]`
- **Minimal Runtime Cost**: Channel checks are simple boolean comparisons
- **Smart Rate Limiting**: Only tracks messages that are actually rate-limited
- **Lazy Evaluation**: Expensive operations only execute when logs will be displayed
- **No Allocations**: Efficient string handling and caching

## Examples

See `Assets/_PackageRoot/Samples~/IJSLoggerExamples.cs` for comprehensive examples including:
- Basic logging with channels
- Channel filtering
- Rate limiting
- Log contexts
- Assertions
- Conditional logging
- Complete gameplay scenarios

## Troubleshooting

### Logs Not Appearing

1. Check if `USE_LOGS` scripting define is enabled: `IJS → Logger → Enable Logs`
2. Verify channel is enabled: `Window → IJS Logger → Settings`
3. Check channel scope matches current environment (Editor vs Build)
4. Ensure logger instance is enabled: `logger.ToggleLogs(true)`
5. For throttled logs, check if rate limit interval has passed

### Settings Not Working

1. Ensure settings asset exists in a Resources folder
2. Check asset is named `IJSLoggerSettings`
3. Try creating new settings via `Window → IJS Logger → Settings`

### Editor Windows Not Showing

1. Check `Window → IJS Logger → Settings` or `Log Viewer`
2. Verify editor assembly is properly referenced
3. Reimport package if necessary

## Migration from v1.0.x

The new version is **backwards compatible**. Existing code will work without changes:

```csharp
// Old code still works
var logger = new IJSLogger("MyClass", Color.cyan);
logger.PrintLog("Hello");
```

To use new features, simply add the channel parameter:

```csharp
// New code with channel
var logger = new IJSLogger("MyClass", Color.cyan, true, LogChannel.Gameplay);
```

## Changelog

### Version 1.1.0 (Current)
- ✨ Added channel-based filtering system with 12 predefined channels
- ✨ Added ChannelScope support (EditorOnly, BuildOnly, Both)
- ✨ Added smart rate limiting with suppression counting
- ✨ Added log contexts with disposable pattern
- ✨ Added assertion system with fluent API
- ✨ Added conditional logging with lazy evaluation
- ✨ Added Settings window for channel configuration
- ✨ Added Log Viewer window for all Unity logs
- 🔧 Updated constructor to accept LogChannel parameter
- 🔧 Updated PrintLog to support contexts automatically
- 📚 Comprehensive README with examples and API documentation
- 📚 Added example script demonstrating all features

### Version 1.0.11
- Initial release with basic logging functionality
- Color-coded logs
- Prefix support
- Instance-level enable/disable
- Global USE_LOGS scripting define control

## Support & Contributing

For issues, questions, or feature requests:
- GitHub: https://github.com/sathyarajshetigar/IJSLogger
- Create an issue with details and example code

## License

See LICENSE file in the repository.

---

**Happy Logging!** 🎉

Built with ❤️ by Ironjaw Studios
