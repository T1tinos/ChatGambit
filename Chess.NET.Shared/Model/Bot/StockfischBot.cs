using Chess.NET.Shared.Model;
using System.Diagnostics;
using System.IO;

namespace Chess.NET.Shared.Model.Bot
{
    public class StockfischBot : IChessBot, IDisposable
    {
        private readonly Process _process;
        private readonly StreamWriter _input;
        private readonly StreamReader _output;

        private readonly int skill = 0;
        private readonly int depth = 0;

        public int Elo => 100 + depth * 350 + skill * 80;

        public string Name => "Stockfish";

        public StockfischBot(int skill = 8, int depth = 10, string stockfishPath = "stockfish.exe")
        {
            this.skill = skill;
            this.depth = depth;

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = stockfishPath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _process.Start();

            _input = _process.StandardInput;
            _output = _process.StandardOutput;

            InitUci();
        }

        private void InitUci()
        {
            Send("uci");
            WaitFor("uciok");

            Send("isready");
            WaitFor("readyok");

            Send($"setoption name Skill Level value {skill}");
        }

        public PendingMove? Move(Game game)
        {
            // 1️ Définir la position (via les coups!)
            var movesUci = string.Join(" ", game.Moves.Select(m => m.ToUci()));
            Send($"position startpos moves {movesUci}");

            // 2️ Laisser le moteur calculer
            Send($"go depth {depth}");

            // 3️ Lire le meilleur coup
            string? line;
            while ((line = _output.ReadLine()) != null)
            {
                Debug.WriteLine($"[Stockfish] {line}");
                if (line.StartsWith("bestmove"))
                {
                    var parts = line.Split(' ');
                    if (parts.Length < 2 || parts[1] == "(none)")
                        return null; // Échec et mat ou pat

                    string uciMove = parts[1]; // ex. e2e4
                    return PendingMove.MapUciMoveToGame(uciMove, game.Board);
                }
            }

            return null;
        }

        private void Send(string command)
        {
            _input.WriteLine(command);
            _input.Flush();
        }

        private void WaitFor(string token)
        {
            string? line;
            while ((line = _output.ReadLine()) != null)
            {
                if (line.Contains(token))
                    return;
            }
        }

        public void Dispose()
        {
            try
            {
                Send("quit");
                _process.Kill();
            }
            catch { }
        }
    }
}
