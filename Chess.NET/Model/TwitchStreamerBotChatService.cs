using Chess.NET.Shared.Model.Chat;
using Chess.NET.Shared.Model.StreamerBot;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.NET.WPF.StreamerBot
{
    /// <summary>
    /// Implémentation de <see cref="IStreamerBotChatService"/> pour Twitch.
    /// Délègue l'envoi à <see cref="IChatConnection"/> déjà instanciée dans le projet,
    /// évitant toute duplication de la logique de connexion au chat.
    /// </summary>
    public class TwitchStreamerBotChatService : IStreamerBotChatService
    {
        private readonly IChatConnection _connection;

        /// <summary>
        /// Initialise le service avec la connexion Twitch active.
        /// </summary>
        /// <param name="connection">Connexion au chat Twitch déjà établie.</param>
        public TwitchStreamerBotChatService(IChatConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Envoie la réplique dans le canal Twitch via <see cref="IChatConnection.SendMessage"/>.
        /// Le token d'annulation est observé avant l'envoi pour éviter
        /// de publier une réplique déjà interrompue.
        /// </summary>
        public Task SendAsync(string message, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _connection.SendMessage(message);
            return Task.CompletedTask;
        }
    }
}