namespace YSAutoPlayer.Core.Parser
{
    public interface IMusicScoreParser
    {
        Task<MusicScore> ParseAsync();
    }
}
