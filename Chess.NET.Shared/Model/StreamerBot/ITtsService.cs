namespace Chess.NET.Shared.Model.StreamerBot
{
    /// <summary>
    /// Interface abstraite pour la synthèse vocale du bot.
    /// Permet de supporter System.Speech (WPF), Web Speech API (Blazor),
    /// Azure Cognitive Services, ou tout autre moteur TTS.
    /// </summary>
    public interface ITtsService
    {
        /// <summary>
        /// Prononce le texte fourni et retourne une tâche qui se termine
        /// à la fin de la lecture, permettant au moteur vocal d'enchaîner
        /// les répliques séquentiellement.
        /// </summary>
        /// <param name="text">Texte à prononcer.</param>
        /// <param name="ct">Token permettant d'interrompre la lecture en cours.</param>
        Task SpeakAsync(string text, CancellationToken ct);
    }
}