namespace YSAutoPlayer.Core
{
    public interface IPlayer
    {
        Task PlayAsync(MusicScore musicScore, CancellationToken cancellationToken);
    }
}
