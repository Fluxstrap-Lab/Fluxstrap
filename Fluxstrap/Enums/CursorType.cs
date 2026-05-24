namespace Fluxstrap.Enums
{
    public enum CursorType
    {
        [EnumSort(Order = 1)]
        [EnumName(FromTranslation = "Common.Default")]
        Default,

        [EnumSort(Order = 3)]
        From2006,

        [EnumSort(Order = 2)]
        From2013,

        [EnumSort(Order = 4)]
        From2006DecalDrag,

        [EnumSort(Order = 5)]
        From2013DecalDrag,

        [EnumSort(Order = 6)]
        FPSCursor,

        [EnumSort(Order = 7)]
        FPSCursorDecalDrag,

        [EnumSort(Order = 8)]
        DotCursor,

        [EnumSort(Order = 9)]
        DotCursorDecalDrag,

        [EnumSort(Order = 10)]
        StoofsCursor,

        [EnumSort(Order = 11)]
        StoofsCursorDecalDrag,

        [EnumSort(Order = 12)]
        CleanCursor,

        [EnumSort(Order = 13)]
        CleanCursorDecalDrag,

        [EnumSort(Order = 14)]
        WhiteDotCursor,

        [EnumSort(Order = 15)]
        WhiteDotCursorDecalDrag,

        [EnumSort(Order = 16)]
        VerySmallWhiteDot,

        [EnumSort(Order = 17)]
        VerySmallWhiteDotDecalDrag
    }
}
