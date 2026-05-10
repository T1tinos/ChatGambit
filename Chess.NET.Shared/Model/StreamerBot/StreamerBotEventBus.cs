using System;
using System.Collections.Generic;

namespace Chess.NET.Shared.Model.StreamerBot
{
    /// <summary>
    /// Définit les niveaux de priorité des événements du bot.
    /// Un événement de priorité supérieure peut interrompre ou court-circuiter
    /// les répliques de priorité inférieure en attente dans la file.
    /// </summary>
    public enum StreamerBotPriority { Low = 1, High = 2, Critical = 3 }

    /// <summary>
    /// Recense tous les moments clés du jeu auxquels le bot peut réagir.
    /// Chaque valeur correspond à une situation de jeu ou d'interaction
    /// avec le chat qui déclenchera la sélection et la lecture d'une réplique.
    /// </summary>
    public enum StreamerBotEventType
    {
        /// <summary>Le chat Twitch a validé et joué un coup aux échecs.</summary>
        ChatMovePlayed,
        /// <summary>Le bot a calculé et joué son propre coup.</summary>
        BotMovePlayed,
        /// <summary>Le roi du chat est en situation d'échec.</summary>
        Check,
        /// <summary>Le roi du chat est mat — fin de partie par victoire du bot.</summary>
        Checkmate,
        /// <summary>Une nouvelle partie vient de démarrer.</summary>
        GameStart,
        /// <summary>La partie vient de se terminer, quelle qu'en soit l'issue.</summary>
        GameEnd,
        /// <summary>Aucun vote n'est arrivé depuis le début de la fenêtre de vote.</summary>
        FirstVoteDelayed,
        /// <summary>Un nouveau viewer vient de rejoindre le stream.</summary>
        ViewerJoined,
        /// <summary>Aucun événement notable — le bot comble le silence.</summary>
        IdleBanter,
    }

    /// <summary>
    /// Représente un événement émis sur le bus, prêt à être consommé par le moteur vocal.
    /// Regroupe le type de l'événement, sa priorité résolue et le contexte
    /// contextuel (coup joué, pièce capturée, etc.) utile à la sélection de réplique.
    /// </summary>
    public class StreamerBotSpeechEvent
    {
        /// <summary>Nature de l'événement déclencheur.</summary>
        public StreamerBotEventType Type { get; init; }

        /// <summary>Priorité déterminée automatiquement d'après le type.</summary>
        public StreamerBotPriority Priority { get; init; }

        /// <summary>
        /// Données contextuelles libres (objet anonyme, record, etc.) transmises
        /// à <see cref="StreamerBotRepliques.Pick"/> pour personnaliser la réplique
        /// avec des tokens comme <c>{Move}</c> ou <c>{Captured}</c>.
        /// </summary>
        public object? Context { get; init; }
    }

    /// <summary>
    /// Bus d'événements statique et centralisé pour toute la couche bot.
    /// Découple les sources d'événements (logique de jeu, chat Twitch, timer idle)
    /// du moteur vocal qui les consomme, sans couplage direct entre les deux.
    /// N'importe quel composant peut émettre via <see cref="Emit"/> ;
    /// le <see cref="StreamerBotSpeechEngine"/> s'y abonne via <see cref="EventFired"/>.
    /// </summary>
    public static class StreamerBotEventBus
    {
        /// <summary>
        /// Table de correspondance entre chaque type d'événement et sa priorité fixe.
        /// Centralise la politique de priorité en un seul endroit pour faciliter
        /// les ajustements sans toucher au code des émetteurs.
        /// </summary>
        private static readonly Dictionary<StreamerBotEventType, StreamerBotPriority> _priorities = new()
        {
            { StreamerBotEventType.ChatMovePlayed,   StreamerBotPriority.High     },
            { StreamerBotEventType.BotMovePlayed,    StreamerBotPriority.High     },
            { StreamerBotEventType.Check,            StreamerBotPriority.Critical },
            { StreamerBotEventType.Checkmate,        StreamerBotPriority.Critical },
            { StreamerBotEventType.GameStart,        StreamerBotPriority.Critical },
            { StreamerBotEventType.GameEnd,          StreamerBotPriority.Critical },
            { StreamerBotEventType.FirstVoteDelayed, StreamerBotPriority.High     },
            { StreamerBotEventType.ViewerJoined,     StreamerBotPriority.Low      },
            { StreamerBotEventType.IdleBanter,       StreamerBotPriority.Low      },
        };

        /// <summary>
        /// Événement .NET déclenché chaque fois qu'un appelant invoque <see cref="Emit"/>.
        /// Le <see cref="StreamerBotSpeechEngine"/> s'y abonne pour recevoir tous les
        /// événements sans connaître leurs sources.
        /// </summary>
        public static event EventHandler<StreamerBotSpeechEvent>? EventFired;

        /// <summary>
        /// Émet un événement sur le bus en résolvant automatiquement sa priorité.
        /// À appeler depuis n'importe quel composant du jeu (logique Chess, chat, timer).
        /// </summary>
        /// <param name="type">Type de l'événement à émettre.</param>
        /// <param name="context">
        /// Objet contextuel optionnel (ex: <c>new { Move = "e4", Captured = "queen" }</c>)
        /// utilisé pour personnaliser la réplique choisie.
        /// </param>
        public static void Emit(StreamerBotEventType type, object? context = null)
        {
            var evt = new StreamerBotSpeechEvent
            {
                Type = type,
                Priority = _priorities[type],
                Context = context,
            };
            EventFired?.Invoke(null, evt);
        }
    }
}