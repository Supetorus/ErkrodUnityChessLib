﻿namespace UnityChess {
	public class Bishop : Piece {
		public Bishop(Square startingPosition, Side owningSide) : base(startingPosition, owningSide) {}
		public Bishop(Bishop bishopCopy) : base(bishopCopy) {}

		public override void UpdateLegalMoves(Board board, Square enPassantEligibleSquare) {
			LegalMoves.Clear();
			CheckDiagonalDirections(board);
		}

		private void CheckDiagonalDirections(Board board) {
			foreach (int fileOffset in new[] {-1, 1}) {
				foreach (int rankOffset in new[] {-1, 1}) {
					Square testSquare = new Square(Position, fileOffset, rankOffset);
					Movement testMove = new Movement(Position, testSquare);

					while (testSquare.IsValid) {
						Square enemyKingPosition = OwningSide == Side.White ? board.BlackKing.Position : board.WhiteKing.Position;
						if (board.IsOccupied(testSquare)) {
							if (!board.IsOccupiedBySide(testSquare, OwningSide) && Rules.MoveObeysRules(board, testMove, OwningSide) && testSquare != enemyKingPosition)
								LegalMoves.Add(new Movement(testMove));

							break;
						}

						if (Rules.MoveObeysRules(board, testMove, OwningSide) && testSquare != enemyKingPosition)
							LegalMoves.Add(new Movement(testMove));

						testSquare = new Square(testSquare, fileOffset, rankOffset);
						testMove = new Movement(Position, testSquare);
					}
				}
			}
		}
	}
}