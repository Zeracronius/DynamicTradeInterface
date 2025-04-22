using DynamicTradeInterface.Mod;
using DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes;
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

namespace DynamicTradeInterface.Defs
{
	internal class MoreIconsDef : Def, IDrawable
	{
		internal delegate void MoreIconsDrawCallback(ref Rect rect, Tradeable item, Transactor transactor, ref bool refresh);
		internal delegate string MoreIconsSearchValueCallback(Tradeable item);
		internal delegate bool MoreIconsInitialiseCallback(Tradeable item);

		internal MoreIconsDrawCallback? _drawCallback;
		internal MoreIconsSearchValueCallback? _searchValueCallback;
		internal MoreIconsInitialiseCallback? _initialiseOpenCallback;

		/// <summary>
		/// Colon-based method identifier string for the method called when icon is drawn.
		/// </summary> 
		public string? drawCallbackHandler = null;

		/// <summary>
		/// Colon-based method identifier string for method to allow this icon to provide additional searchable strings.
		/// </summary> 
		public string? searchValueCallbackHandler = null;

		/// <summary>
		/// Colon-based method identifier string for method to allow this icon to load and cache data.
		/// </summary>
		public string? initialiseCallbackHandler = null;

		public void Draw(ref Rect rect, Tradeable item, Transactor transactor, ref bool refresh)
		{
			if (_drawCallback != null)
				_drawCallback(ref rect, item, transactor, ref refresh);
		}

		public string GetSearchString(Tradeable item)
		{
			if (_searchValueCallback != null)
				return _searchValueCallback(item);

			return "";
		}

		public bool Initialise(Tradeable item)
		{
			if (_initialiseOpenCallback != null)
				return _initialiseOpenCallback(item);

			return false;
		}


		public void ParseCallbacks()
		{
			_drawCallback = ParseCallbackHandler<MoreIconsDrawCallback>(drawCallbackHandler,
				$"Unable to locate draw callback '{drawCallbackHandler}' for IconDef {defName}.\nEnsure referenced method has arguments matching 'ref Rect rect, Tradeable item, Transactor transactor, ref bool refresh''");

			_searchValueCallback = ParseCallbackHandler<MoreIconsSearchValueCallback>(searchValueCallbackHandler,
				$"Unable to locate search value callback '{searchValueCallbackHandler}' for column {defName}.\nEnsure referenced method has arguments matching 'Tradeable item' and a return type of 'string'");

			_initialiseOpenCallback = ParseCallbackHandler<MoreIconsInitialiseCallback>(initialiseCallbackHandler,
				$"Unable to locate initialise callback '{initialiseCallbackHandler}' for column {defName}.\nEnsure referenced method has arguments matching 'Tradeable item' returning a bool on whether it is relevant or not.");
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
