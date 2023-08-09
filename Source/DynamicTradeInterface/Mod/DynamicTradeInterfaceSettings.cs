using DynamicTradeInterface.Defs;
using DynamicTradeInterface.InterfaceComponents.TableBox;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static DynamicTradeInterface.Defs.TradeColumnDef;

namespace DynamicTradeInterface.Mod
{
	internal class DynamicTradeInterfaceSettings : ModSettings
	{
		private HashSet<TradeColumnDef> _validColumnDefs;
		private List<TradeColumnDef> _visibleColumns;



		bool _profilingEnabled;
		Dictionary<TradeColumnDef, Queue<long>> _tradeColumnProfilings;

		public DynamicTradeInterfaceSettings()
		{
			_validColumnDefs = new HashSet<TradeColumnDef>();
			_visibleColumns = new List<TradeColumnDef>();
			_tradeColumnProfilings = new Dictionary<TradeColumnDef, Queue<long>>();
		}

		internal HashSet<TradeColumnDef> ValidColumns => _validColumnDefs;
		internal List<TradeColumnDef> VisibleColumns => _visibleColumns;

		internal bool ProfilingEnabled
		{
			get => _profilingEnabled;
			set => _profilingEnabled = value;
		}

		internal Dictionary<TradeColumnDef, Queue<long>> TradeColumnProfilings => _tradeColumnProfilings;



		public override void ExposeData()
		{
			base.ExposeData();


			List<TradeColumnDef> visibleColumns = _visibleColumns;
			Scribe_Collections.Look(ref visibleColumns, nameof(visibleColumns));
			if (visibleColumns != null && visibleColumns.Count > 0)
				_visibleColumns = visibleColumns;
		}


		internal void Initialize()
		{
			_validColumnDefs.Clear();
			List<TradeColumnDef> tradeColumns = DefDatabase<TradeColumnDef>.AllDefsListForReading;

			foreach (TradeColumnDef columnDef in tradeColumns)
			{
				if (string.IsNullOrWhiteSpace(columnDef.callbackHandler) == false)
				{
					try
					{
						columnDef._callback = AccessTools.MethodDelegate<TradeColumnCallback>(columnDef.callbackHandler);
						if (columnDef._callback != null)
						{
							_validColumnDefs.Add(columnDef);
							_visibleColumns.Add(columnDef);
						}
					}
					catch (Exception e)
					{
						Logging.Error($"Unable to locate draw callback '{columnDef.callbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has following arguments: 'ref Rect, Tradeable, TradeAction'");
						Logging.Error(e);
					}
				}

				if (string.IsNullOrWhiteSpace(columnDef.orderValueCallbackHandler) == false)
				{
					try
					{
						columnDef._orderValueCallback = AccessTools.MethodDelegate<TradeColumnOrderValueCallback>(columnDef.orderValueCallbackHandler);
					}
					catch (Exception e)
					{
						Logging.Error($"Unable to locate order value callback '{columnDef.orderValueCallbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has argument of 'List<Tradeable>' and return type of 'Func<Tradeable, object>'");
						Logging.Error(e);
					}
				}
			}
		}

		internal IEnumerable<TradeColumnDef> GetVisibleTradeColumns()
		{
			foreach (TradeColumnDef columnDef in _visibleColumns)
			{
				if (_validColumnDefs.Contains(columnDef))
					yield return columnDef;
			}
		}
	}
}
