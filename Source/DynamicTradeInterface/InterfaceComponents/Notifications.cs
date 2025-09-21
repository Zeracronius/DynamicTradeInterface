using DynamicTradeInterface.InterfaceComponents.TableBox;
using DynamicTradeInterface.Notifications;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTradeInterface.InterfaceComponents
{
	internal class Notifications
	{
		IEnumerable<NotificationEntry> _entries;
		Dictionary<NotificationEntry, (int, string)> _notifications;
		List<TableRow<Tradeable>> _wares;

		public int TotalHits { get; private set; }
		public string TotalHitsText { get; private set; }

		public Notifications(IEnumerable<NotificationEntry> notifications)
        {
			_entries = notifications;
			_notifications = new Dictionary<NotificationEntry, (int, string)>();
			_wares = new List<TableRow<Tradeable>>();
			TotalHitsText = "";
			TotalHits = 0;
		}

		public string GetCombinedRegEx()
		{
			string combinedRegEx = string.Empty;
			foreach (var item in _entries)
			{
				if (item.Active)
				{
					if (combinedRegEx.Length > 0)
						combinedRegEx += "|";

					combinedRegEx += $"({item.RegExText})";
				}
			}

			return combinedRegEx;
		}

		public string this[NotificationEntry entry]
		{
			get
			{
				_notifications.TryGetValue(entry, out (int, string) hits);
				return hits.Item2;
			}
		}

		/// <summary>
		/// Refreshes the number of hits for each notification across provided wares.
		/// </summary>
		/// <param name="allWares">The wares to check notifications against.</param>
		public void Load(IEnumerable<TableRow<Tradeable>> allWares)
		{
			_wares.Clear();
			_wares.AddRange(allWares);
			Refresh();
		}

		/// <summary>
		/// Refreshes all notifications.
		/// </summary>
		public void Refresh()
		{
			TotalHits = 0;
			foreach (NotificationEntry entry in _entries)
			{
				int hits = 0;
				if (entry.Active && String.IsNullOrEmpty(entry.RegExText) == false)
				{
					foreach (TableRow<Tradeable> row in _wares)
					{
						if (ApplySearch(row, entry))
							hits++;
					}
				}
				TotalHits += hits;
				_notifications[entry] = (hits, hits.ToString());
			}

			TotalHitsText = TotalHits.ToString();
		}

		/// <summary>
		/// Refreshes the specified notification.
		/// </summary>
		/// <param name="entry">The entry.</param>
		public void Refresh(NotificationEntry entry)
		{
			int hits = 0;
			if (entry.Active && String.IsNullOrEmpty(entry.RegExText) == false)
			{
				// Recalculate single
				foreach (TableRow<Tradeable> row in _wares)
				{
					if (ApplySearch(row, entry))
						hits++;
				}
			}
			_notifications[entry] = (hits, hits.ToString());

			// Update totals
			TotalHits = _notifications.Sum(x => x.Value.Item1);
			TotalHitsText = TotalHits.ToString();
		}


		private bool ApplySearch(TableRow<Tradeable> row, NotificationEntry entry)
		{
			if (entry.Regex != null)
				return entry.Regex.IsMatch(row.SearchString);

			return row.SearchString.Contains(entry.RegExText);
		}
	}
}
