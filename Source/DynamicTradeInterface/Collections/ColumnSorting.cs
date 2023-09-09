using DynamicTradeInterface.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DynamicTradeInterface.Collections
{
	internal struct ColumnSorting : IExposable
	{
		public TradeColumnDef ColumnDef;
		public bool Ascending;

		public ColumnSorting(TradeColumnDef columnDef, bool ascending)
		{
			ColumnDef = columnDef;
			Ascending = ascending;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref ColumnDef, nameof(ColumnDef));
			Scribe_Values.Look(ref Ascending, nameof(Ascending));
		}
	}
}
