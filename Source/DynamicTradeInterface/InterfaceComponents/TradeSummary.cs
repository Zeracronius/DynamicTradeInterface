using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;
using Verse.Noise;

namespace DynamicTradeInterface.InterfaceComponents
{
	internal static class TradeSummary
	{
		private const float LINE_SPACING = 3;
		static List<Tradeable> _tradeablesSelling;
		static List<Tradeable> _tradeablesBuying;
		static ListBox<Tradeable> _sellingListBox;
		static ListBox<Tradeable> _buyingListBox;

		static string _giftingLabel;
		static string _sellingLabel;
		static string _buyingLabel;

		static string _buyingSumLabel;
		static string _sellingSumLabel;
		static string _currency;

		static TradeSummary()
        {
			_tradeablesSelling = new List<Tradeable>();
			_sellingListBox = new ListBox<Tradeable>(_tradeablesSelling);

			_tradeablesBuying = new List<Tradeable>();
			_buyingListBox = new ListBox<Tradeable>(_tradeablesBuying);

			_giftingLabel = "TradeHelperTitleGift".Translate();
			_sellingLabel = "TradeHelperTitleSelling".Translate();
			_buyingLabel = "TradeHelperTitleBuying".Translate();

			_buyingSumLabel = "";
			_sellingSumLabel = "";
			_currency = string.Empty;
		}

        public static void Refresh(List<Tradeable> wares)
		{
			_tradeablesBuying.Clear();
			_tradeablesBuying.AddRange(wares.Where(x => x.CountToTransfer > 0));

			int sum = 0;
			foreach (Tradeable trad in _tradeablesBuying)
				sum += (int)Math.Abs(trad.GetPriceFor(TradeAction.PlayerBuys) * trad.CountToTransfer);

			_buyingSumLabel = _buyingLabel + " (-" + sum.ToString() + " " + _currency + ")";


			_tradeablesSelling.Clear();
			_tradeablesSelling.AddRange(wares.Where(x => x.CountToTransfer < 0));

			sum = 0;
			foreach (Tradeable trad in _tradeablesSelling)
				sum += (int)Math.Abs(trad.GetPriceFor(TradeAction.PlayerSells) * trad.CountToTransfer);

			_sellingSumLabel = _sellingLabel + " (+" + sum.ToString() + " " + _currency + ")";

			_currency = TradeSession.deal.CurrencyTradeable.LabelCap;
		}

		public static void Draw(ref Rect inRect)
		{
			// Tradehelper lists everything as one long list inside a combined scroll viewer.
			// Instead scale up each Buy and Sell section until 50% before adding a scrollbar for each individually.
			// Get scrollable list box from PM
			// Further investigation into notification system, go with original intention of tooltip editor
			// Tradehelper doesn't scale and overflows if more than 8 things are added.

			string label;
			float y = inRect.yMin;

			Rect sellingRect;
			Rect buyingRect;


			Widgets.DrawBoxSolidWithOutline(inRect, Widgets.WindowBGFillColor, Color.gray);
			inRect = inRect.ContractedBy(4);

			if (TradeSession.giftMode)
			{
				buyingRect = inRect;
				sellingRect = default;
				label = _giftingLabel;
			}
			else
			{
				inRect.SplitHorizontallyWithMargin(out buyingRect, out sellingRect, out _, 4, (inRect.height - 4) / 2);
				label = _buyingSumLabel;
			}

			// Centered "Buying (-##### Silver)"
			DrawHeader(buyingRect.x, ref y, buyingRect.width, label);
			if (_tradeablesBuying.Count > 0)
			{
				_buyingListBox.Draw(new Rect(buyingRect.x, y + LINE_SPACING, buyingRect.width, buyingRect.height - 28f), out float height, DrawItem);
				y += height;
			}

			if (TradeSession.giftMode == false)
			{
				// Centered "Selling (+##### Silver)"
				DrawHeader(sellingRect.x, ref y, sellingRect.width, _sellingSumLabel);
				if (_tradeablesSelling.Count > 0)
				{
					_sellingListBox.Draw(new Rect(sellingRect.x, y + LINE_SPACING, sellingRect.width, sellingRect.height - 28f), out float height, DrawItem);
					y += height;
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private static void DrawHeader(float x, ref float y, float width, string label)
		{
			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Small;
			Widgets.Label(new Rect(x, y + LINE_SPACING, width, 28f), label);
			Widgets.DrawHighlight(new Rect(x, y + LINE_SPACING, width, 22f));
			y += 28f;
		}

		private static void DrawItem(Rect rect, Tradeable item)
		{
			float x = rect.x;
			float width = rect.width;
			Thing thing = item.AnyThing;
			Text.Anchor = TextAnchor.UpperLeft;

			Widgets.ThingIcon(new Rect(x, rect.y, rect.height, rect.height), thing);

			width -= rect.height + 40;
			x += rect.height + 6;
			Widgets.Label(new Rect(x, rect.y + LINE_SPACING, width, rect.height), thing.LabelCapNoCount);

			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(new Rect(rect.xMax - 40, rect.y + LINE_SPACING, 40, rect.height), Math.Abs(item.CountToTransfer).ToStringCached());
		}
	}
}
