using DynamicTradeInterface.InterfaceComponents;
using DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal class ColumnExtraIcons
	{
		private static Dictionary<Tradeable, ITradeable> _rowCache = new Dictionary<Tradeable, ITradeable>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackTradeable.PostOpen(rows, transactor);

			foreach (var row in rows)
			{
				if (!_rowCache.ContainsKey(row))
				{
					if (row.AnyThing is Pawn pawn)
						_rowCache[row] = new PawnTradeable(row, pawn);
					else if (row.AnyThing is Genepack genepack && GeneAssistant.Active)
						_rowCache[row] = new GenepackTradeable(genepack);
				}
			}
		}

		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackTradeable.PostClosed(rows, transactor);
			_rowCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_rowCache.TryGetValue(row, out var cache))
				cache.Draw(ref rect, row, transactor, ref refresh);
		}
	}
}
