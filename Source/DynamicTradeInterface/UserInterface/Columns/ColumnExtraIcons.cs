using DynamicTradeInterface.Defs;
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
		private static Dictionary<Tradeable, Stack<IDrawable>> _rowCache = new Dictionary<Tradeable, Stack<IDrawable>>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackDrawableRow.PostOpen(rows, transactor);

			List<MoreIconsDef> moreIconDefs = DefDatabase<MoreIconsDef>.AllDefsListForReading;

			foreach (var row in rows)
			{
				if (!_rowCache.ContainsKey(row))
				{
					Stack<IDrawable> drawables = new Stack<IDrawable>();

//					if (row.AnyThing is Pawn pawn)
//					{
//						IDrawable pawnDrawable = new PawnDrawable();
//						pawnDrawable.Initialise(row);
//						drawables.Push(pawnDrawable);
//					}

//#if V1_5
//					if (row.AnyThing is Book book)
//					{
//						IDrawable bookDrawable = new BookDrawable();
//						bookDrawable.Initialise(row);
//						drawables.Push(bookDrawable);
//					}
//#endif

//					if (GeneAssistant.Active && row.AnyThing is Genepack genepack)
//					{
//						IDrawable genepackDrawable = new GenepackDrawable();
//						genepackDrawable.Initialise(row);
//						drawables.Push(genepackDrawable);
//					}

//					if (Techprints.Active)
//					{
//						var techComp = row.AnyThing.TryGetComp<CompTechprint>();
//						if (techComp != null)
//						{
//							IDrawable techprintDrawable = new TechprintDrawable();
//							techprintDrawable.Initialise(row);
//							drawables.Push(techprintDrawable);
//						}
//					}

					foreach (MoreIconsDef iconDef in moreIconDefs)
					{
						if (iconDef.Initialise(row))
							drawables.Push(iconDef);
					}
					_rowCache[row] = drawables;
				}
			}
		}

		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackDrawableRow.PostClosed(rows, transactor);
			_rowCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_rowCache.TryGetValue(row, out var cache))
			{
				foreach (IDrawable drawable in cache)
					drawable.Draw(ref rect, row, transactor, ref refresh);
			}
		}


		public static string SearchValue(Tradeable row, Transactor transactor)
		{
			if (_rowCache.TryGetValue(row, out var cache))
			{
				return string.Join(" ", cache.Select(x => x.GetSearchString(row)));
			}
			return string.Empty;
		}
	}
}
