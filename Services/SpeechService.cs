namespace FoodApp.Services;

public static class SpeechService
{
    private static CancellationTokenSource? _currentSpeech;

    public static async Task SpeakAsync(string text)
    {
        Stop();

        _currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.85f,
            Pitch = 1.0f
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, _currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
            // Speech was cancelled
        }
    }

    public static void Stop()
    {
        _currentSpeech?.Cancel();
        _currentSpeech?.Dispose();
        _currentSpeech = null;
    }
}