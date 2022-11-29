using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace Battleships
{
    internal class BattleshipSoundPlayer : SoundPlayer
    {
        SoundPlayer SoundPlayer;

        public void PlayBellSound()
        {
            SoundPlayer = new SoundPlayer(Properties.Resources.BellSound);
            SoundPlayer.Play();
        }

        public void PlayFireSound()
        {
            SoundPlayer = new SoundPlayer(Properties.Resources.BattleshipFireSound);
            SoundPlayer.Play();
        }
    }
}
