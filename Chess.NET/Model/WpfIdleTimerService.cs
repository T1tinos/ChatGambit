using Chess.NET.Shared.Model.StreamerBot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace Chess.NET.Model
{
    /// <summary>
    /// Implémentation WPF de <see cref="IIdleTimerService"/> via <see cref="DispatcherTimer"/>.
    /// S'exécute sur le thread UI, ce qui évite tout marshalling manuel
    /// pour accéder aux éléments WPF depuis le callback.
    /// </summary>
    public class WpfIdleTimerService : IIdleTimerService
    {
        private readonly DispatcherTimer _timer;

        /// <inheritdoc/>
        public event Action? OnTick;

        /// <summary>
        /// Initialise le timer avec l'intervalle spécifié.
        /// </summary>
        /// <param name="interval">Durée entre deux déclenchements du banter idle.</param>
        public WpfIdleTimerService(TimeSpan interval)
        {
            _timer = new DispatcherTimer { Interval = interval };
            _timer.Tick += (_, _) => OnTick?.Invoke();
        }

        /// <inheritdoc/>
        public void Start() => _timer.Start();

        /// <inheritdoc/>
        public void Stop() => _timer.Stop();

        /// <inheritdoc/>
        public void Dispose() => _timer.Stop();
    }
}
