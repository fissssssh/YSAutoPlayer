using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YSAutoPlayer.Core;
using YSAutoPlayer.Core.Parser;
using YSAutoPlayer.Extensions;

namespace YSAutoPlayer
{
    public enum ContinuousPlayMode
    {
        [Description("循环")]
        Repeat,
        [Description("顺序")]
        Sequencial,
        [Description("随机")]
        Random
    }

    public class MainWindowViewModel : ObservableObject
    {
        public class SelectionItem<T>
        {
            public SelectionItem(string name, T value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }
            public T Value { get; set; }
        }
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
        public IReadOnlyCollection<SelectionItem<ContinuousPlayMode>> ContinuousPlayModes { get; } = Enum.GetValues<ContinuousPlayMode>().Select(x => new SelectionItem<ContinuousPlayMode>(x.Description(), x)).ToList();
        public ContinuousPlayMode ContinuousPlayMode { get => _continuousPlayMode; set => SetProperty(ref _continuousPlayMode, value); }
        public IRelayCommand LoadMusicScoreListCommand { get; }
        public MainWindowViewModel()
        {
            LoadMusicScoreListCommand = new RelayCommand(LoadMusicScoreList);
            LoadMusicScoreList();
        }
        private async Task PlayAsync(CancellationToken cancellationToken)
        {
            if (LoadedMusicScore == null)
            {
                return;
            }
            await _player.PlayAsync(LoadedMusicScore, cancellationToken);
        }

        private void LoadMusicScoreList()
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

        public void RegisterHotkey(IntPtr handle)
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
        }

        public void UnregisterHotkey()
        {
            _hotKey?.Dispose();
        }
    }
}
