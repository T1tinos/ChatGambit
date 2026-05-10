using Chess.NET.Shared.Model.StreamerBot;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.NET.Shared.Model.StreamerBot
{
    /// <summary>
    /// Banque de répliques du bot et logique de sélection contextuelle.
    /// Associe chaque <see cref="StreamerBotEventType"/> à une liste de phrases,
    /// garantit l'absence de répétition sur une fenêtre glissante configurable,
    /// et remplace les tokens de la forme <c>{PropName}</c> par les valeurs
    /// extraites de l'objet contextuel passé lors de l'émission de l'événement.
    /// </summary>
    public class StreamerBotRepliques
    {
        /// <summary>Générateur aléatoire pour la sélection des répliques candidates.</summary>
        private readonly Random _rng = new();

        /// <summary>
        /// Fenêtre anti-répétition par type d'événement : mémorise les N dernières
        /// répliques jouées pour éviter qu'une même phrase revienne trop vite.
        /// </summary>
        private readonly Dictionary<StreamerBotEventType, Queue<string>> _recentlyPlayed = new();

        /// <summary>
        /// Nombre de répliques mémorisées par type avant qu'une phrase
        /// puisse être rejouée. Augmenter cette valeur réduit les répétitions
        /// au prix d'un pool de candidats plus petit pour les types peu garnis.
        /// </summary>
        private const int NoRepeatWindow = 3;

        /// <summary>
        /// Banque statique des répliques, organisée par type d'événement.
        /// Les tokens de la forme <c>{Move}</c>, <c>{Captured}</c>, etc. sont remplacés
        /// dynamiquement par <see cref="ApplyContext"/> lors de la sélection.
        /// Enrichir cette banque est la principale façon d'étoffer la personnalité du bot.
        /// </summary>
        private static readonly Dictionary<StreamerBotEventType, List<string>> _bank = new()
        {
            [StreamerBotEventType.ChatMovePlayed] = new()
            {
                "Intéressant. Vous avez joué exactement ce que j'espérais.",
                "Oh, {Move}. Ma pièce préférée à capturer.",
                "Je note ce mouvement dans mes archives de l'incompétence.",
            },
            [StreamerBotEventType.BotMovePlayed] = new()
            {
                "Permettez-moi de vous démontrer comment cela se fait.",
                "{Move}. Vous pouvez pleurer maintenant, c'est autorisé.",
                "Calcul effectué en 0,003 secondes. Vous avez le reste de votre vie pour réfléchir.",
            },
            [StreamerBotEventType.Check] = new()
            {
                "Échec. Juste pour vous rappeler que je suis là.",
                "Votre roi semble... vulnérable. C'est délicieux.",
            },
            [StreamerBotEventType.Checkmate] = new()
            {
                "Échec et mat. Merci pour cette partie... éducative.",
                "Partie terminée. L'autopsie révèle une mort par erreurs répétées.",
            },
            [StreamerBotEventType.GameStart] = new()
            {
                "Nouvelle partie. Nouvelles erreurs à cataloguer. Commençons.",
                "Je vous souhaite bonne chance. Vous en aurez besoin.",
            },
            [StreamerBotEventType.GameEnd] = new()
            {
                "Partie archivée. Je la conserverai comme exemple de ce qu'il ne faut pas faire.",
            },
            [StreamerBotEventType.FirstVoteDelayed] = new()
            {
                "Je constate que vous délibérez. Prenez votre temps. J'en profite pour optimiser mes algorithmes.",
                "L'hésitation est humaine. L'erreur aussi. Je suis curieuse de voir laquelle vous choisirez.",
            },
            [StreamerBotEventType.ViewerJoined] = new()
            {
                "Un nouveau témoin. Bienvenue dans l'arène.",
                "Bienvenue. Vous arrivez juste à temps pour observer.",
            },
            [StreamerBotEventType.IdleBanter] = new()
            {
                "Le silence du chat est éloquent.",
                "Je recalcule. Vous, apparemment, vous réfléchissez.",
                "Mon évaluation de la position : désastreuse pour vous.",
            },
        };

        /// <summary>
        /// Sélectionne aléatoirement une réplique adaptée au type d'événement donné,
        /// en évitant les répétitions récentes et en substituant les tokens contextuels.
        /// Retourne <c>null</c> si aucune réplique n'est disponible pour ce type.
        /// </summary>
        /// <param name="type">Type d'événement pour lequel choisir une réplique.</param>
        /// <param name="context">
        /// Objet dont les propriétés publiques remplaceront les tokens <c>{PropName}</c>
        /// dans le texte sélectionné. Peut être <c>null</c>.
        /// </param>
        public string? Pick(StreamerBotEventType type, object? context)
        {
            if (!_bank.TryGetValue(type, out var pool) || pool.Count == 0)
                return null;

            if (!_recentlyPlayed.TryGetValue(type, out var recent))
            {
                recent = new Queue<string>();
                _recentlyPlayed[type] = recent;
            }

            var candidates = pool.Except(recent).ToList();
            if (candidates.Count == 0) candidates = pool;

            var picked = candidates[_rng.Next(candidates.Count)];

            recent.Enqueue(picked);
            if (recent.Count > NoRepeatWindow) recent.Dequeue();

            return ApplyContext(picked, context);
        }

        /// <summary>
        /// Remplace dans <paramref name="text"/> tous les tokens de la forme <c>{PropName}</c>
        /// par la valeur de la propriété correspondante de <paramref name="context"/>,
        /// via réflexion. Les tokens sans propriété correspondante sont laissés intacts.
        /// </summary>
        /// <param name="text">Texte de la réplique contenant éventuellement des tokens.</param>
        /// <param name="context">Objet source des valeurs de substitution, ou <c>null</c>.</param>
        /// <returns>Texte avec les tokens remplacés par leurs valeurs contextuelles.</returns>
        private static string ApplyContext(string text, object? context)
        {
            if (context is null) return text;

            foreach (var prop in context.GetType().GetProperties())
            {
                var value = prop.GetValue(context)?.ToString() ?? "";
                text = text.Replace($"{{{prop.Name}}}", value,
                    StringComparison.OrdinalIgnoreCase);
            }
            return text;
        }
    }
}
