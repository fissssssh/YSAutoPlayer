namespace YSAutoPlayer.Core
{
    /// <summary>
    /// 乐谱
    /// </summary>
    public class MusicScore
    {
        public string? Title { get; set; }
        public int Beat { get; set; }
        public IEnumerable<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
    }

    /// <summary>
    /// 音轨
    /// </summary>
    public class MusicTrack : List<KeyValuePair<Note, double>>
    {
        public void Add(Note note, double meter) => Add(new KeyValuePair<Note, double>(note, meter));
    }
}
