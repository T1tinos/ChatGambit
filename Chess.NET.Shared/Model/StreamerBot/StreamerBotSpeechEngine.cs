using Chess.NET.Shared.Model.StreamerBot;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.NET.Shared.Model.StreamerBot
{
    /// <summary>
    /// Moteur vocal principal du bot — point d'entrée unique pour toute prise de parole.
    /// S'abonne au <see cref="StreamerBotEventBus"/>, sélectionne les répliques via
    /// <see cref="StreamerBotRepliques"/>, les place dans la <see cref="StreamerBotSpeechQueue"/>
    /// et les consomme séquentiellement grâce à un verrou asynchrone.
    /// Toutes les dépendances spécifiques à la plateforme sont injectées via les interfaces
    /// <see cref="ITtsService"/>, <see cref="IStreamerBotChatService"/> et <see cref="IIdleTimerService"/>,
    /// ce qui rend ce moteur utilisable sans modification en WPF comme en Blazor.
    /// </summary>
    public class StreamerBotSpeechEngine : IDisposable
    {
        private readonly StreamerBotSpeechQueue _queue;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly StreamerBotRepliques _repliques;
        private readonly ITtsService _tts;
        private readonly IStreamerBotChatService _chat;
        private readonly IIdleTimerService _idleTimer;

        private CancellationTokenSource? _currentCts;
        private bool _isSpeaking;

        /// <summary>
        /// Initialise le moteur avec ses dépendances injectées.
        /// S'abonne au <see cref="StreamerBotEventBus"/> et démarre le timer idle.
        /// </summary>
        /// <param name="tts">Service de synthèse vocale spécifique à la plateforme.</param>
        /// <param name="chat">Service d'envoi de messages dans le chat de streaming.</param>
        /// <param name="idleTimer">Timer déclenchant le banter idle, spécifique à la plateforme.</param>
        /// <param name="repliques">
        /// Banque de répliques à utiliser. Si <c>null</c>, une instance par défaut est créée.
        /// Permet d'injecter une banque personnalisée ou mockée pour les tests.
        /// </param>
        public StreamerBotSpeechEngine(
            ITtsService tts,
            IStreamerBotChatService chat,
            IIdleTimerService idleTimer,
            StreamerBotRepliques? repliques = null)
        {
            _tts = tts;
            _chat = chat;
            _idleTimer = idleTimer;
            _repliques = repliques ?? new StreamerBotRepliques();
            _queue = new StreamerBotSpeechQueue();

            StreamerBotEventBus.EventFired += OnEventFired;
            _idleTimer.OnTick += () => StreamerBotEventBus.Emit(StreamerBotEventType.IdleBanter);
            _idleTimer.Start();
        }

        /// <summary>
        /// Handler appelé à chaque émission sur le <see cref="StreamerBotEventBus"/>.
        /// Sélectionne une réplique, déclenche une interruption si l'événement
        /// est CRITICAL, enfile la réplique puis lance le traitement asynchrone de la file.
        /// </summary>
        private void OnEventFired(object? sender, StreamerBotSpeechEvent evt)
        {
            var replique = _repliques.Pick(evt.Type, evt.Context);
            if (replique is null) return;

            var item = new StreamerBotSpeechItem { Text = replique, Priority = evt.Priority };

            if (evt.Priority == StreamerBotPriority.Critical && _isSpeaking)
                Interrupt();

            _queue.Enqueue(item);
            _ = ProcessQueueAsync();
        }

        /// <summary>
        /// Interrompt immédiatement la réplique en cours en annulant son token,
        /// puis vide toute la file pour que la prochaine réplique CRITICAL
        /// parte sans attendre les éléments précédents.
        /// </summary>
        private void Interrupt()
        {
            _currentCts?.Cancel();
            _queue.Clear();
        }

        /// <summary>
        /// Boucle de consommation asynchrone de la file.
        /// Protégée par un <see cref="SemaphoreSlim"/> pour n'avoir qu'un seul
        /// consommateur actif à la fois. Traite les répliques dans l'ordre de priorité
        /// jusqu'à épuisement de la file ou interruption par annulation.
        /// Appelée en fire-and-forget depuis <see cref="OnEventFired"/>.
        /// </summary>
        private async Task ProcessQueueAsync()
        {
            if (!await _lock.WaitAsync(0)) return;
            try
            {
                while (true)
                {
                    var next = _queue.Dequeue();
                    if (next is null) break;

                    _currentCts = new CancellationTokenSource();
                    _isSpeaking = true;
                    try
                    {
                        await SpeakAsync(next.Text, _currentCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    finally
                    {
                        _isSpeaking = false;
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Déclenche simultanément la synthèse vocale et l'envoi du texte dans le chat,
        /// puis attend que les deux soient terminés.
        /// Le token permet d'interrompre les deux opérations de concert.
        /// </summary>
        /// <param name="text">Texte de la réplique à prononcer et à publier.</param>
        /// <param name="ct">Token d'annulation lié à la réplique en cours.</param>
        private async Task SpeakAsync(string text, CancellationToken ct)
        {
            await Task.WhenAll(
                _tts.SpeakAsync(text, ct),
                _chat.SendAsync(text, ct)
            );
        }

        /// <summary>
        /// Libère les ressources : arrête le timer idle, se désabonne du bus
        /// et libère le <see cref="SemaphoreSlim"/>.
        /// À appeler à la fermeture de l'application.
        /// </summary>
        public void Dispose()
        {
            _idleTimer.Stop();
            _idleTimer.Dispose();
            StreamerBotEventBus.EventFired -= OnEventFired;
            _lock.Dispose();
        }
    }
}