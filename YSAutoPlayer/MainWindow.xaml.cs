using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
            var handle = new WindowInteropHelper(this).Handle;
            Loaded += (s, e) =>
            {
                _hotKey = new HotKey(80, handle); // register P as  hotkey of play
                _hotKey.HotKeyPressed += k =>
                {
                    if (cts == null)
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
            await _player.PlayAsync(new MusicScore
            {
                Title = "TEST",
                Tracks = new List<MusicTrack> {
                    new MusicTrack(240) {
                        {Note.Do,2 },
                        {Note.Re,1 },
                        {Note.Mi,4 },
                        {Note.Fa,1 },
                        {Note.Sol,1 },
                        {Note.La,1 },
                        {Note.Si,1 },
                    },
                    new MusicTrack(240) {
                        {Note.DoHigh,2 },
                        {Note.ReHigh,1 },
                        {Note.MiHigh,4 },
                        {Note.FaHigh,1 },
                        {Note.SolHigh,1 },
                        {Note.LaHigh,1 },
                        {Note.SiHigh,1 },
                    },
                    new MusicTrack(240) {
                        {Note.DoLow,2 },
                        {Note.ReLow,1 },
                        {Note.MiLow,4 },
                        {Note.FaLow,1 },
                        {Note.SolLow,1 },
                        {Note.LaLow,1 },
                        {Note.SiLow,1 },
                    }
                }
            }, cancellationToken);
        }
    }
}
