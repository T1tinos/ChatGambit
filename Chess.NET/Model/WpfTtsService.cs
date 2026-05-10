using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using Chess.NET.Shared.Model.StreamerBot;

namespace Chess.NET.WPF.StreamerBot
{
    /// <summary>
    /// Implémentation WPF de <see cref="ITtsService"/> via <see cref="SpeechSynthesizer"/>.
    /// Utilise la synthèse vocale native Windows, disponible sans dépendance externe.
    /// </summary>
    public class WpfTtsService : ITtsService, IDisposable
    {
        private readonly SpeechSynthesizer _synth = new();

        /// <summary>
        /// Prononce le texte via <see cref="SpeechSynthesizer"/> de façon asynchrone.
        /// Surveille le token d'annulation pour interrompre la lecture si nécessaire.
        /// </summary>
        public async Task SpeakAsync(string text, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource();

            _synth.SpeakCompleted += Handler;
            _synth.SpeakAsync(text);

            await using var reg = ct.Register(() =>
            {
                _synth.SpeakAsyncCancelAll();
                tcs.TrySetCanceled();
            });

            await tcs.Task;
            return;

            void Handler(object? s, SpeakCompletedEventArgs e)
            {
                _synth.SpeakCompleted -= Handler;
                tcs.TrySetResult();
            }
        }

        /// <summary>Libère le <see cref="SpeechSynthesizer"/> sous-jacent.</summary>
        public void Dispose() => _synth.Dispose();
    }
}