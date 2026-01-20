public static class GameSession
{
    // Player-specific selections for multiplayer
    public static SMFile SelectedSongP1;
    public static SMChart SelectedChartP1;

    public static SMFile SelectedSongP2;
    public static SMChart SelectedChartP2;

    // Backwards-compatibility singleplayer accessors (map to player 1)
    public static SMFile SelectedSong
    {
        get => SelectedSongP1;
        set => SelectedSongP1 = value;
    }

    public static SMChart SelectedChart
    {
        get => SelectedChartP1;
        set => SelectedChartP1 = value;
    }
}
