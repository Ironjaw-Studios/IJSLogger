using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.ijs.logger
{
    /// <summary>
    /// Configuration data for a single log channel.
    /// </summary>
    [Serializable]
    public class ChannelConfig
    {
        public LogChannel channel;
        public ChannelScope scope = ChannelScope.Both;
        public bool enabled = true;

        public ChannelConfig(LogChannel channel, ChannelScope scope = ChannelScope.Both, bool enabled = true)
        {
            this.channel = channel;
            this.scope = scope;
            this.enabled = enabled;
        }
    }

    /// <summary>
    /// Settings for IJSLogger system. Manages channel configurations and filtering.
    /// </summary>
    [CreateAssetMenu(fileName = "IJSLoggerSettings", menuName = "IJS/Logger Settings")]
    public class IJSLoggerSettings : ScriptableObject
    {
        [Header("Channel Configuration")]
        [Tooltip("Configuration for each log channel")]
        [SerializeField] private List<ChannelConfig> channelConfigs = new List<ChannelConfig>();

        [Header("Rate Limiting")]
        [Tooltip("Enable global rate limiting")]
        [SerializeField] private bool enableRateLimiting = true;

        [Tooltip("Default rate limit in seconds")]
        [SerializeField] private float defaultRateLimitSeconds = 0.1f;

        private static IJSLoggerSettings _instance;
        private static bool _instanceSearched;

        /// <summary>
        /// Gets the singleton instance of IJSLoggerSettings.
        /// </summary>
        public static IJSLoggerSettings Instance
        {
            get
            {
                if (_instance == null && !_instanceSearched)
                {
                    _instanceSearched = true;
                    _instance = Resources.Load<IJSLoggerSettings>("IJSLoggerSettings");

                    // Initialize default settings if not found
                    if (_instance == null)
                    {
                        Debug.LogWarning("[IJSLogger] No IJSLoggerSettings found in Resources folder. Using default settings. " +
                                         "Create one via Assets -> Create -> IJS -> Logger Settings and place it in a Resources folder.");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Checks if a channel is enabled based on current scope (editor vs build) and configuration.
        /// </summary>
        public static bool IsChannelEnabled(LogChannel channel)
        {
            // Default channel is always enabled
            if (channel == LogChannel.Default)
                return true;

            var instance = Instance;
            if (instance == null)
                return true; // If no settings, allow all channels

            var config = instance.channelConfigs.FirstOrDefault(c => c.channel == channel);
            if (config == null)
                return true; // If channel not configured, default to enabled

            if (!config.enabled)
                return false;

            // Check scope
#if UNITY_EDITOR
            return config.scope == ChannelScope.EditorOnly || config.scope == ChannelScope.Both;
#else
            return config.scope == ChannelScope.BuildOnly || config.scope == ChannelScope.Both;
#endif
        }

        /// <summary>
        /// Gets the configuration for a specific channel.
        /// </summary>
        public ChannelConfig GetChannelConfig(LogChannel channel)
        {
            return channelConfigs.FirstOrDefault(c => c.channel == channel);
        }

        /// <summary>
        /// Sets whether a channel is enabled.
        /// </summary>
        public void SetChannelEnabled(LogChannel channel, bool enabled)
        {
            var config = channelConfigs.FirstOrDefault(c => c.channel == channel);
            if (config != null)
            {
                config.enabled = enabled;
            }
            else
            {
                channelConfigs.Add(new ChannelConfig(channel, ChannelScope.Both, enabled));
            }
        }

        /// <summary>
        /// Sets the scope for a channel.
        /// </summary>
        public void SetChannelScope(LogChannel channel, ChannelScope scope)
        {
            var config = channelConfigs.FirstOrDefault(c => c.channel == channel);
            if (config != null)
            {
                config.scope = scope;
            }
            else
            {
                channelConfigs.Add(new ChannelConfig(channel, scope, true));
            }
        }

        /// <summary>
        /// Initializes default channel configurations.
        /// </summary>
        public void InitializeDefaults()
        {
            channelConfigs.Clear();

            // Add all channels with default settings
            foreach (LogChannel channel in Enum.GetValues(typeof(LogChannel)))
            {
                if (channel == LogChannel.Default)
                    continue;

                // Performance logs only in editor by default
                var scope = channel == LogChannel.Performance ? ChannelScope.EditorOnly : ChannelScope.Both;
                channelConfigs.Add(new ChannelConfig(channel, scope, true));
            }
        }

        private void OnValidate()
        {
            // Ensure we have at least the basic channels
            if (channelConfigs.Count == 0)
            {
                InitializeDefaults();
            }
        }

        public bool EnableRateLimiting => enableRateLimiting;
        public float DefaultRateLimitSeconds => defaultRateLimitSeconds;
    }
}
