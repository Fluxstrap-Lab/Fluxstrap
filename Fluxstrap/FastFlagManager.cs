using Fluxstrap.Enums.FlagPresets;

namespace Fluxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        private Dictionary<string, object> OriginalProp = new();

        public override string ClassName => nameof(FastFlagManager);

        public override string LOG_IDENT_CLASS => ClassName;

        public override string FileName => "ClientAppSettings.json";

        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings", FileName);

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },

            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },

            { "Rendering.FPSCap.RemoveLimit", "FFlagTaskSchedulerLimitTargetFpsTo2402" },
            { "Rendering.FPSCap.Target", "DFIntTaskSchedulerTargetFps" },
        };

        public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes => new Dictionary<MSAAMode, string?>
        {
            { MSAAMode.Default, null },
            { MSAAMode.x1, "1" },
            { MSAAMode.x2, "2" },
            { MSAAMode.x4, "4" }
        };

        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels => new Dictionary<TextureQuality, string?>
        {
            { TextureQuality.Default, null },
            { TextureQuality.Level0, "0" },
            { TextureQuality.Level1, "1" },
            { TextureQuality.Level2, "2" },
            { TextureQuality.Level3, "3" },
        };

        // all fflags are stored as strings
        // to delete a flag, set the value as null
        public void SetValue(string key, object? value)
        {
            const string LOG_IDENT = "FastFlagManager::SetValue";

            if (value is null)
            {
                if (Prop.ContainsKey(key))
                    App.Logger.WriteLine(LOG_IDENT, $"Deletion of '{key}' is pending");

                Prop.Remove(key);
            }
            else
            {
                if (Prop.ContainsKey(key))
                {
                    if (key == Prop[key].ToString())
                        return;

                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{Prop[key]}' to '{value}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");
                }

                Prop[key] = value.ToString()!;
            }
        }

        // this returns null if the fflag doesn't exist
        public string? GetValue(string key)
        {
            // check if we have an updated change for it pushed first
            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public void SetPresetEnum(string prefix, string target, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
            {
                if (pair.Key.StartsWith($"{prefix}.{target}"))
                    SetValue(pair.Value, value);
                else
                    SetValue(pair.Value, null);
            }
        }

        public string? GetPreset(string name)
        {
            if (!PresetFlags.ContainsKey(name))
            {
                App.Logger.WriteLine("FastFlagManager::GetPreset", $"Could not find preset {name}");
                Debug.Assert(false, $"Could not find preset {name}");
                return null;
            }

            return GetValue(PresetFlags[name]);
        }

        public T GetPresetEnum<T>(IReadOnlyDictionary<T, string> mapping, string prefix, string value) where T : Enum
        {
            foreach (var pair in mapping)
            {
                if (pair.Value == "None")
                    continue;

                if (GetPreset($"{prefix}.{pair.Value}") == value)
                    return pair.Key;
            }

            return mapping.First().Key;
        }

        public override void Save()
        {
            // convert all flag values to strings before saving

            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();

            // clone the dictionary
            OriginalProp = new(Prop);
        }

        public override bool Load(bool alertFailure = true)
        {
            bool result = base.Load(alertFailure);

            // clone the dictionary
            OriginalProp = new(Prop);

            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");

            return result;
        }

        public void ApplyPreset(Enums.FlagPresets.FastFlagPreset preset)
        {
            switch (preset)
            {
                case Enums.FlagPresets.FastFlagPreset.Off:
                    break;

                case Enums.FlagPresets.FastFlagPreset.Performance:
                    SetPreset("Rendering.MSAA", "1");
                    SetValue("DFFlagTextureQualityOverrideEnabled", "True");
                    SetValue("DFIntTextureQualityOverride", "0");
                    SetPreset("Rendering.DisableScaling", "True");
                    break;

                case Enums.FlagPresets.FastFlagPreset.Quality:
                    SetPreset("Rendering.MSAA", "4");
                    SetValue("DFFlagTextureQualityOverrideEnabled", "True");
                    SetValue("DFIntTextureQualityOverride", "3");
                    SetPreset("Rendering.DisableScaling", "False");
                    break;

                case Enums.FlagPresets.FastFlagPreset.Competitive:
                    SetPreset("Rendering.MSAA", "2");
                    SetValue("DFFlagTextureQualityOverrideEnabled", "True");
                    SetValue("DFIntTextureQualityOverride", "1");
                    SetPreset("Rendering.DisableScaling", "True");
                    break;

                case Enums.FlagPresets.FastFlagPreset.UltimatePerformance:
                    ApplyUltimatePerformancePreset();
                    break;

                case Enums.FlagPresets.FastFlagPreset.ExtremePerformance:
                    ApplyExtremePerformancePreset();
                    break;
            }

            Save();
        }

        private void ApplyUltimatePerformancePreset()
        {
            SetPreset("Rendering.MSAA", "2");
            SetValue("DFFlagTextureQualityOverrideEnabled", "True");
            SetValue("DFIntTextureQualityOverride", "3");
            SetPreset("Rendering.DisableScaling", "True");

            // FPS Unlock
            SetPreset("Rendering.FPSCap.RemoveLimit", "False");
            SetPreset("Rendering.FPSCap.Target", App.Settings.Prop.FPSCap.ToString());

            // Disable telemetry (CPU savings, no visual impact)
            SetValue("FFlagDebugDisableTelemetryEphemeralCounter", "True");
            SetValue("FFlagDebugDisableTelemetryEphemeralStat", "True");
            SetValue("FFlagDebugDisableTelemetryEventIngest", "True");
            SetValue("FFlagDebugDisableTelemetryPoint", "True");
            SetValue("FFlagDebugDisableTelemetryV2Counter", "True");
            SetValue("FFlagDebugDisableTelemetryV2Event", "True");
            SetValue("FFlagDebugDisableTelemetryV2Stat", "True");

            // Direct3D 11
            SetValue("FFlagDebugGraphicsPreferD3D11", "True");

            // Ensure shadows render at full quality
            SetValue("FIntRenderShadowIntensity", "1");
        }

        private void ApplyExtremePerformancePreset()
        {
            SetPreset("Rendering.MSAA", "1");
            SetValue("DFFlagTextureQualityOverrideEnabled", "True");
            SetValue("DFIntTextureQualityOverride", "0");
            SetPreset("Rendering.DisableScaling", "True");

            // FPS Unlock
            SetPreset("Rendering.FPSCap.RemoveLimit", "False");
            SetPreset("Rendering.FPSCap.Target", App.Settings.Prop.FPSCap.ToString());

            // Disable telemetry
            SetValue("FFlagDebugDisableTelemetryEphemeralCounter", "True");
            SetValue("FFlagDebugDisableTelemetryEphemeralStat", "True");
            SetValue("FFlagDebugDisableTelemetryEventIngest", "True");
            SetValue("FFlagDebugDisableTelemetryPoint", "True");
            SetValue("FFlagDebugDisableTelemetryV2Counter", "True");
            SetValue("FFlagDebugDisableTelemetryV2Event", "True");
            SetValue("FFlagDebugDisableTelemetryV2Stat", "True");

            // GPU & threading
            SetValue("FFlagDebugGraphicsPreferD3D11", "True");

            // Render quality reduction
            SetValue("DFIntDebugFRMQualityLevelOverride", "1");
            SetValue("FIntFRMMaxGrassDistance", "0");
            SetValue("FIntFRMMinGrassDistance", "0");
            SetValue("FFlagDisablePostFx", "True");
            SetValue("FIntRenderShadowIntensity", "0");
            SetValue("FIntSSAOMipLevels", "0");
            SetValue("FIntBloomFrmCutoff", "0");
            SetValue("FFlagGlobalWindRendering", "False");

            // CSG LOD at minimum
            SetValue("DFIntCSGLevelOfDetailSwitchingDistance", "0");
            SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL12", "0");
            SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL23", "0");
            SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL34", "0");

            // Animation limits
            SetValue("DFIntMaxActiveAnimationTracks", "128");
            SetValue("DFIntPhysicsStepsPerFrame", "1");
        }
    }
}
