using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using YSAutoPlayer.Core;

namespace YSAutoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotKey? _hotKey;
        private CancellationTokenSource? cts;
        private readonly IPlayer _player = new KeyBoardPlayer();
        private MusicScore? _musicScore;
        public string? MusicScoreFile { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            var handle = new WindowInteropHelper(this).Handle;
            Loaded += (s, e) =>
            {
                _hotKey = new HotKey(80, handle); // register P as  hotkey of play
                _hotKey.HotKeyPressed += k =>
                {
                    if (cts == null || cts.IsCancellationRequested)
                    {
                        cts = new CancellationTokenSource();
                        Task.Run(async () =>
                        {
                            await PlayAsync(cts.Token);
                            cts = null;
                        }, cts.Token);
                    }
                    else
                    {
                        cts.Cancel();
                    }
                };
            };
            Unloaded += (s, e) =>
            {
                _hotKey?.Dispose();
            };
        }
        private async Task PlayAsync(CancellationToken cancellationToken)
        {
            if (_musicScore == null)
            {
                return;
            }
            await _player.PlayAsync(_musicScore, cancellationToken);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "文本文件|*.txt"
            };
            if (ofd.ShowDialog() ?? false)
            {
                var lines = await File.ReadAllLinesAsync(ofd.FileName);
                lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                var title = lines[0];
                var beat = int.Parse(lines[1]);
                var trackCount = int.Parse(lines[2]);
                _musicScore = new MusicScore()
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
                _musicScore.Tracks = tracks;
                MusicScoreFile = ofd.FileName;
            }
        }
    }
}
