using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using YSAutoPlayer.Core;
using YSAutoPlayer.Core.Parser;

namespace YSAutoPlayer
{
    public enum ContinuousPlayMode
    {
        Repeat,
        Sequencial,
        Random
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        public class MusicScoreFile
        {
            public MusicScoreFile(string name, string path)
            {
                Name = name;
                Path = path;
            }

            public string Name { get; }
            public string Path { get; }
        }

        private const string MusicScoresFolder = "MusicScores";

        private HotKey? _hotKey;
        private CancellationTokenSource? cts;
        private readonly IPlayer _player = new KeyBoardPlayer();

        private ObservableCollection<MusicScoreFile>? _musicScoreFiles;
        private MusicScoreFile? _selectedMusicScoreFile;
        private MusicScore? _loadedMusicScore;
        private bool _isContinuousPlay = false;
        private ContinuousPlayMode _continuousPlayMode = ContinuousPlayMode.Sequencial;

        public ObservableCollection<MusicScoreFile>? MusicScoreFiles { get => _musicScoreFiles; private set => SetProperty(ref _musicScoreFiles, value); }
        public MusicScoreFile? SelectedMusicScore
        {
            get => _selectedMusicScoreFile; set
            {
                LoadMusicScore(value?.Path);
                SetProperty(ref _selectedMusicScoreFile, value);
            }
        }
        public MusicScore? LoadedMusicScore { get => _loadedMusicScore; set => SetProperty(ref _loadedMusicScore, value); }
        public bool IsContinuousPlay { get => _isContinuousPlay; set => SetProperty(ref _isContinuousPlay, value); }
        public IReadOnlyCollection<ContinuousPlayMode> ContinuousPlayModes { get; } = Enum.GetValues<ContinuousPlayMode>().Cast<ContinuousPlayMode>().ToList();
        public ContinuousPlayMode ContinuousPlayMode { get => _continuousPlayMode; set => SetProperty(ref _continuousPlayMode, value); }

        public MainWindow()
        {
            InitializeComponent();
            var handle = new WindowInteropHelper(this).Handle;
            Loaded += (s, e) =>
            {
                _hotKey = new HotKey(0x77, handle); // register P as  hotkey of play
                _hotKey.HotKeyPressed += k =>
                {
                    if (cts == null || cts.IsCancellationRequested)
                    {
                        cts = new CancellationTokenSource();
                        Task.Run(async () =>
                        {
                            if (IsContinuousPlay)
                            {
                                while (true)
                                {
                                    await PlayAsync(cts.Token);
                                    if (cts.IsCancellationRequested)
                                    {
                                        break;
                                    }
                                    if (ContinuousPlayMode == ContinuousPlayMode.Repeat)
                                    {
                                        continue;
                                    }
                                    else if (ContinuousPlayMode == ContinuousPlayMode.Sequencial)
                                    {
                                        if (MusicScoreFiles?.Any() ?? false && SelectedMusicScore != null)
                                        {
                                            var index = MusicScoreFiles.IndexOf(SelectedMusicScore!);
                                            if (index < MusicScoreFiles.Count - 1)
                                            {
                                                index++;
                                            }
                                            else
                                            {
                                                index = 0;
                                            }
                                            SelectedMusicScore = MusicScoreFiles[index];
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (MusicScoreFiles?.Any() ?? false)
                                        {
                                            var index = Random.Shared.Next(MusicScoreFiles.Count);
                                            SelectedMusicScore = MusicScoreFiles[index];
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    await Task.Delay(1000);
                                }
                            }
                            else
                            {
                                await PlayAsync(cts.Token);
                            }
                            cts = null;
                        }, cts.Token);
                    }
                    else
                    {
                        cts.Cancel();
                    }
                };
                LoadMusicScoreList(s, e);
            };
            Unloaded += (s, e) =>
            {
                _hotKey?.Dispose();
            };
        }
        private async Task PlayAsync(CancellationToken cancellationToken)
        {
            if (LoadedMusicScore == null)
            {
                return;
            }
            await _player.PlayAsync(LoadedMusicScore, cancellationToken);
        }

        private void LoadMusicScoreList(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(MusicScoresFolder))
            {
                Directory.CreateDirectory(MusicScoresFolder);
            }
            var files = Directory.GetFiles(MusicScoresFolder, "*.ysap");
            MusicScoreFiles = new ObservableCollection<MusicScoreFile>(files.Select(x => new MusicScoreFile(Path.GetFileNameWithoutExtension(x), x)));
        }

        private async void LoadMusicScore(string? filename)
        {
            try
            {
                if (filename == null)
                {
                    LoadedMusicScore = null;
                    return;
                }
                var parser = new YSAPFileScoreParser(filename);
                LoadedMusicScore = await parser.ParseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "加载YSAP文件失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
