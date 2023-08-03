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
		private List<TradeColumnDef> _validColumnDefs;
		private HashSet<TradeColumnDef> _visibleColumns;

        public DynamicTradeInterfaceSettings()
        {
			_validColumnDefs = new List<TradeColumnDef>();
			_visibleColumns = new HashSet<TradeColumnDef>();
		}

		//const int DEFAULT_PORT = 8339;
		//public int port = DEFAULT_PORT;

		//public override void ExposeData()
		//{
		//	Scribe_Values.Look(ref port, "port", DEFAULT_PORT);
		//}




		internal void Initialize()
		{
			_validColumnDefs.Clear();
			List<TradeColumnDef> tradeColumns = DefDatabase<TradeColumnDef>.AllDefsListForReading;

			foreach (TradeColumnDef columnDef in tradeColumns)
			{
				try
				{
					if (String.IsNullOrWhiteSpace(columnDef.callbackHandler) == false)
					{
						columnDef._callback = AccessTools.MethodDelegate<TradeColumnCallback>(columnDef.callbackHandler);
						if (columnDef._callback != null)
						{
							_validColumnDefs.Add(columnDef);
							_visibleColumns.Add(columnDef);
							continue;
						}
					}
				}
				catch (Exception e)
				{
					Logging.Error($"Unable to locate draw callback '{columnDef.callbackHandler}' for column {columnDef.defName}.\nEnsure referenced method has following arguments: 'ref Rect, Tradeable, TradeAction'");
					Logging.Error(e);
				}
			}
		}

		internal IEnumerable<TradeColumnDef> GetVisibleTradeColumns()
		{
			foreach (TradeColumnDef columnDef in _validColumnDefs)
			{
				if (_visibleColumns.Contains(columnDef))
					yield return columnDef;
			}
		}
	}
}
