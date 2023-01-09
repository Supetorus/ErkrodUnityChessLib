using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityChess {
	/// <summary>An 8x8 matrix representation of a chessboard.</summary>
	public class Board {
		private readonly Piece[,] boardMatrix;
		private readonly Dictionary<Side, Square?> currentKingSquareBySide = new Dictionary<Side, Square?> {
			[Side.White] = null,
			[Side.Black] = null
		};

		public Piece this[Square position] {
			get {
				if (position.IsValid()) return boardMatrix[position.File - 1, position.Rank - 1];
				throw new ArgumentOutOfRangeException($"Position was out of range: {position}");
			}

			set {
				if (position.IsValid()) boardMatrix[position.File - 1, position.Rank - 1] = value;
				else throw new ArgumentOutOfRangeException($"Position was out of range: {position}");
			}
		}

		public Piece this[int file, int rank] {
			get => this[new Square(file, rank)];
			set => this[new Square(file, rank)] = value;
		}

		/// <summary>Creates a Board given the passed square-piece pairs.</summary>
		public Board(params (Square, Piece)[] squarePiecePairs) {
			boardMatrix = new Piece[8, 8];
			
			foreach ((Square position, Piece piece) in squarePiecePairs) {
				this[position] = piece;
			}
		}

		/// <summary>Creates a deep copy of the passed Board.</summary>
		public Board(Board board) {
			// TODO optimize this method
			// Creates deep copy (makes copy of each piece and deep copy of their respective ValidMoves lists) of board (list of BasePiece's)
			// this may be a memory hog since each Board has a list of Piece's, and each piece has a list of Movement's
			// avg number turns/Board's per game should be around ~80. usual max number of pieces per board is 32
			boardMatrix = new Piece[8, 8];
			for (int file = 1; file <= 8; file++) {
				for (int rank = 1; rank <= 8; rank++) {
					Piece pieceToCopy = board[file, rank];
					if (pieceToCopy == null) { continue; }

					this[file, rank] = pieceToCopy.DeepCopy();
				}
			}
		}

		public void ClearBoard() {
			for (int file = 1; file <= 8; file++) {
				for (int rank = 1; rank <= 8; rank++) {
					this[file, rank] = null;
				}
			}

			currentKingSquareBySide[Side.White] = null;
			currentKingSquareBySide[Side.Black] = null;
		}

		public static readonly (Square, Piece)[] StartingPositionPieces = {
			(new Square("a1"), new Rook(Side.White)),
			(new Square("b1"), new Knight(Side.White)),
			(new Square("c1"), new Bishop(Side.White)),
			(new Square("d1"), new Queen(Side.White)),
			(new Square("e1"), new King(Side.White)),
			(new Square("f1"), new Bishop(Side.White)),
			(new Square("g1"), new Knight(Side.White)),
			(new Square("h1"), new Rook(Side.White)),
			
			(new Square("a2"), new Pawn(Side.White)),
			(new Square("b2"), new Pawn(Side.White)),
			(new Square("c2"), new Pawn(Side.White)),
			(new Square("d2"), new Pawn(Side.White)),
			(new Square("e2"), new Pawn(Side.White)),
			(new Square("f2"), new Pawn(Side.White)),
			(new Square("g2"), new Pawn(Side.White)),
			(new Square("h2"), new Pawn(Side.White)),
			
			(new Square("a8"), new Rook(Side.Black)),
			(new Square("b8"), new Knight(Side.Black)),
			(new Square("c8"), new Bishop(Side.Black)),
			(new Square("d8"), new Queen(Side.Black)),
			(new Square("e8"), new King(Side.Black)),
			(new Square("f8"), new Bishop(Side.Black)),
			(new Square("g8"), new Knight(Side.Black)),
			(new Square("h8"), new Rook(Side.Black)),
			
			(new Square("a7"), new Pawn(Side.Black)),
			(new Square("b7"), new Pawn(Side.Black)),
			(new Square("c7"), new Pawn(Side.Black)),
			(new Square("d7"), new Pawn(Side.Black)),
			(new Square("e7"), new Pawn(Side.Black)),
			(new Square("f7"), new Pawn(Side.Black)),
			(new Square("g7"), new Pawn(Side.Black)),
			(new Square("h7"), new Pawn(Side.Black)),
		};

		public static (Square, Piece)[] Generate960()
		{
			(Square, Piece)[] layout = new (Square, Piece)[32];

			var rand = new Random();

			string[] files = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };
			List<string> filesList = files.ToList();

			// 0 1 2 3 4 5 6 7
			// place bishops
			string firstBishopFile = filesList[rand.Next(0, 4) * 2];
			string secondBishopFile = filesList[rand.Next(0, 4) * 2 + 1];
			filesList.Remove(firstBishopFile);
			filesList.Remove(secondBishopFile);
			layout[0] = (new Square(firstBishopFile + "1"), new Bishop(Side.White));
			layout[1] = (new Square(firstBishopFile + "8"), new Bishop(Side.Black));
			layout[2] = (new Square(secondBishopFile + "1"), new Bishop(Side.White));
			layout[3] = (new Square(secondBishopFile + "8"), new Bishop(Side.Black));

			// 0 1 2 3 4 5
			// place first rook
			int firstRookIndex = rand.Next(0, 6);
			string firstRookFile = filesList[firstRookIndex];
			layout[4] = (new Square(firstRookFile + "1"), new Rook(Side.White));
			layout[5] = (new Square(firstRookFile + "8"), new Rook(Side.Black));

			// 0 1 2 3 4 5
			// place king
			int kingIndex;
			// There are not enough spaces on the left, king must be on right
			if (firstRookIndex <= 1) kingIndex = rand.Next(firstRookIndex + 1, 5);
			// There are not enough spaces on the right, king must be on the left.
			else if (firstRookIndex >= 4) kingIndex = rand.Next(1, firstRookIndex);
			else // King can be placed on either side.
			{
				kingIndex = firstRookIndex;
				while (kingIndex == firstRookIndex) kingIndex = rand.Next(1, filesList.Count-1);
			}
			string kingFile = filesList[kingIndex];
			layout[6] = (new Square(kingFile + "1"), new King(Side.White));
			layout[7] = (new Square(kingFile + "8"), new King(Side.Black));

			// 0 1 2 3 4 5
			// place second rook
			int secondRookIndex;
			if (kingIndex < firstRookIndex) secondRookIndex = rand.Next(0, kingIndex);
			else secondRookIndex = rand.Next(kingIndex+1, filesList.Count);
			string secondRookFile = filesList[secondRookIndex];
			filesList.Remove(kingFile);
			filesList.Remove(firstRookFile);
			filesList.Remove(secondRookFile);
			layout[8] = (new Square(secondRookFile + "1"), new Rook(Side.White));
			layout[9] = (new Square(secondRookFile + "8"), new Rook(Side.Black));

			// 0 1 2
			// place knights
			string firstKnightFile = filesList[rand.Next(0, 3)];
			filesList.Remove(firstKnightFile);
			layout[10] = (new Square(firstKnightFile + "1"), new Knight(Side.White));
			layout[11] = (new Square(firstKnightFile + "8"), new Knight(Side.Black));
			// 0 1
			int secondKnightIndex = rand.Next(0, 2);
			string secondKnightFile = filesList[secondKnightIndex];
			filesList.Remove(secondKnightFile);
			layout[12] = (new Square(secondKnightFile + "1"), new Knight(Side.White));
			layout[13] = (new Square(secondKnightFile + "8"), new Knight(Side.Black));

			// 0
			// place queens
			string queenFile = filesList[0];
			layout[14] = (new Square(queenFile + "1"), new Queen(Side.White));
			layout[15] = (new Square(queenFile + "8"), new Queen(Side.Black));

			// place pawns
			int layoutIndex = 16;
			for (int i = 0; i < 8; i++)
			{
				layout[layoutIndex++] = (new Square(files[i] + "2"), new Pawn(Side.White));
				layout[layoutIndex++] = (new Square(files[i] + "7"), new Pawn(Side.Black));
			}

			return layout;
		}

		public void MovePiece(Movement move) {
			if (this[move.Start] is not { } pieceToMove) {
				throw new ArgumentException($"No piece was found at the given position: {move.Start}");
			}

			this[move.Start] = null;
			this[move.End] = pieceToMove;

			if (pieceToMove is King) {
				currentKingSquareBySide[pieceToMove.Owner] = move.End;
			}

			(move as SpecialMove)?.HandleAssociatedPiece(this);
		}
		
		internal bool IsOccupiedAt(Square position) => this[position] != null;

		internal bool IsOccupiedBySideAt(Square position, Side side) => this[position] is Piece piece && piece.Owner == side;

		public Square GetKingSquare(Side player) {
			if (currentKingSquareBySide[player] == null) {
				for (int file = 1; file <= 8; file++) {
					for (int rank = 1; rank <= 8; rank++) {
						if (this[file, rank] is King king) {
							currentKingSquareBySide[king.Owner] = new Square(file, rank);
						}
					}
				}
			}

			return currentKingSquareBySide[player] ?? Square.Invalid;
		}

		public string ToTextArt() {
			string result = string.Empty;
			
			for (int rank = 8; rank >= 1; --rank) {
				for (int file = 1; file <= 8; ++file) {
					Piece piece = this[file, rank];
					result += piece.ToTextArt();
					result += file != 8
						? "|"
						: $"\t {rank}";
				}

				result += "\n";
			}
			
			result += "a b c d e f g h";

			return result;
		} 
	}
}