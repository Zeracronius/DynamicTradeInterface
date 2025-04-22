using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes
{
	public interface IDrawable
	{
		bool Initialise(Tradeable item);

		void Draw(ref Rect rect, Tradeable item, Transactor transactor, ref bool refresh);

		string GetSearchString(Tradeable item);
	}
}
