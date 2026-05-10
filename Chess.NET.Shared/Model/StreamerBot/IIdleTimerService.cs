using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.NET.Shared.Model.StreamerBot
{
    /// <summary>
    /// Interface abstraite pour le timer de banter idle du bot.
    /// Découple le moteur vocal du mécanisme de timer sous-jacent :
    /// <see cref="System.Windows.Threading.DispatcherTimer"/> en WPF,
    /// <see cref="System.Threading.PeriodicTimer"/> en Blazor ou worker service.
    /// </summary>
    public interface IIdleTimerService : IDisposable
    {
        /// <summary>
        /// Événement déclenché à chaque expiration du timer idle.
        /// Le <see cref="StreamerBotSpeechEngine"/> s'y abonne pour émettre
        /// un événement <see cref="StreamerBotEventType.IdleBanter"/>.
        /// </summary>
        event Action OnTick;

        /// <summary>Démarre le timer avec l'intervalle configuré.</summary>
        void Start();

        /// <summary>Arrête le timer sans le détruire.</summary>
        void Stop();
    }

}
