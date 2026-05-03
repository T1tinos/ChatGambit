using System.Collections.Concurrent;
using System.Timers;

namespace Chess.NET.Shared.Model.Chat
{
    /// <summary>
    /// Gère la logique d'un tour de jeu orchestré par le chat.
    /// Plateforme-agnostique (Twitch, Discord, YouTube, etc.)
    /// Responsabilité unique: coordonner les votes, valider les coups, gérer les timers et le tour de jeu.
    /// </summary>
    public class ChatTurn
    {
        private readonly Game _game;
        private readonly ChatVoteCollector _voteCollector;
        private readonly IChatConnection _chatConnection;

        private const double MAX_TURN_DURATION = 60000; // 60 secondes en ms
        private const double REACTION_WINDOW = 30000;   // 30 secondes en ms

        private System.Timers.Timer? _turnTimer;
        private bool _isFirstVoteReceived = false;
        private bool _isTurnActive = false;

        /// <summary>
        /// Événement déclenché quand un tour se termine.
        /// </summary>
        public event Action? OnTurnEnded;

        /// <summary>
        /// Événement déclenché à la fin du vote.
        /// </summary>
        public event Action<ConcurrentDictionary<string, string>>? OnVoteClosed; // (liste des votes)

        public ChatTurn(Game game, ChatVoteCollector voteCollector, IChatConnection chatConnection)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _voteCollector = voteCollector ?? new ChatVoteCollector(_game);
            _chatConnection = chatConnection ?? throw new ArgumentNullException(nameof(chatConnection));

            // Abonner le collecteur de votes aux messages Twitch
            _chatConnection.OnMessageReceived += (args) =>
            {
                _voteCollector.ProcessMessage(args.DisplayName, args.Message);
            };

            // S'abonner aux votes collectés
            _voteCollector.OnVoteReceived += OnVoteReceived;
        }

        /// <summary>
        /// Démarre un nouveau tour de jeu et lance le timer.
        /// </summary>
        public void StartNewTurn()
        {
            _voteCollector.ResetVotes();
            _isFirstVoteReceived = false;
            _isTurnActive = true;

            if (_turnTimer == null)
            {
                // Utilisation du constructeur avec intervalle et initialisation simplifiée (évite l'avertissement IDE0017)
                _turnTimer = new System.Timers.Timer(MAX_TURN_DURATION)
                {
                    AutoReset = false // Ne s'exécute qu'une fois
                };
                _turnTimer.Elapsed += (s, e) => EndTurn();
            }
            else
            {
                // Mettre à jour l'intervalle si le Timer existait déjà
                _turnTimer.Interval = MAX_TURN_DURATION;
            }

            _turnTimer.Start();

            System.Diagnostics.Debug.WriteLine("[ChatTurn] Nouveau tour. Maximum 60 secondes pour voter.");
            _chatConnection.SendMessage("🎮 Nouveau tour! Vous avez 60 secondes pour voter. Envoyez un coup en notation UCI (ex: e2e4)");
        }

        /// <summary>
        /// Gère un vote reçu du collector de votes.
        /// Valide le coup dans le contexte actuel du jeu.
        /// </summary>
        private void OnVoteReceived(string username, string uciMove)
        {
            if (!_isTurnActive)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatTurn] Vote ignoré de {username} : aucun tour actif");
                return;
            }

            // Gestion dynamique du chrono
            if (!_isFirstVoteReceived)
            {
                _isFirstVoteReceived = true;

                // On réduit le temps restant à 30 secondes
                // On arrête et on redémarre le timer avec le nouveau délai court
                _turnTimer?.Stop();
                if (_turnTimer != null)
                {
                    _turnTimer.Interval = REACTION_WINDOW;
                    _turnTimer.Start();
                }

                System.Diagnostics.Debug.WriteLine("[ChatTurn] Premier vote valide reçu. Plus que 30 secondes !");
                _chatConnection.SendMessage($"✅ Premier vote reçu! Vous avez maintenant 30 secondes pour vous mettre d'accord.");
            }
        }

        /// <summary>
        /// Termine le tour actuel et déclenche l'événement OnTurnEnded.
        /// </summary>
        public void EndTurn()
        {
            _isTurnActive = false;
            _turnTimer?.Stop();
            OnVoteClosed?.Invoke(GetCurrentVotes());
            _voteCollector.ResetVotes();

            System.Diagnostics.Debug.WriteLine("[ChatTurn] Fin du temps imparti. Analyse des votes...");
            _chatConnection.SendMessage("⏱️ Fin du tour! Analyse des votes en cours...");

            // On déclenche l'événement pour que le ViewModel traite le coup gagnant
            OnTurnEnded?.Invoke();
        }

        /// <summary>
        /// Récupère une copie des votes actuels.
        /// </summary>
        public ConcurrentDictionary<string, string> GetCurrentVotes()
        {
            return new ConcurrentDictionary<string, string>(_voteCollector.CurrentVotes);
        }

        /// <summary>
        /// Récupère les votes et les réinitialise pour le prochain tour.
        /// </summary>
        public ConcurrentDictionary<string, string> GetAndResetVotes()
        {
            return _voteCollector.GetAndResetVotes();
        }

        /// <summary>
        /// Arrête le tour actuel prématurément et arrête le timer.
        /// </summary>
        public void StopTurn()
        {
            _isTurnActive = false;
            _turnTimer?.Stop();
            OnVoteClosed?.Invoke(GetCurrentVotes());
            _voteCollector.ResetVotes();
            System.Diagnostics.Debug.WriteLine("[ChatTurn] Tour arrêté prématurément");
        }

        /// <summary>
        /// Retourne l'état du tour actuel.
        /// </summary>
        public bool IsTurnActive => _isTurnActive;

        /// <summary>
        /// Retourne le nombre de votes actuels.
        /// </summary>
        public int GetVoteCount() => _voteCollector.GetVoteCount();
    }
}
