using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace YSAutoPlayer.Core.Test
{
    public class KeyBoardPlayerTest
    {
        [Fact]
        public async Task PlayTest()
        {
            IPlayer player = new KeyBoardPlayer();
            await player.PlayAsync(new MusicScore
            {
                Title = "TEST",
                Tracks = new List<MusicTrack> {
                    new MusicTrack(240) {
                        {Note.Do,1 },
                        {Note.Re,1 },
                        {Note.Mi,1 },
                        {Note.Fa,1 },
                        {Note.Sol,1 },
                        {Note.La,1 },
                        {Note.Si,1 },
                    }
                }
            },CancellationToken.None);
        }
    }
}