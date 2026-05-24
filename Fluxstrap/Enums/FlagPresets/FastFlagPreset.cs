namespace Fluxstrap.Enums.FlagPresets
{
    public enum FastFlagPreset
    {
        [EnumName(StaticName = "Off")]
        Off,
        [EnumName(FromTranslation = "Enums.FlagPresets.FastFlagPreset.Performance")]
        Performance,
        [EnumName(FromTranslation = "Enums.FlagPresets.FastFlagPreset.Quality")]
        Quality,
        [EnumName(FromTranslation = "Enums.FlagPresets.FastFlagPreset.Competitive")]
        Competitive,
        [EnumName(StaticName = "Ultimate Performance")]
        UltimatePerformance,
        [EnumName(StaticName = "Extreme Performance")]
        ExtremePerformance
    }
}
