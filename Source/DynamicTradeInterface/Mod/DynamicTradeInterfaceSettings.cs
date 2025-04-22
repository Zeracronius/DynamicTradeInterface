using DynamicTradeInterface.Collections;
using DynamicTradeInterface.Defs;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DynamicTradeInterface.Mod
{
	internal class DynamicTradeInterfaceSettings : ModSettings
	{
		[Flags]
		public enum TableType
		{
			None = 0,
			Colony = 1,
			Trader = 2,
			Both = 3,
		}

		internal class ColumnCustomization : IExposable
		{
			public ColumnCustomization()
			{

			}

			internal ColumnCustomization(TradeColumnDef columnDef)
			{
				Width = columnDef.defaultWidth;
				ShowCaption = columnDef.showCaption;
			}

			public float Width;
			public bool ShowCaption;
			public TableType TableType = TableType.Both;


			public void ExposeData()
			{
				Scribe_Values.Look(ref Width, nameof(Width));
				Scribe_Values.Look(ref ShowCaption, nameof(ShowCaption));
				Scribe_Values.Look(ref TableType, nameof(TableType), TableType.Both);
			}
		}


		const float DEFAULT_TRADE_WIDTH = 0.75f;
		const float DEFAULT_TRADE_HEIGHT = 0.8f;
		public const int DEFAULT_TRADE_SUMMARY_WIDTH = 200;

		Dictionary<TradeColumnDef, ColumnCustomization> _columnCustomization;
		Dictionary<TradeColumnDef, Queue<long>> _tradeColumnProfilings;
		HashSet<TradeColumnDef> _validColumnDefs;
		List<TradeColumnDef> _visibleColumns;

		List<ColumnSorting> _colonySorting;
		List<ColumnSorting> _traderSorting;

		public DynamicTradeInterfaceSettings()
		{
			_validColumnDefs = new HashSet<TradeColumnDef>();
			_visibleColumns = new List<TradeColumnDef>();
			ValidationDefs = new HashSet<TradeValidationDef>();
			_tradeColumnProfilings = new Dictionary<TradeColumnDef, Queue<long>>();
			_columnCustomization = new Dictionary<TradeColumnDef, ColumnCustomization>();
			_colonySorting = new List<ColumnSorting>();
			_traderSorting = new List<ColumnSorting>();
		}

		internal HashSet<TradeColumnDef> ValidColumns => _validColumnDefs;
		internal List<TradeColumnDef> VisibleColumns => _visibleColumns;
		public HashSet<TradeValidationDef> ValidationDefs { get; }


		internal Dictionary<TradeColumnDef, Queue<long>> TradeColumnProfilings => _tradeColumnProfilings;


		public bool ProfilingEnabled;
		public float TradeWidthPercentage = DEFAULT_TRADE_WIDTH;
		public float TradeHeightPercentage = DEFAULT_TRADE_HEIGHT;
		public bool ExcludeUnwillingItems;
		public bool GhostButtons;
		public bool StackDurability;
		public bool RememberSortings;
		public bool TradeWindowLocked;
		public bool AutoRefocus = false;
		public bool OpenAsDefault = true;
		public bool PauseAfterTrade;
		public bool ShowTradeSummary;
		public bool ShowAvailableOnMap;
		public bool AlternatingRowColor;
		public int TradeSummaryWidthPixels = DEFAULT_TRADE_SUMMARY_WIDTH;


		public List<ColumnSorting> StoredColonySorting
		{
			get => _colonySorting;
		}

		public List<ColumnSorting> StoredTraderSorting
		{
			get => _traderSorting;
		}

		public override void ExposeData()
		{
			base.ExposeData();


			Scribe_Values.Look(ref TradeWidthPercentage, nameof(TradeWidthPercentage), DEFAULT_TRADE_WIDTH);
			Scribe_Values.Look(ref TradeHeightPercentage, nameof(TradeHeightPercentage), DEFAULT_TRADE_HEIGHT);

			Scribe_Values.Look(ref ExcludeUnwillingItems, nameof(ExcludeUnwillingItems), false);
			Scribe_Values.Look(ref GhostButtons, nameof(GhostButtons), false);
			Scribe_Values.Look(ref RememberSortings, nameof(RememberSortings), false);
			Scribe_Values.Look(ref TradeWindowLocked, nameof(TradeWindowLocked), false);
			Scribe_Values.Look(ref StackDurability, nameof(StackDurability), false);
			Scribe_Values.Look(ref AutoRefocus, nameof(AutoRefocus), false);
			Scribe_Values.Look(ref OpenAsDefault, nameof(OpenAsDefault), true);
			Scribe_Values.Look(ref PauseAfterTrade, nameof(PauseAfterTrade), false);
			Scribe_Values.Look(ref ShowTradeSummary, nameof(ShowTradeSummary), false);
			Scribe_Values.Look(ref TradeSummaryWidthPixels, nameof(TradeSummaryWidthPixels), DEFAULT_TRADE_SUMMARY_WIDTH);
			Scribe_Values.Look(ref ShowAvailableOnMap, nameof(ShowAvailableOnMap), true);
			Scribe_Values.Look(ref AlternatingRowColor, nameof(AlternatingRowColor), false);



			if (TradeWidthPercentage < 0.01)
				TradeWidthPercentage = DEFAULT_TRADE_WIDTH;

			if (TradeHeightPercentage < 0.01)
				TradeHeightPercentage = DEFAULT_TRADE_HEIGHT;

			if (TradeSummaryWidthPixels < 20)
				TradeSummaryWidthPixels = DEFAULT_TRADE_SUMMARY_WIDTH;

			Scribe_Collections.Look(ref _visibleColumns, "visibleColumns");
			if (_visibleColumns != null)
				_visibleColumns = _visibleColumns.Where(x => x != null).ToList();


			Scribe_Collections.Look(ref _traderSorting, "traderSorting", LookMode.Deep);
			if (_traderSorting != null)
				_traderSorting = _traderSorting.Where(x => x.ColumnDef != null).ToList();
			else
				_traderSorting = new List<ColumnSorting>();


			Scribe_Collections.Look(ref _colonySorting, "colonySorting", LookMode.Deep);
			if (_colonySorting != null)
				_colonySorting = _colonySorting.Where(x => x.ColumnDef != null).ToList();
			else
				_colonySorting = new List<ColumnSorting>();


			Scribe_Collections.Look(ref _columnCustomization, nameof(ColumnCustomization), LookMode.Def, LookMode.Deep);
			if (_columnCustomization == null)
				_columnCustomization = new Dictionary<TradeColumnDef, ColumnCustomization>();
		}

		private void InitializeColumns()
		{
			_validColumnDefs.Clear();
			List<TradeColumnDef> tradeColumns = DefDatabase<TradeColumnDef>.AllDefsListForReading;

			foreach (TradeColumnDef columnDef in tradeColumns)
			{
				if (columnDef.ParseCallbacks())
					_validColumnDefs.Add(columnDef);
			}

			foreach (MoreIconsDef iconDef in DefDatabase<MoreIconsDef>.AllDefsListForReading)
			{
				iconDef.ParseCallbacks();
			}

			// Default visible columns
			if (_visibleColumns.Count == 0)
				_visibleColumns.AddRange(_validColumnDefs.Where(x => x.defaultVisible));
		}

		private void InitializeTradeValidation()
		{
			foreach (var validationDef in DefDatabase<TradeValidationDef>.AllDefsListForReading)
			{
				if (string.IsNullOrWhiteSpace(validationDef.validationCallbackHandler))
				{
					continue;
				}

				try
				{
					validationDef.validationCallback = AccessTools.MethodDelegate<TradeValidationDef.TradeValidationAction>(validationDef.validationCallbackHandler);
					if (validationDef.validationCallback != null)
					{
						ValidationDefs.Add(validationDef);
					}
				}
				catch (Exception e)
				{
					Logging.Error($"Unable to locate validation handler '{validationDef.validationCallbackHandler}' for validator {validationDef.defName}.\nEnsure referenced method has no arguments and a return type of bool'");
					Logging.Error(e);
				}
			}
		}

		internal void Initialize()
		{
			InitializeColumns();
			InitializeTradeValidation();
		}


		internal IEnumerable<TradeColumnDef> GetVisibleTradeColumns()
		{
			foreach (TradeColumnDef columnDef in _visibleColumns)
			{
				if (_validColumnDefs.Contains(columnDef))
					yield return columnDef;
			}
		}

		public ColumnCustomization? GetColumnCustomization(TradeColumnDef columnDef)
		{
			_columnCustomization.TryGetValue(columnDef, out ColumnCustomization customization);
			return customization;
		}

		public ColumnCustomization CreateColumnCustomization(TradeColumnDef columnDef)
		{
			if (_columnCustomization.TryGetValue(columnDef, out ColumnCustomization customization))
				return customization;

			customization = new ColumnCustomization(columnDef);
			_columnCustomization[columnDef] = customization;
			return customization;
		}

		public void ClearColumnCustomization()
		{
			_columnCustomization.Clear();
		}
	}
}
