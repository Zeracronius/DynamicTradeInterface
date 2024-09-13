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
		private class SummaryItem
		{
			public SummaryItem(Tradeable item, string count)
			{
				Thing = item.AnyThing;
				Label = Thing.LabelCapNoCount;
				Count = count;
			}

            public Thing Thing { get; }
			public string Label { get; }
			public string Count { get; }
		}




		private const float LINE_SPACING = 3;
		static List<(Tradeable, float)> _buffer;
		static List<SummaryItem> _tradeablesSelling;
		static List<SummaryItem> _tradeablesBuying;
		static ListBox<SummaryItem> _sellingListBox;
		static ListBox<SummaryItem> _buyingListBox;

		static string _giftingLabel;
		static string _sellingLabelKey;
		static string _buyingLabelKey;

		static string _buyingSumLabel;
		static string _sellingSumLabel;
		static string _currency;

		static TradeSummary()
        {
			_tradeablesSelling = new List<SummaryItem>();
			_sellingListBox = new ListBox<SummaryItem>(_tradeablesSelling);

			_tradeablesBuying = new List<SummaryItem>();
			_buyingListBox = new ListBox<SummaryItem>(_tradeablesBuying);

			_buffer = new List<(Tradeable, float)>();

			_giftingLabel = "DynamicTradeWindowSummaryGifting".Translate();
			_sellingLabelKey = "DynamicTradeWindowSummarySelling";
			_buyingLabelKey = "DynamicTradeWindowSummaryBuying";

			_buyingSumLabel = "";
			_sellingSumLabel = "";
			_currency = string.Empty;
		}

        public static void Refresh(List<Tradeable> wares)
		{
			_tradeablesBuying.Clear();
			_tradeablesSelling.Clear();

			if (TradeSession.giftMode)
			{
				foreach (var item in wares.Where(x => x.CountToTransfer > 0).OrderByDescending(x => x.CountToTransfer))
					_tradeablesBuying.Add(new SummaryItem(item, Math.Abs(item.CountToTransfer).ToString()));
			}
			else
			{
				_currency = TradeSession.deal.CurrencyTradeable.LabelCap;
				float sum = 0;
				float value = 0;
				_buffer.Clear();
				foreach (var item in wares.Where(x => x.CountToTransfer > 0))
				{
					value = item.GetPriceFor(TradeAction.PlayerBuys) * item.CountToTransfer * -1;
					sum += value;
					_buffer.Add((item, value));
				}
				_tradeablesBuying.AddRange(_buffer.OrderByDescending(x => Mathf.Abs(x.Item2)).Select(x => new SummaryItem(x.Item1, x.Item2.ToStringWithSign())));
				_buyingSumLabel = _buyingLabelKey.Translate(Mathf.RoundToInt(sum).ToStringWithSign(), _currency);

				sum = 0;
				_buffer.Clear();
				foreach (var item in wares.Where(x => x.CountToTransfer < 0))
				{
					value = item.GetPriceFor(TradeAction.PlayerSells) * item.CountToTransfer * -1;
					sum += value;
					_buffer.Add((item, value));
				}
				_tradeablesSelling.AddRange(_buffer.OrderByDescending(x => Mathf.Abs(x.Item2)).Select(x => new SummaryItem(x.Item1, x.Item2.ToStringWithSign())));
				_sellingSumLabel = _sellingLabelKey.Translate(Mathf.RoundToInt(sum).ToStringWithSign(), _currency);
			}
		}

		public static void Draw(ref Rect inRect)
		{

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

		private static void DrawItem(Rect rect, SummaryItem item)
		{
			float x = rect.x;
			float width = rect.width;
			Text.Anchor = TextAnchor.UpperLeft;

			Widgets.ThingIcon(new Rect(x, rect.y, rect.height, rect.height), item.Thing);

			// Allocate 20% of column to numbers
			float counterWidth = rect.width * 0.2f; 

			width -= rect.height + counterWidth;
			x += rect.height + 6;
			Widgets.Label(new Rect(x, rect.y + LINE_SPACING, width, rect.height), item.Label);

			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(new Rect(rect.xMax - counterWidth, rect.y + LINE_SPACING, counterWidth, rect.height), item.Count);
		}
	}
}
