public static class TimingWindows
{
    public static float Marvelous { get { return TimingWindows.kerroin * 0.033f; } }
    public static float Perfect { get { return TimingWindows.kerroin * 0.066f; } }
    public static float Great { get { return TimingWindows.kerroin * 0.1f; } }
    public static float Good { get { return TimingWindows.kerroin * 0.140f; } }
    public static float Bad { get { return TimingWindows.kerroin * 0.180f; } }

    private static float kerroin = 1;

    public static void setMultiplier(float kerroin)
    {
        if (kerroin > 0)
        {
            TimingWindows.kerroin = kerroin;
        }
    }
}
