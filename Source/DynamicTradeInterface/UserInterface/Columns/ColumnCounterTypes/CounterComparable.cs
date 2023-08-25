using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTradeInterface.UserInterface.Columns.ColumnCounterTypes
{
	/// <summary>
	/// Wraps integer to sort positive at the top, then negatives, then 0.
	/// Used for counter column to sort buying, selling, none.
	/// </summary>
	/// <seealso cref="System.IComparable" />
	struct CounterComparable : IComparable
	{
		int _value;
		public CounterComparable(int value)
		{
			_value = value;
		}

		// return +1 to consider this instance greater, -1 to consider instance this smaller
		public int CompareTo(object other)
		{
			if (other is CounterComparable comparable == false)
				return 1;

			int otherValue = comparable._value;
			if (_value == otherValue)
				return 0;

			// If that is 0 and this isn't, then consider this greater.
			if (otherValue == 0)
				return 1;

			// If this is 0 and that isn't then consider this smaller
			if (_value == 0)
				return -1;

			// This is buying
			if (_value > 0)
			{
				// That is buying
				if (otherValue > 0)
					return _value > otherValue ? 1 : -1;

				// IF this is buying and that isn't then consider this greater.
				return 1;
			}

			// This is selling
			if (_value < 0)
			{
				// That is selling
				if (otherValue < 0)
					return _value > otherValue ? -1 : 1;

				// If this is selling and that isn't, then consider this smaller.
				return -1;
			}

			return 1;
		}
	}
}
