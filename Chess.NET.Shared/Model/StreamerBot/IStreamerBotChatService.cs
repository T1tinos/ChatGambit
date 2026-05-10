using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.NET.Shared.Model.StreamerBot
{
    /// <summary>
    /// Interface abstraite pour l'envoi de messages du bot dans le chat.
    /// Découple le moteur vocal de la plateforme de streaming (Twitch, YouTube, Discord…)
    /// et réutilise le contrat déjà établi par <see cref="IChatConnection"/>
    /// pour l'envoi de messages sortants spécifiques au bot.
    /// </summary>
    public interface IStreamerBotChatService
    {
        /// <summary>
        /// Envoie une réplique du bot dans le canal de chat actif.
        /// </summary>
        /// <param name="message">Texte de la réplique à publier.</param>
        /// <param name="ct">Token permettant d'annuler l'envoi si le bot est interrompu.</param>
        Task SendAsync(string message, CancellationToken ct);
    }
}
