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
		private static Dictionary<Tradeable, IDrawable> _rowCache = new Dictionary<Tradeable, IDrawable>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackDrawable.PostOpen(rows, transactor);

			foreach (var row in rows)
			{
				if (!_rowCache.ContainsKey(row))
				{
					if (row.AnyThing is Pawn pawn)
					{
						_rowCache[row] = new PawnDrawable(row, pawn);
						continue;
					}

					if (row.AnyThing is Book book)
					{
						_rowCache[row] = new BookDrawable(book);
						continue;
					}

					if (GeneAssistant.Active && row.AnyThing is Genepack genepack)
					{
						_rowCache[row] = new GenepackDrawable(genepack);
						continue;
					}

					if (Techprints.Active)
					{
						var techComp = row.AnyThing.TryGetComp<CompTechprint>();
						if (techComp != null)
						{
							_rowCache[row] = new TechprintDrawable(techComp);
							continue;
						}
					}
				}
			}
		}

		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackDrawable.PostClosed(rows, transactor);
			_rowCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_rowCache.TryGetValue(row, out var cache))
				cache.Draw(ref rect, transactor, ref refresh);
		}


		public static string SearchValue(Tradeable row, Transactor transactor)
		{
			if (_rowCache.TryGetValue(row, out IDrawable? cache))
				return cache.GetSearchString();

			return string.Empty;
		}
	}
}
