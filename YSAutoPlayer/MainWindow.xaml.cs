using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using YSAutoPlayer.Core;
using YSAutoPlayer.Core.Parser;

namespace YSAutoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        private HotKey? _hotKey;
        private CancellationTokenSource? cts;
        private readonly IPlayer _player = new KeyBoardPlayer();
        private MusicScore? _musicScore;
        private string? _musicScoreFile;
        private string? _musicScoreTitle;
        private int? _musicScoreBeat;
        private int? _musicScoreTracksCount;

        public string? MusicScoreFile { get => _musicScoreFile; private set => SetProperty(ref _musicScoreFile, value); }
        public string? MusicScoreTitle { get => _musicScoreTitle; private set => SetProperty(ref _musicScoreTitle, value); }
        public int? MusicScoreBeat { get => _musicScoreBeat; private set => SetProperty(ref _musicScoreBeat, value); }
        public int? MusicScoreTracksCount { get => _musicScoreTracksCount; private set => SetProperty(ref _musicScoreTracksCount, value); }

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
                Filter = "YSAP 文件|*.ysap"
            };
            if (ofd.ShowDialog() ?? false)
            {
                try
                {
                    var parser = new YSAPFileScoreParser(ofd.FileName);
                    _musicScore = await parser.ParseAsync();
                    MusicScoreFile = ofd.FileName;
                    MusicScoreTitle = _musicScore.Title;
                    MusicScoreBeat = _musicScore.Beat;
                    MusicScoreTracksCount = _musicScore.Tracks.Count;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "加载YSAP文件失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
