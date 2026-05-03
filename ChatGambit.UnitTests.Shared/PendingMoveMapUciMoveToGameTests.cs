using Xunit;
using Chess.NET.Shared.Model;
using Chess.NET.Shared.Model.Pieces;

namespace ChatGambit.UnitTests.Shared
{
    public class PendingMoveMapUciMoveToGameTests
    {
        /// <summary>
        /// Helper method to create a board with the standard starting position.
        /// </summary>
        private Board CreateDefaultBoard()
        {
            var board = new Board();
            board.Reset();
            return board;
        }

        /// <summary>
        /// Helper method to create a custom board with specific pieces.
        /// Useful for testing specific scenarios.
        /// </summary>
        private Board CreateEmptyBoard()
        {
            return new Board();
        }

        /// <summary>
        /// Tests a valid UCI move without promotion (4 characters).
        /// Example: "e2e4" - moving a pawn from e2 to e4
        /// Verifies that a PendingMove object is returned with correct piece and destination.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_ValidMove_NoPromotion_ReturnsPendingMove()
        {
            // Arrange
            var board = CreateDefaultBoard();
            var uci = "e2e4";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Piece);
            Assert.Equal("e4", result.To.ToString());
            Assert.Null(result.PromotionType);
        }

        /// <summary>
        /// Tests a valid UCI move with Queen promotion (5 characters).
        /// Example: "e7e8q" - pawn promotes to Queen
        /// Verifies that promotion type is correctly set to Queen.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_ValidMove_QueenPromotion_ReturnsPendingMoveWithPromotion()
        {
            // Arrange
            var board = CreateEmptyBoard();
            // Place a black pawn at e7 (source position)
            board.Pieces.Add(new Pawn(Position.Parse("e7"), Color.Black));
            var uci = "e7e8q";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PieceType.Queen, result.PromotionType);
            Assert.Equal("e8", result.To.ToString());
        }

        /// <summary>
        /// Tests a valid UCI move with Rook promotion (5 characters).
        /// Example: "a7a8r" - pawn promotes to Rook
        /// Verifies that promotion type is correctly set to Rook.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_ValidMove_RookPromotion_ReturnsPendingMoveWithPromotion()
        {
            // Arrange
            var board = CreateEmptyBoard();
            // Place a black pawn at a7 (source position)
            board.Pieces.Add(new Pawn(Position.Parse("a7"), Color.Black));
            var uci = "a7a8r";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PieceType.Rook, result.PromotionType);
            Assert.Equal("a8", result.To.ToString());
        }

        /// <summary>
        /// Tests a valid UCI move with Bishop promotion (5 characters).
        /// Example: "h7h8b" - pawn promotes to Bishop
        /// Verifies that promotion type is correctly set to Bishop.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_ValidMove_BishopPromotion_ReturnsPendingMoveWithPromotion()
        {
            // Arrange
            var board = CreateEmptyBoard();
            // Place a black pawn at h7 (source position)
            board.Pieces.Add(new Pawn(Position.Parse("h7"), Color.Black));
            var uci = "h7h8b";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PieceType.Bishop, result.PromotionType);
            Assert.Equal("h8", result.To.ToString());
        }

        /// <summary>
        /// Tests a valid UCI move with Knight promotion (5 characters).
        /// Example: "b7b8n" - pawn promotes to Knight
        /// Verifies that promotion type is correctly set to Knight.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_ValidMove_KnightPromotion_ReturnsPendingMoveWithPromotion()
        {
            // Arrange
            var board = CreateEmptyBoard();
            // Place a black pawn at b7 (source position)
            board.Pieces.Add(new Pawn(Position.Parse("b7"), Color.Black));
            var uci = "b7b8n";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PieceType.Knight, result.PromotionType);
            Assert.Equal("b8", result.To.ToString());
        }

        /// <summary>
        /// Tests that null is returned when no piece exists at the source position.
        /// The source position is empty, so the move is invalid.
        /// Example: "e4e5" - e4 is empty in a starting position
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_NoPieceAtSource_ReturnsNull()
        {
            // Arrange
            var board = CreateEmptyBoard();
            var uci = "e4e5"; // e4 is empty

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that null is returned when an invalid promotion character is used (5 characters).
        /// Valid promotion characters are: 'q' (Queen), 'r' (Rook), 'b' (Bishop), 'n' (Knight).
        /// Example: "e7e8x" - 'x' is invalid
        /// When the promotion character is invalid, the method should still return a PendingMove
        /// but with PromotionType set to null (as per the switch default case).
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_InvalidPromotionCharacter_ReturnsPendingMoveWithNullPromotion()
        {
            // Arrange
            var board = CreateEmptyBoard();
            // Place a black pawn at e7 (source position)
            board.Pieces.Add(new Pawn(Position.Parse("e7"), Color.Black));
            var uci = "e7e8x"; // 'x' is not a valid promotion character

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.PromotionType);
        }

        /// <summary>
        /// Tests that destination position is correctly parsed from UCI notation.
        /// Example: "g1f3" - Knight moves from g1 to f3
        /// Verifies that the destination position is correctly set.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_CorrectDestinationPosition_ReturnsCorrectTo()
        {
            // Arrange
            var board = CreateDefaultBoard();
            var uci = "g1f3";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("f3", result.To.ToString());
        }

        /// <summary>
        /// Tests that the piece is correctly extracted from the source position.
        /// The returned PendingMove should contain the correct piece object.
        /// Example: "e2e4" - the piece at e2 is a white pawn
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_CorrectSourcePiece_ReturnsCorrectPiece()
        {
            // Arrange
            var board = CreateDefaultBoard();
            var uci = "e2e4";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Piece);
            // The piece at e2 in a standard game is a white pawn
            Assert.Equal(Color.White, result.Piece.Color);
            Assert.Equal(PieceType.Pawn, result.Piece.Type);
        }

        /// <summary>
        /// Tests that null is returned for UCI notation with less than 4 characters.
        /// Example: "e2e" - only 3 characters
        /// The method should validate that UCI length is at least 4.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_UciLessThan4Characters_ReturnsNull()
        {
            // Arrange
            var board = CreateEmptyBoard();
            var uci = "e2e"; // Only 3 characters

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the source position is correctly parsed from UCI notation.
        /// Example: "g1f3" - the source is g1
        /// Verifies that the piece at the source location is correctly identified.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_CorrectSourcePosition_ReturnsCorrectPieceFromSource()
        {
            // Arrange
            var board = CreateDefaultBoard();
            var uci = "g1f3"; // Knight from g1 to f3

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            // g1 contains a white knight in starting position
            Assert.Equal(PieceType.Knight, result.Piece.Type);
            Assert.Equal(Color.White, result.Piece.Color);
        }

        /// <summary>
        /// Tests a black pawn move.
        /// Example: "e7e5" - black pawn moves from e7 to e5
        /// Verifies that the piece color is correct for black's pieces.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_BlackPawnMove_ReturnsBlackPawnPendingMove()
        {
            // Arrange
            var board = CreateDefaultBoard();
            var uci = "e7e5";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Piece);
            Assert.Equal(Color.Black, result.Piece.Color);
            Assert.Equal(PieceType.Pawn, result.Piece.Type);
            Assert.Equal("e5", result.To.ToString());
        }

        /// <summary>
        /// Tests that the method handles null board parameter gracefully.
        /// While this is an edge case, it ensures robustness.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_NullBoard_ThrowsException()
        {
            // Arrange
            var uci = "e2e4";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => PendingMove.MapUciMoveToGame(uci, null!));
        }

        /// <summary>
        /// Tests that the method handles null or empty UCI string gracefully.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_NullUci_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => PendingMove.MapUciMoveToGame(null!, CreateEmptyBoard()));
        }

        /// <summary>
        /// Tests that the method handles empty UCI string gracefully.
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_EmptyUci_ReturnsNull()
        {
            // Arrange
            var board = CreateEmptyBoard();
            var uci = "";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests multiple valid knight moves to ensure the method works for different piece types.
        /// Example: "b1c3" - knight moves from b1 to c3
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_KnightMove_ReturnsCorrectKnightPendingMove()
        {
            // Arrange
            var board = CreateDefaultBoard();
            var uci = "b1c3";

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Piece);
            Assert.Equal(PieceType.Knight, result.Piece.Type);
            Assert.Equal(Color.White, result.Piece.Color);
            Assert.Equal("c3", result.To.ToString());
        }

        /// <summary>
        /// Tests that all four valid promotion characters are handled correctly.
        /// This test ensures the switch statement covers all expected cases.
        /// </summary>
        [Theory]
        [InlineData("e7e8q", PieceType.Queen)]
        [InlineData("e7e8r", PieceType.Rook)]
        [InlineData("e7e8b", PieceType.Bishop)]
        [InlineData("e7e8n", PieceType.Knight)]
        public void MapUciMoveToGame_AllValidPromotionTypes_ReturnsCorrectPromotionType(string uci, PieceType expectedPromotion)
        {
            // Arrange
            var board = CreateEmptyBoard();
            // Place a black pawn at e7 (source position)
            board.Pieces.Add(new Pawn(Position.Parse("e7"), Color.Black));

            // Act
            var result = PendingMove.MapUciMoveToGame(uci, board);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPromotion, result.PromotionType);
        }

        /// <summary>
        /// Tests that invalid UCI format with incorrect position characters returns null.
        /// Example: "e2z4" - 'z' is not a valid file character
        /// </summary>
        [Fact]
        public void MapUciMoveToGame_InvalidPositionCharacter_ThrowsException()
        {
            // Arrange
            var board = CreateEmptyBoard();
            var uci = "e2z4"; // 'z' is not a valid file character

            // Act & Assert
            Assert.Throws<ArgumentException>(() => PendingMove.MapUciMoveToGame(uci, board));
        }
    }
}
