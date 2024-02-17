using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public static class SoundPlayerHelper
{
    private static Task _playbackTask;
    private static CancellationTokenSource _cancellationTokenSource;
    private static readonly string RelativePath = @"Resources\converted_Siren.wav";
    private static readonly string FullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, RelativePath);

    public static void PlaySoundRepeatedly()
    {
        if (_playbackTask == null || _playbackTask.IsCompleted)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _playbackTask = Task.Run(() =>
            {
                var player = new SoundPlayer(FullPath);
                try
                {
                    player.Load(); // Attempt to load the sound file in advance
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading sound file: {ex.Message}");
                    return; // Exit if cannot load the file
                }

                while (!token.IsCancellationRequested)
                {
                    player.PlaySync(); // This will play the sound once; the loop will cause it to repeat
                }
            }, token);
        }
    }

    public static void StopSound()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null; // Dispose of the CancellationTokenSource if needed
    }
}
