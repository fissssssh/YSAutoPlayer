namespace YSAutoPlayer.Core.Parser
{
    public abstract class FileMusicScoreParser : IMusicScoreParser
    {
        private readonly string _filePath;

        protected string FilePath { get => _filePath; }

        protected FileMusicScoreParser(string filePath)
        {
            _filePath = filePath;
        }

        public abstract Task<MusicScore> ParseAsync();
    }
}
