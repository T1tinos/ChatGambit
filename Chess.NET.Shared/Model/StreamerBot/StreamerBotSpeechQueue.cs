using Chess.NET.Shared.Model.StreamerBot;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Text;

namespace Chess.NET.Shared.Model.StreamerBot
{
    /// <summary>
    /// Unité de travail placée dans la file d'attente vocale.
    /// Contient le texte de la réplique à prononcer ainsi que sa priorité,
    /// utilisée pour déterminer l'ordre de traitement et les règles de suppression.
    /// </summary>
    public class StreamerBotSpeechItem
    {
        /// <summary>Texte final de la réplique, après substitution des tokens contextuels.</summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>Priorité héritée de l'événement déclencheur.</summary>
        public StreamerBotPriority Priority { get; init; }
    }

    /// <summary>
    /// File d'attente à trois niveaux de priorité pour les répliques du bot.
    /// Garantit qu'un événement CRITICAL est toujours traité en premier,
    /// qu'un événement LOW est ignoré si la file est déjà occupée,
    /// et qu'un événement CRITICAL purge les files inférieures pour prendre la tête.
    /// Utilisée exclusivement par <see cref="StreamerBotSpeechEngine"/>.
    /// </summary>
    public class StreamerBotSpeechQueue
    {
        /// <summary>File dédiée aux répliques de priorité maximale (échec, mat, début/fin de partie).</summary>
        private readonly Queue<StreamerBotSpeechItem> _critical = new();

        /// <summary>File dédiée aux répliques importantes mais non urgentes (coups joués, votes tardifs).</summary>
        private readonly Queue<StreamerBotSpeechItem> _high = new();

        /// <summary>File dédiée au banter de remplissage, ignorée si d'autres répliques attendent.</summary>
        private readonly Queue<StreamerBotSpeechItem> _low = new();

        /// <summary>
        /// Nombre total de répliques en attente dans toutes les files confondues.
        /// Utilisé pour décider si une réplique LOW doit être acceptée ou ignorée.
        /// </summary>
        public int TotalCount => _critical.Count + _high.Count + _low.Count;

        /// <summary>
        /// Ajoute une réplique dans la file correspondant à sa priorité,
        /// en appliquant les règles de suppression propres à chaque niveau :
        /// <list type="bullet">
        ///   <item><term>CRITICAL</term><description>Purge HIGH et LOW, puis s'insère.</description></item>
        ///   <item><term>HIGH</term><description>S'insère normalement.</description></item>
        ///   <item><term>LOW</term><description>Ignorée si la file globale n'est pas vide.</description></item>
        /// </list>
        /// </summary>
        /// <param name="item">La réplique à mettre en file.</param>
        public void Enqueue(StreamerBotSpeechItem item)
        {
            switch (item.Priority)
            {
                case StreamerBotPriority.Critical:
                    _high.Clear();
                    _low.Clear();
                    _critical.Enqueue(item);
                    break;

                case StreamerBotPriority.High:
                    _high.Enqueue(item);
                    break;

                case StreamerBotPriority.Low:
                    if (TotalCount == 0)
                        _low.Enqueue(item);
                    break;
            }
        }

        /// <summary>
        /// Retire et retourne la prochaine réplique à jouer,
        /// en respectant l'ordre de priorité décroissant : CRITICAL → HIGH → LOW.
        /// Retourne <c>null</c> si toutes les files sont vides.
        /// </summary>
        public StreamerBotSpeechItem? Dequeue()
        {
            if (_critical.Count > 0) return _critical.Dequeue();
            if (_high.Count > 0) return _high.Dequeue();
            if (_low.Count > 0) return _low.Dequeue();
            return null;
        }

        /// <summary>
        /// Vide intégralement les trois files.
        /// Appelé lors d'une interruption CRITICAL pour que la réplique urgente
        /// parte sans délai dès la fin de l'interruption.
        /// </summary>
        public void Clear()
        {
            _critical.Clear();
            _high.Clear();
            _low.Clear();
        }
    }
}
