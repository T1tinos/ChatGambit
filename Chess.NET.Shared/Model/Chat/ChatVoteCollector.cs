using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Chess.NET.Shared.Model.Chat
{
    /// <summary>
    /// Collecte et valide les votes en notation UCI provenant du chat.
    /// Plateforme-agnostique (Twitch, Discord, YouTube, etc.)
    /// Responsabilité unique: extraire les notations UCI des messages et les stocker.
    /// </summary>
    public class ChatVoteCollector
    {
        private readonly Game _game;

        const char _commandPrefix = '!'; // Si on veut exiger un préfixe pour les commandes de vote (ex: "!e2e4"), sinon on peut le retirer

        // Regex pour capturer la notation UCI (ex: "e2e4", "g8f6", ou "e7e8q" pour une promotion)
        // Le \b assure qu'on ne prend que des mots exacts, RegexOptions.IgnoreCase permet d'accepter "E2E4"
        //private static readonly Regex _uciRegex = new Regex(@$"\b{_commandPrefix}[a-h][1-8][a-h][1-8][qrbn]?\b", RegexOptions.IgnoreCase);
        private static readonly Regex _uciRegex = new Regex(@$"{_commandPrefix}[a-h][1-8][a-h][1-8][qrbn]?\b", RegexOptions.IgnoreCase);

        /// <summary>
        /// Dictionnaire thread-safe contenant les votes actuels.
        /// Clé: nom d'utilisateur, Valeur: notation UCI du coup voté
        /// </summary>
        public ConcurrentDictionary<string, string> CurrentVotes { get; private set; }

        /// <summary>
        /// Événement déclenché quand un vote valide est reçu.
        /// </summary>
        public event Action<string, string>? OnVoteReceived; // (username, uciMove)

        public ChatVoteCollector(Game game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            CurrentVotes = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// Traite un message provenant du chat et en extrait la notation UCI si présente.
        /// Vérifie que le coup est valide et l'enregistre dans le dictionnaire des votes le cas échéant.
        /// </summary>
        /// <param name="username">Nom d'utilisateur du chat</param>
        /// <param name="message">Contenu du message</param>
        /// <returns>La notation UCI extraite (ex: "e2e4"), ou null si aucune n'est trouvée ou n'est pas valide</returns>
        public string? ProcessMessage(string username, string message)
        {
            // On cherche si le message contient une notation UCI
            Match match = _uciRegex.Match(message);

            if (match.Success)
            {
                // Si la regex trouve une correspondance, on normalise en minuscules (ex: E2E4 -> e2e4)
                string uciMove = match.Value.ToLower().TrimStart(_commandPrefix);

                // Valider le coup dans le contexte du jeu actuel
                PendingMove? move = PendingMove.MapUciMoveToGame(uciMove, _game.Board);

                if (move == null || !_game.IsMoveValid(move.Piece, move.To))
                {
                    // Le coup n'est pas valide dans le contexte actuel du jeu, on ignore ce vote
                    System.Diagnostics.Debug.WriteLine($"[ChatTurn] Vote invalide reçu de {username} : {uciMove}");
                    return null;
                }

                // On utilise le nom d'affichage comme clé. 
                // Si l'utilisateur a déjà voté, sa valeur est écrasée par son nouveau choix.
                CurrentVotes.AddOrUpdate(username, uciMove, (key, oldValue) => uciMove);

                System.Diagnostics.Debug.WriteLine($"[ChatVoteCollector] Vote extrait : {username} -> {uciMove}");

                // Déclencher l'événement pour notifier les observateurs
                OnVoteReceived?.Invoke(username, uciMove);

                return uciMove;
            }

            return null;
        }

        /// <summary>
        /// Récupère une copie des votes actuels et vide le dictionnaire.
        /// Utilisé à la fin d'un tour pour récupérer les votes et réinitialiser pour le prochain tour.
        /// </summary>
        public ConcurrentDictionary<string, string> GetAndResetVotes()
        {
            // On copie les votes actuels pour les renvoyer
            var votesSnapshot = new ConcurrentDictionary<string, string>(CurrentVotes);

            // On vide le dictionnaire pour le prochain tour
            CurrentVotes.Clear();

            System.Diagnostics.Debug.WriteLine($"[ChatVoteCollector] {votesSnapshot.Count} vote(s) récupéré(s) et dictionnaire réinitialisé");

            return votesSnapshot;
        }

        /// <summary>
        /// Réinitialise les votes actuels sans les retourner.
        /// </summary>
        public void ResetVotes()
        {
            CurrentVotes.Clear();
            System.Diagnostics.Debug.WriteLine("[ChatVoteCollector] Votes réinitialisés");
        }

        /// <summary>
        /// Retourne le nombre de votes actuels.
        /// </summary>
        public int GetVoteCount() => CurrentVotes.Count;
    }
}
