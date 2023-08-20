using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents
{
	public static class DragSelect
	{
		private static bool _dragSelectMod = false;
		private static bool _painting = false;
		private static PaintingDirection _paintingDirection = PaintingDirection.None;

		public enum PaintingDirection
		{
			None,
			Left,
			Middle,
			Right,
		}

		public static void Initialize()
		{
			_dragSelectMod = ModsConfig.IsActive("telardo.DragSelect");
		}

		public static void Reset()
		{
			if (_dragSelectMod == false)
				return;

			if (Event.current.rawType == EventType.MouseUp || Input.GetMouseButtonUp(0))
			{
				_painting = false;
				_paintingDirection = PaintingDirection.None;
			}
		}

		public static bool IsPainting(Rect rect, PaintingDirection direction)
		{
			if (_dragSelectMod == false)
				return false;

			if (!_painting && Widgets.ButtonInvisibleDraggable(rect) == Widgets.DraggableResult.Dragged)
			{
				_painting = true;
				_paintingDirection = direction;
			}

			return _painting && _paintingDirection == direction && Mouse.IsOver(rect);
		}
	}
}
