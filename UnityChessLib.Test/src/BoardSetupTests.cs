using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityChess.Test
{
	public class BoardSetupTests
	{
		private List<(Square, Piece)[]> layouts = new List<(Square, Piece)[]>();

		[SetUp]
		public void Init()
		{
			for (int i = 0; i < 100; i++)
			{
				layouts.Add(Board.Generate960());
			}
		}

		[Test]
		public void Chess960ValidRooks()
		{
			for (int i = 0; i < layouts.Count; i++)
			{
				(Square, Piece)[] pieces = layouts[i];
				(Square, Piece)? rook1 = null;
				for (int j = 0; j < pieces.Length; j++)
				{
					if (pieces[j].Item2 is Rook)
					{
						rook1 = pieces[j];
						break;
					}
				}
				(Square, Piece)? rook2 = null;
				for (int j = 0; j < pieces.Length; j++)
				{
					if (pieces[j].Item2 is Rook && pieces[j].Item2.Owner == rook1?.Item2.Owner && pieces[j] != rook1)
					{
						rook2 = pieces[j];
						break;
					}
				}
				(Square, Piece)? king = null;
				for (int j = 0; j < pieces.Length; j++)
				{
					if (pieces[j].Item2 is King)
					{
						king = pieces[j];
						break;
					}
				}
				Assert.AreNotEqual(rook1?.Item1.File, rook2?.Item1.File);
				if (rook1?.Item1.File > rook2?.Item1.File)
				{
					Assert.IsTrue(rook1?.Item1.File > king?.Item1.File);
					Assert.IsTrue(king?.Item1.File > rook2?.Item1.File);
				}
				else
				{
					Assert.IsTrue(rook1?.Item1.File < king?.Item1.File);
					Assert.IsTrue(king?.Item1.File  < rook2?.Item1.File);
				}
			}
		}

		[Test]
		public void Chess960BishopPlacement()
		{
			// Each bishop is on a different color
			for (int j = 0; j < layouts.Count; j++)
			{
				(Square, Piece)[] pieces = layouts[j];
				(Square, Piece)? b1 = null;
				for (int i = 0; i < pieces.Length; i++)
				{
					if (pieces[i].Item2 is Bishop)
					{
						b1 = pieces[i];
						break;
					}
				}
				Assert.IsNotNull(b1);
				(Square, Piece)? b2 = null;
				for (int i = 0; i < pieces.Length; i++)
				{
					if (pieces[i].Item2 is Bishop && pieces[i].Item2.Owner == b1?.Item2.Owner && pieces[i] != b1)
					{
						b2 = pieces[i];
						break;
					}
				}
				Assert.IsNotNull(b2);
				Assert.AreEqual(b1?.Item1.Rank, b2?.Item1.Rank);
				// if b1's file is even, make sure that b2's file is odd. Otherwise (b1's file is odd) make sure b2's file is even.
				Assert.IsTrue(b1?.Item1.File % 2 == 0 ? b2?.Item1.File % 2 != 0 : b2?.Item1.File % 2 == 0);
			}
		}

		[Test]
		public void Chess960MatchSides()
		{
			// Both sides are the same
			var rankKey = new Dictionary<int, int>();
			rankKey.Add(1, 8);
			rankKey.Add(2, 7);
			rankKey.Add(8, 1);
			rankKey.Add(7, 2);

			for (int i = 0; i < layouts.Count; i++)
			{

				(Square, Piece)[] pieces = layouts[0];
				var map = MapLayout(pieces);
				for (int j = 0; j < 8; j++)
				{
					Square square = pieces[j].Item1;
					Square otherSquare = new Square(square.File, rankKey[square.Rank]);
					Piece piece1 = pieces[j].Item2;
					Piece piece2 = map[otherSquare];
					Assert.AreEqual(piece1.GetType(), piece2.GetType());
				}
			}
		}

		private Dictionary<Square, Piece> MapLayout((Square, Piece)[] pieces)
		{
			var map = new Dictionary<Square, Piece>();
			for (int j = 0; j < pieces.Length; j++)
			{
				map.Add(pieces[j].Item1, pieces[j].Item2);
			}
			return map;
		}
	}
}
