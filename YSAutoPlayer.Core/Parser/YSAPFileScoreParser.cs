using System.Text.RegularExpressions;

namespace YSAutoPlayer.Core.Parser
{
    public class YSAPFileScoreParser : FileMusicScoreParser
    {
        public YSAPFileScoreParser(string filePath) : base(filePath)
        {
        }

        public override async Task<MusicScore> ParseAsync()
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(FilePath);
                lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                var title = lines[0];
                var beat = int.Parse(lines[1]);
                var trackCount = int.Parse(lines[2]);
                var musicScore = new MusicScore()
                {
                    Title = title,
                    Beat = beat
                };
                var tracks = new List<MusicTrack>(trackCount);
                for (int trackIndex = 0; trackIndex < trackCount; trackIndex++)
                {
                    var trackString = string.Join(',', lines.Skip(3).Where((l, i) => (i - trackIndex) % trackCount == 0));
                    trackString = Regex.Replace(trackString, @"\s", string.Empty);
                    var items = trackString.Split(',');
                    var track = new MusicTrack();
                    foreach (var item in items)
                    {
                        var match = Regex.Match(item, @"^(?<note>[01234567])(?<level>[`\.])?(\((?<meter>[\d\.]*)\))?$");
                        var note = match.Groups["note"].Value switch
                        {
                            "0" => Note.Zero,
                            "1" => Note.Do,
                            "2" => Note.Re,
                            "3" => Note.Mi,
                            "4" => Note.Fa,
                            "5" => Note.Sol,
                            "6" => Note.La,
                            "7" => Note.Si,
                            _ => throw new InvalidOperationException()
                        };
                        if (note != Note.Zero)
                        {
                            var level = match.Groups["level"].Value;
                            if (level == "`")
                            {
                                note += 7;
                            }
                            else if (level == ".")
                            {
                                note -= 7;
                            }
                        }
                        double meter = 1;
                        var meterString = match.Groups["meter"].Value;
                        if (!string.IsNullOrEmpty(meterString))
                        {
                            meter = double.Parse(meterString);
                        }
                        track.Add(note, meter);
                    }
                    tracks.Add(track);
                }
                musicScore.Tracks = tracks;
                return musicScore;
            }
            catch (Exception e)
            {
                throw new MusicScoreParseException($"解析YSAP文件失败，{e.Message}", e);
            }
        }
    }
}
