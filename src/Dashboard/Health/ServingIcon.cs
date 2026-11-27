namespace Dashboard.Health;

/// <summary>Holds the serving icon's SVG path per <see cref="ServingState"/>.</summary>
public static class ServingIcon
{
    /// <summary>Holds the serving icon's SVG path per <see cref="ServingState"/>.</summary>
    public static string PathData(ServingState state) => state switch
    {
        ServingState.Primary => "M3 8.5l3.2 3.2L13 4.6",
        ServingState.FailedOver => "M2 5.5h11M10 2.5l3 3-3 3M14 10.5H3M6 7.5l-3 3 3 3",
        ServingState.Down => "M4 4l8 8M12 4l-8 8",
        ServingState.Pending => "M3.9 8h.2M7.9 8h.2M11.9 8h.2",
        ServingState.NoNodes => "M3.9 8h.2M7.9 8h.2M11.9 8h.2",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };
}
