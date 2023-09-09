using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal static class ColumnCaption
	{
		private struct Cache
		{
			public string Label;
			public float LabelWidth;
			public Color Color;
			public string? JoinAs;
			public string? JoinAsDesc;
			public float JoinAsWidth;
		}

		private static Dictionary<Tradeable, Cache> _labelCache = new Dictionary<Tradeable, Cache>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
			{
				string label = row.LabelCap;
				string? joinAsLabel = null;
				string? joinAsDesc = null;
				float joinAsWidth = 0;
				if (ModsConfig.IdeologyActive && row.AnyThing is Pawn pawn)
				{
					if (pawn.RaceProps.Humanlike && pawn.guest != null && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony)
					{
						joinAsLabel = (pawn.guest.joinStatus == JoinStatus.JoinAsColonist ? "JoinsAsColonist" : "JoinsAsSlave").Translate();
						joinAsDesc = (pawn.guest.joinStatus == JoinStatus.JoinAsColonist ? "JoinsAsColonistDesc" : "JoinsAsSlaveDesc").Translate();
						joinAsWidth = Text.CalcSize(joinAsLabel).x;
					}

				}

				Cache cache = new Cache()
				{
					Label = label,
					LabelWidth = Text.CalcSize(label).x,
					Color = row.TraderWillTrade ? Color.white : TradeUI.NoTradeColor,
					JoinAs = joinAsLabel,
					JoinAsDesc = joinAsDesc,
					JoinAsWidth = joinAsWidth,
				};

				_labelCache[row] = cache;
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_labelCache.Clear();
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_labelCache.TryGetValue(row, out Cache cached) == false)
				return;

			Text.WordWrap = false;
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = cached.Color;

			Rect labelRect = new Rect(rect.x, rect.y, Math.Min(rect.width, cached.LabelWidth), rect.height);
			DrawLabel(ref labelRect, cached.Label, cached.LabelWidth);

			Rect joinAsRect = new Rect(labelRect.xMax + GenUI.GapTiny, rect.y, Math.Min(rect.width - (labelRect.xMax + GenUI.GapTiny), cached.JoinAsWidth), rect.height);
			if (cached.JoinAs != null)
			{
				GUI.color = TradeUI.NoTradeColor;
				DrawLabel(ref joinAsRect, cached.JoinAs, cached.JoinAsWidth);
			}
			
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;

			if (Mouse.IsOver(labelRect))
			{
				TooltipHandler.TipRegion(labelRect, () =>
				{
					Thing thing = row.AnyThing;
					if (thing != null)
					{
						string tipDescription = row.TipDescription;
						if (string.IsNullOrWhiteSpace(tipDescription) == false)
						{
							return $"{row.LabelCap}: {tipDescription}{TransferableUIUtility.ContentSourceDescription(thing)}";
						}
					}
					return "";
				}, row.GetHashCode());
			}

			if (cached.JoinAs != null && Mouse.IsOver(joinAsRect))
				TooltipHandler.TipRegion(joinAsRect, cached.JoinAsDesc);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DrawLabel(ref Rect rect, string label, float width)
		{
			//if (width > rect.width)
			//	label.Truncate(rect.width);
			Widgets.Label(rect, label);
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) => _labelCache[row].Label;
		}
	}
}
