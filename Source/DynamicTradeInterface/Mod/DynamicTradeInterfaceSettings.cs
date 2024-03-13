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

			public void ExposeData()
			{
				Scribe_Values.Look(ref Width, nameof(Width));
				Scribe_Values.Look(ref ShowCaption, nameof(ShowCaption));
			}
		}


		const float DEFAULT_TRADE_WIDTH = 0.75f;
		const float DEFAULT_TRADE_HEIGHT = 0.8f;

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


			if (TradeWidthPercentage < 0.01)
				TradeWidthPercentage = DEFAULT_TRADE_WIDTH;

			if (TradeHeightPercentage < 0.01)
				TradeHeightPercentage = DEFAULT_TRADE_HEIGHT;

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
				if (string.IsNullOrWhiteSpace(columnDef.callbackHandler) == false)
				{
					try
					{
						columnDef._callback = AccessTools.MethodDelegate<TradeColumnDef.TradeColumnCallback>(columnDef.callbackHandler);
						if (columnDef._callback == null)
							continue;
						_validColumnDefs.Add(columnDef);
					}
					catch (Exception e)
					{
						Logging.Error($"Unable to locate draw callback '{columnDef.callbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has following arguments: 'ref Rect, Tradeable, TradeAction'");
						Logging.Error(e);
						continue;
					}
				}

				columnDef._searchValueCallback = ParseCallbackHandler<TradeColumnDef.TradeColumnSearchValueCallback>(columnDef.searchValueCallbackHandler,
					$"Unable to locate search value callback '{columnDef.searchValueCallbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has argument of 'List<Tradeable>' and return type of 'Func<Tradeable, object>'");

				columnDef._orderValueCallback = ParseCallbackHandler<TradeColumnDef.TradeColumnOrderValueCallback>(columnDef.orderValueCallbackHandler,
					$"Unable to locate order value callback '{columnDef.orderValueCallbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has argument of 'List<Tradeable>' and return type of 'Func<Tradeable, IComparable>'");

				columnDef._postOpenCallback = ParseCallbackHandler<TradeColumnDef.TradeColumnEventCallback>(columnDef.postOpenCallbackHandler,
					$"Unable to locate post-open callback '{columnDef.postOpenCallbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has arguments matching 'IEnumerable<Tradeable> rows, Transactor transactor'");

				columnDef._postClosedCallback = ParseCallbackHandler<TradeColumnDef.TradeColumnEventCallback>(columnDef.postClosedCallbackHandler,
					$"Unable to locate post-closed callback '{columnDef.postClosedCallbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has arguments matching 'IEnumerable<Tradeable> rows, Transactor transactor'");
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

		private T? ParseCallbackHandler<T>(string? handler, string error) where T : Delegate
		{
			T? result = null;
			if (string.IsNullOrWhiteSpace(handler) == false)
			{
				try
				{
					result = AccessTools.MethodDelegate<T>(handler);
				}
				catch (Exception e)
				{
					Logging.Error(error);
					Logging.Error(e);
				}
			}
			return result;
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
