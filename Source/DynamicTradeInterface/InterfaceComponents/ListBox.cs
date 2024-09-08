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
		private const float ROW_SPACING = 3;

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

			float width = inRect.width - GenUI.ScrollBarWidth - ROW_SPACING;
			Rect rowRect = new Rect(0, 0, width, Text.LineHeight + ROW_SPACING);
			Rect listbox = new Rect(0, 0, width, (_collection.Count + 1) * rowRect.height);

			try
			{
				Widgets.BeginScrollView(inRect, ref _scrollPosition, listbox, true);

				rowRect.y = _scrollPosition.y;

				T currentRow;
				// Get index of first row visible in scrollbox
				int currentIndex = Mathf.FloorToInt(_scrollPosition.y / rowRect.height);
				int rowCount = _collection.Count;
				for (; currentIndex < rowCount; currentIndex++)
				{
					currentRow = _collection[currentIndex];

					callback?.Invoke(rowRect, currentRow);

					rowRect.y += rowRect.height;

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
