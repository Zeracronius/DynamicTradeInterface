using DynamicTradeInterface.InterfaceComponents.TableBox;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents
{
	/// <summary>
	/// Searchable list box.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ListBox<T>
	{
		private Vector2 _scrollPosition = new Vector2(0, 0);
		private List<T> _collection;
		public float RowSpacing { get; set; } = 3;

		public List<T> Items => _collection;

		public ListBox(List<T> collection)
		{
			_collection = collection;
		}

		/// <summary>
		/// Draws filter box.
		/// </summary>
		/// <param name="inRect">The parent rectangle.</param>
		/// <param name="x">X position.</param>
		/// <param name="y">Y position.</param>
		/// <param name="height">Filterbox height.</param>
		/// <param name="callback">The callback.</param>
		public void Draw(Rect inRect, out float height, Action<Rect, T> callback)
		{
			float curY = inRect.y;

			Text.Font = GameFont.Tiny;

			float width = inRect.width - GenUI.ScrollBarWidth - GenUI.GapTiny;
			Rect rowRect = new Rect(0, 0, width, Text.LineHeight + GenUI.GapTiny);
			float spacedRowHeight = rowRect.height + RowSpacing;
			Rect listbox = new Rect(0, 0, width, (_collection.Count + 1) * spacedRowHeight);

			try
			{
				Widgets.BeginScrollView(inRect, ref _scrollPosition, listbox, true);

				int currentIndex = Mathf.FloorToInt(_scrollPosition.y / rowRect.height);
				rowRect.y = rowRect.height * currentIndex;

				T currentRow;
				// Get index of first row visible in scrollbox
				for (; currentIndex < _collection.Count; currentIndex++)
				{
					currentRow = _collection[currentIndex];

					callback?.Invoke(rowRect, currentRow);

					rowRect.y += spacedRowHeight;

					// Break if next row starts outside bottom of scrollbox + 1 row to ensure smooth scrolling - though this should possibly not be needed for IMGUI.
					if (rowRect.y > inRect.height + _scrollPosition.y)
						break;
				}

				height = Mathf.Min(rowRect.yMax, inRect.height);
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}
	}
}
