using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes
{
	public interface ITradeable
	{
		void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh);
	}
}
