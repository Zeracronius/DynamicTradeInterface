using DynamicTradeInterface.InterfaceComponents.TableBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.OleDb;
using System.Linq;

namespace DynamicTradeInterface.Collections
{
	internal class ListFilter<T>
	{
		private struct SortingEntry
		{
			public bool Ascending;
			public object? Key;
			public Func<T, IComparable> SortFunction;

			public SortingEntry(Func<T, IComparable> sortFunction, bool ascending, object? key)
			{
				Ascending = ascending;
				Key = key;
				SortFunction = sortFunction;
			}
		}

		ReadOnlyCollection<T> _filteredCollection;
		List<T> _totalCollection;
		List<T> _bufferList;
		Func<T, string, bool> _filterCallback;
		string? _filterString;
		Queue<SortingEntry> _sortingQueue;
		Queue<SortingEntry> _sortingQueueBuffer;

		/// <summary>
		/// Gets the filtered collection of items.
		/// </summary>
		public ReadOnlyCollection<T> Filtered => _filteredCollection;

		/// <summary>
		/// Gets or sets the collection with all items.
		/// </summary>
		public IList<T> Items
		{
			get => _totalCollection;
			set
			{
				_totalCollection = value.ToList();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the filter string parsed to the filter callback when filtering collection.
		/// </summary>
		public string? Filter
		{
			get => _filterString;
			set
			{
				if (_filterString == value || _filterString != null && _filterString.Equals(value))
					return;

				_filterString = value?.ToLower();
				Invalidate();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListFilter{T}"/> class.
		/// </summary>
		/// <param name="collection">Initial collection that is copied.</param>
		/// <param name="filterCallback">Filter callback called for each item when filter text is modified. Provides Item, Filtertext and expects a bool returned on whether or not item is visible.</param>
		public ListFilter(IEnumerable<T> collection, Func<T, string, bool> filterCallback)
		{
			_totalCollection = collection.ToList();
			_bufferList = new List<T>();
			_filterCallback = filterCallback;
			_sortingQueue = new Queue<SortingEntry>();
			_sortingQueueBuffer = new Queue<SortingEntry>();
			_filteredCollection = _totalCollection.AsReadOnly();
			Invalidate();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListFilter{T}"/> class.
		/// </summary>
		/// <param name="filterCallback">Filter callback called for each item when filter text is modified. Provides Item, Filtertext and expects a bool returned on whether or not item is visible.</param>
		public ListFilter(Func<T, string, bool> filterCallback)
		{
			_totalCollection = new List<T>();
			_bufferList = new List<T>();
			_filterCallback = filterCallback;
			_sortingQueue = new Queue<SortingEntry>();
			_sortingQueueBuffer = new Queue<SortingEntry>();
			_filteredCollection = _totalCollection.AsReadOnly();
		}

		/// <summary>
		/// Orders underlying collection ascending.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		/// <param name="reset">Whether to reset sorting stack or not.</param>
		public void OrderBy(Func<T, IComparable> keySelector, bool reset, object? key = null)
		{
			AddOrdering(keySelector, true, reset, key);
		}

		/// <summary>
		/// Orders underlying collection descending.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		/// <param name="reset">Whether to reset sorting stack or not.</param>
		public void OrderByDescending(Func<T, IComparable> keySelector, bool reset, object? key = null)
		{
			AddOrdering(keySelector, false, reset, key);
		}


		public SortDirection GetSortingDirection(object key)
		{
			foreach (SortingEntry sortEntry in _sortingQueue)
			{
				if (sortEntry.Key == key)
					return sortEntry.Ascending ? SortDirection.Ascending : SortDirection.Descending;
			}
			return SortDirection.None;
		}

		public void ClearSorting(object? key)
		{
			if (key != null)
			{
				// Remove specific key from sort queue.
				_sortingQueueBuffer.Clear();
				while (_sortingQueue.TryDequeue(out SortingEntry entry))
				{
					if (entry.Key == key)
						continue;

					_sortingQueueBuffer.Enqueue(entry);
				}
				(_sortingQueue, _sortingQueueBuffer) = (_sortingQueueBuffer, _sortingQueue);
			}
			else
			{
				// Clear entire sort queue.
				_sortingQueue.Clear();
			}
			Invalidate();
		}

		/// <summary>
		/// Iterates and returns the sorting queue.
		/// </summary>
		public IEnumerable<(object, bool)> GetSortings()
		{
			foreach (SortingEntry item in _sortingQueue)
			{
				if (item.Key == null)
					continue;

				yield return (item.Key, item.Ascending);
			}
		}

		private void AddOrdering(Func<T, IComparable> keySelector, bool ascending, bool reset, object? key = null)
		{
			if (reset)
				_sortingQueue.Clear();

			_sortingQueueBuffer.Clear();

			bool exists = false;
			if (key != null)
			{
				while (_sortingQueue.TryDequeue(out SortingEntry entry))
				{
					if (entry.Key == key)
					{
						// Found existing sorting entry by key, update ascending value and block inserting new entry.
						exists = true;
						entry.Ascending = ascending;
					}

					_sortingQueueBuffer.Enqueue(entry);
				}

				(_sortingQueue, _sortingQueueBuffer) = (_sortingQueueBuffer, _sortingQueue);
			}

			if (exists == false)
				_sortingQueue.Enqueue(new SortingEntry(keySelector, ascending, key));

			Invalidate();
		}

		/// <summary>
		/// Invalidates the filtered collection and regenerates it.
		/// </summary>
		public void Invalidate()
		{
			int sortingCount = _sortingQueue.Count;
			IEnumerable<T> rowSource = _totalCollection;
			if (sortingCount > 0)
			{
				var first = _sortingQueue.First();
				IOrderedEnumerable<T> orderedEnumeration = first.Ascending ?
					rowSource.OrderBy(first.SortFunction) : rowSource.OrderByDescending(first.SortFunction);
				bool ignoreFirst = true;
				foreach (SortingEntry item in _sortingQueue)
				{
					if (ignoreFirst)
					{
						ignoreFirst = false;
						continue;
					}
					if (item.Ascending)
						orderedEnumeration = orderedEnumeration.ThenBy(item.SortFunction);
					else
						orderedEnumeration = orderedEnumeration.ThenByDescending(item.SortFunction);
				}
				rowSource = orderedEnumeration;
			}

			if (string.IsNullOrEmpty(_filterString) == false)
			{
				rowSource = rowSource.Where(x => _filterCallback(x, _filterString ?? string.Empty));
			}
			
			_bufferList.Clear();
			_bufferList.AddRange(rowSource);
			_filteredCollection = _bufferList.AsReadOnly();
		}
	}
}
