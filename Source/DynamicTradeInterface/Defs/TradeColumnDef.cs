using DynamicTradeInterface.InterfaceComponents.TableBox;
using DynamicTradeInterface.Mod;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.Defs
{
	public class TradeColumnDef : Def
	{
		internal delegate void TradeColumnCallback(ref Rect boundingBox, Tradeable item, Transactor transactor, ref bool refresh);
		internal delegate void TradeColumnEventCallback(IEnumerable<Tradeable> rows, Transactor transactor);
		internal delegate Func<Tradeable, IComparable> TradeColumnOrderValueCallback(Transactor transactor);
		internal delegate string TradeColumnSearchValueCallback(Tradeable item, Transactor transactor);

		/// <summary>
		/// Colon-based method identifier string for method called when column is drawn.
		/// </summary> 
		public string? callbackHandler = null;

		/// <summary>
		/// Colon-based method identifier string for method to allow rows to be sorted by this column.
		/// </summary> 
		public string? orderValueCallbackHandler = null;

		/// <summary>
		/// Colon-based method identifier string for method to allow this column to provide rows with additional searchable strings.
		/// </summary> 
		public string? searchValueCallbackHandler = null;

		/// <summary>
		/// Colon-based method identifier string for method called right after the trade window has been opened. Can be used to cache data.
		/// </summary> 
		public string? postOpenCallbackHandler = null;

		/// <summary>
		/// Colon-based method identifier string for method called right after the trade window has been closed. Can be used to clean up cached data.
		/// </summary> 
		public string? postClosedCallbackHandler = null;

		public float defaultWidth = 100;

		/// <summary>
		/// Whether or not the column should be automatically added to visible columns.
		/// </summary>
		public bool defaultVisible = true;

		/// <summary>
		/// Whether the label should be shown in the column header.
		/// </summary>
		public bool showCaption = true;

		/// <summary>
		/// If set then this is shown as a tooltip on column header, otherwise label is used.
		/// </summary>
		public string? tooltip;

		/// <summary>
		/// The direction the column will sort by on first click. Descending is default.
		/// </summary>
		public SortDirection initialSort = SortDirection.Descending;

		internal TradeColumnCallback? _callback;
		internal TradeColumnOrderValueCallback? _orderValueCallback;
		internal TradeColumnSearchValueCallback? _searchValueCallback;
		internal TradeColumnEventCallback? _postOpenCallback;
		internal TradeColumnEventCallback? _postClosedCallback;

		// TODO consider if this override is required.
		public override void ResolveReferences()
		{
			base.ResolveReferences();
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (var error in base.ConfigErrors()) 
			{
				yield return error;
			}

			if (string.IsNullOrEmpty(callbackHandler))
				yield return "TradeColumnDef must have a callbackHandler defined.";
		}


		public bool ParseCallbacks()
		{
			if (string.IsNullOrWhiteSpace(callbackHandler) == false)
			{
				try
				{
					_callback = AccessTools.MethodDelegate<TradeColumnCallback>(callbackHandler);
					if (_callback == null)
						return false;
				}
				catch (Exception e)
				{
					Logging.Error($"Unable to locate draw callback '{callbackHandler}' for column {defName}.\nEnsure referenced method has following arguments: 'ref Rect, Tradeable, TradeAction'");
					Logging.Error(e);
					return false;
				}
			}

			_searchValueCallback = ParseCallbackHandler<TradeColumnSearchValueCallback>(searchValueCallbackHandler,
				$"Unable to locate search value callback '{searchValueCallbackHandler}' for column {defName}.\nEnsure referenced method has argument of 'List<Tradeable>' and return type of 'Func<Tradeable, object>'");

			_orderValueCallback = ParseCallbackHandler<TradeColumnOrderValueCallback>(orderValueCallbackHandler,
				$"Unable to locate order value callback '{orderValueCallbackHandler}' for column {defName}.\nEnsure referenced method has argument of 'List<Tradeable>' and return type of 'Func<Tradeable, IComparable>'");

			_postOpenCallback = ParseCallbackHandler<TradeColumnEventCallback>(postOpenCallbackHandler,
				$"Unable to locate post-open callback '{postOpenCallbackHandler}' for column {defName}.\nEnsure referenced method has arguments matching 'IEnumerable<Tradeable> rows, Transactor transactor'");

			_postClosedCallback = ParseCallbackHandler<TradeColumnEventCallback>(postClosedCallbackHandler,
				$"Unable to locate post-closed callback '{postClosedCallbackHandler}' for column {defName}.\nEnsure referenced method has arguments matching 'IEnumerable<Tradeable> rows, Transactor transactor'");
			return true;
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
	}
}
