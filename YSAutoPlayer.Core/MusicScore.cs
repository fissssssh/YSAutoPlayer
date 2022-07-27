namespace YSAutoPlayer.Core
{
    /// <summary>
    /// 乐谱
    /// </summary>
    public class MusicScore
    {
        public string? Title { get; set; }
        public IEnumerable<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
    }

    /// <summary>
    /// 音轨
    /// </summary>
    public class MusicTrack : List<KeyValuePair<Note, int>>
    {
        public int Beat { get; }

        public MusicTrack(int beat)
        {
            Beat = beat;
        }

        public void Add(Note note, int meter) => Add(new KeyValuePair<Note, int>(note, meter));
    }
}
