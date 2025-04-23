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

namespace DynamicTradeInterface.Defs
{
	internal class MoreIconsDef : Def
	{
		internal delegate IEnumerable<(Texture, string?, Color?)> MoreIconsGetIconsCallback(Tradeable tradeable);
		internal delegate string MoreIconsSearchValueCallback(Tradeable item);

		internal MoreIconsSearchValueCallback? _searchValueCallback;
		internal MoreIconsGetIconsCallback? _getIconsCallback;

		/// <summary>
		/// Colon-based method identifier string for the method called when retreiving icons.
		/// </summary> 
		public string? getIconsCallbackHandler = null;

		/// <summary>
		/// Colon-based method identifier string for method to allow this icon to provide additional searchable strings.
		/// </summary> 
		public string? searchValueCallbackHandler = null;

		public IEnumerable<(Texture, string?, Color?)> GetIcons(Tradeable tradeable)
		{
			if (_getIconsCallback == null)
				yield break;

			foreach (var item in _getIconsCallback(tradeable))
				yield return item;
		}

		public string GetSearchString(Tradeable item)
		{
			if (_searchValueCallback != null)
				return _searchValueCallback(item);

			return "";
		}

		public void ParseCallbacks()
		{
			_getIconsCallback = ParseCallbackHandler<MoreIconsGetIconsCallback>(getIconsCallbackHandler,
				$"Unable to locate draw callback '{getIconsCallbackHandler}' for IconDef {defName}.\nEnsure referenced method has arguments matching 'Tradeable' and a return type of 'IEnumerable<(Texture, string?, Color?)>'");

			_searchValueCallback = ParseCallbackHandler<MoreIconsSearchValueCallback>(searchValueCallbackHandler,
				$"Unable to locate search value callback '{searchValueCallbackHandler}' for column {defName}.\nEnsure referenced method has arguments matching 'Tradeable' and a return type of 'string'");
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
