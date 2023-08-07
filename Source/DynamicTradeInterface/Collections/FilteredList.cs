using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DynamicTradeInterface.Collections
{
	internal class ListFilter<T>
	{
		public delegate void OnSortEventHandler(IEnumerable<T> originalCollection, ref IOrderedEnumerable<T>? ordering);
		public event OnSortEventHandler OnSorting;

		ReadOnlyCollection<T> _filteredCollection;
		List<T> _totalCollection;
		List<T> _bufferList;
		Func<T, string, bool> _filterCallback;
		string _filterString;
		bool _sorting;

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
		public string Filter
		{
			get => _filterString;
			set
			{
				if (_filterString == value || _filterString != null && _filterString.Equals(value))
					return;

				_filterString = value.ToLower();
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
			_sorting = false;
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
			_sorting = false;
		}

		/// <summary>
		/// Orders underlying collection ascending.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		public void OrderBy<TKey>(Func<T, TKey> keySelector) where TKey : IComparable
		{
			_sorting = true;
			_bufferList.Clear();

			IOrderedEnumerable<T>? ordering = null;
			OnSorting?.Invoke(_totalCollection, ref ordering);

			if (ordering != null)
				_bufferList.AddRange(ordering.ThenByDescending(keySelector));
			else
				_bufferList.AddRange(_totalCollection.OrderByDescending(keySelector));

			List<T> old = _totalCollection;
			_totalCollection = _bufferList;
			_bufferList = old;
			Invalidate();
			_sorting = false;
		}

		/// <summary>
		/// Orders underlying collection descending.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		public void OrderByDescending<TKey>(Func<T, TKey> keySelector) where TKey : IComparable
		{
			_sorting = true;
			_bufferList.Clear();

			IOrderedEnumerable<T>? ordering = null;
			OnSorting?.Invoke(_totalCollection, ref ordering);

			if (ordering != null) 
				_bufferList.AddRange(ordering.ThenByDescending(keySelector));
			else
				_bufferList.AddRange(_totalCollection.OrderByDescending(keySelector));

			List<T> old = _totalCollection;
			_totalCollection = _bufferList;
			_bufferList = old;
			Invalidate();
			_sorting = false;
		}

		/// <summary>
		/// Invalidates the filtered collection and regenerates it.
		/// </summary>
		public void Invalidate()
		{
			if (_sorting == false)
			{
				IOrderedEnumerable<T>? ordering = null;
				OnSorting?.Invoke(_totalCollection, ref ordering);
				if (ordering != null)
				{
					_bufferList.Clear();
					_bufferList.AddRange(ordering);

					List<T> old = _totalCollection;
					_totalCollection = _bufferList;
					_bufferList = old;
				}
			}
			ApplyFilter();
		}

		private void ApplyFilter()
		{
			if (string.IsNullOrEmpty(_filterString))
			{
				_filteredCollection = _totalCollection.AsReadOnly();
				return;
			}

			_bufferList.Clear();
			_bufferList.AddRange(_totalCollection.Where(x => _filterCallback(x, _filterString)));
			_filteredCollection = _bufferList.AsReadOnly();
		}
	}
}
