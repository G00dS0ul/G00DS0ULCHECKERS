using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Media;
using System.Windows;

namespace G00DS0ULCHECKERS.ViewModel
{
    public class SoundManager
    {
        private SoundPlayer? _moveSound;
        private SoundPlayer? _captureSound;
        private SoundPlayer? _winSound;

        public SoundManager()
        {
            _moveSound = Load("move.wav");
            _captureSound = Load("capture.wav");
            _winSound = Load("win.wav");
        }

        private SoundPlayer? Load(string fileName)
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", fileName);

                if (File.Exists(path))
                {
                    var player = new SoundPlayer(path);
                    player.Load();
                    return player;
                }
            }
            catch 
            {
            }

            return null;
        }

        public void PlayMove() => Play(_moveSound);
        public void PlayCapture() => Play(_captureSound);
        public void PlayWin() => Play(_winSound);

        private void Play(SoundPlayer? player)
        {
            Task.Run(() =>
            {
                try
                {
                    player?.Play();
                }
                catch
                {
                }
            });
        }

    }
}
