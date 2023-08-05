using DynamicTradeInterface.InterfaceComponents.TableBox;
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
	internal class TradeColumnDef : Def
	{
		internal delegate void TradeColumnCallback(ref Rect boundingBox, Tradeable item, Transactor transactor, ref bool refresh);

		/// <summary>
		/// Colon-based method identifier string for column draw callback.
		/// </summary> 
		public string callbackHandler = null;
		public float defaultWidth = 100;


		//TODO way to define sorting logic.
		//TODO way to add additional data to search string.

		internal TradeColumnCallback _callback;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
		}

	}
}
