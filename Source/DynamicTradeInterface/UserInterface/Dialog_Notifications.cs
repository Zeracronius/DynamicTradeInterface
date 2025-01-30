using DynamicTradeInterface.InterfaceComponents;
using DynamicTradeInterface.Mod;
using DynamicTradeInterface.Notifications;
using DynamicTradeInterface.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface
{
	internal class Dialog_Notifications : Window
	{
		private string? _windowTitle;
		private string? _filterToThisTooltip;
		private string? _toggleNotificationTooltip;
		private string? _addTooltip;
		private string? _removeTooltip;
		private string? _newRowText;
		private ListBox<NotificationEntry>? _notificationListBox;
		private Vector2 _initialPosition;
		private Action<string>? _applyFilterCallback;
		private InterfaceComponents.Notifications _notifications;

        public Dialog_Notifications(Vector2 initialPosition, Action<string> applyFilterCallback, InterfaceComponents.Notifications notifications)
        {
			resizeable = true;
			closeOnClickedOutside = true;
			doCloseButton = false;
			doCloseX = false;
			absorbInputAroundWindow = true;
			_initialPosition = initialPosition;
			_applyFilterCallback = applyFilterCallback;
			_notifications = notifications;
			soundClose = null;
		}

		public override void PreOpen()
		{
			base.PreOpen();



			_windowTitle = "DynamicTradeWindowNotificationsTitle".Translate();
			_filterToThisTooltip = "DynamicTradeWindowNotificationsScopeFilter".Translate();
			_toggleNotificationTooltip = "DynamicTradeWindowNotificationsEnable".Translate();
			_addTooltip = "DynamicTradeWindowNotificationsAdd".Translate();
			_removeTooltip = "DynamicTradeWindowNotificationsDelete".Translate();

			var notifications = GameSettings.Notifications;
			_notificationListBox = new ListBox<NotificationEntry>(notifications);
			_notificationListBox.RowSpacing = 7;
			base.windowRect.width = 400;
			base.windowRect.height = Text.LineHeight * 20;
			//base.windowRect.x = _initialPosition.x; // - (windowRect.width / 2);
			//base.windowRect.y = _initialPosition.y; // + (windowRect.height / 2);

			Vector2 mousePosition = UI.MousePositionOnUIInverted;

			if (mousePosition.x + InitialSize.x > (float)UI.screenWidth)
				mousePosition.x = (float)UI.screenWidth - InitialSize.x;

			if (mousePosition.y > (float)UI.screenHeight)
				mousePosition.y = UI.screenHeight;

			base.windowRect.x = mousePosition.x;
			base.windowRect.y = mousePosition.y + 20;
		}

		public override void PreClose()
		{
			base.PreClose();
		}


		public override void DoWindowContents(Rect inRect)
		{
			if (Event.current.type == EventType.Layout) // this gets sent every frame but can only draw behind every window
				return;

			inRect = inRect.ContractedBy(GenUI.GapTiny);
			inRect.SplitHorizontallyWithMargin(out Rect top, out Rect bottom, out _, GenUI.GapTiny, Constants.SQUARE_BUTTON_SIZE);


			//if (Mouse.IsOver(newButtonRect))
			//	TooltipHandler.TipRegion(newButtonRect, _addTooltip);

			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(top, _windowTitle);
			Text.Anchor = TextAnchor.UpperLeft;
			
			bottom.SplitHorizontallyWithMargin(out Rect listRect, out Rect newRowRect, out _, GenUI.GapTiny, bottomHeight: Text.LineHeight);

			float height = 0;
			_notificationListBox?.Draw(listRect, out height, DrawNotificationLine);

			newRowRect.y = listRect.y + height + GenUI.GapTiny;
			DrawNewRowLine(newRowRect);
		}

		private void DrawNewRowLine(Rect inRect)
		{
			inRect.SplitVerticallyWithMargin(out Rect left, out Rect right, out _, GenUI.GapTiny, leftWidth: inRect.height);
			if (Widgets.ButtonImage(left, Textures.Plus, tooltip: _addTooltip))
			{
				if (String.IsNullOrWhiteSpace(_newRowText) == false)
				{
					NewNotification(_newRowText!);
					_newRowText = "";
				}
			}

			_newRowText = Widgets.TextField(right, _newRowText);
		}

		private void DrawNotificationLine(Rect rect, NotificationEntry entry)
		{
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;

			// Left-most
			float rowHeight = rect.height;

			float width = _applyFilterCallback != null ? rowHeight : 0;
			Rect inspectButtonRect = new Rect(rect.x, rect.y, width, rowHeight);

			if (_applyFilterCallback != null)
			{
				if (entry.Active)
				{
					Text.Font = GameFont.Medium;
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(inspectButtonRect, _notifications[entry]);
					Text.Anchor = TextAnchor.UpperLeft;
					Text.Font = GameFont.Small;

					if (Mouse.IsOver(inspectButtonRect))
					{
						TooltipHandler.TipRegion(inspectButtonRect, _filterToThisTooltip);
						Widgets.DrawHighlight(inspectButtonRect);
					}

					if (Widgets.ButtonInvisible(inspectButtonRect))
						ApplyFilter(entry);
				}
				else
				{
					if (Widgets.ButtonImage(inspectButtonRect, Textures.Inspect, tooltip: _filterToThisTooltip))
						ApplyFilter(entry);
				}
			}

			// Right of inspect button
			Rect checkboxRect = new Rect(inspectButtonRect.xMax + GenUI.GapTiny, rect.y, rowHeight, rowHeight);

			// Right-most
			Rect deleteRect = new Rect(rect.xMax - rowHeight, rect.y, rowHeight, rowHeight);

			// Place between checkbox and delete
			Rect textboxRect = new Rect(checkboxRect.xMax + GenUI.GapTiny, rect.y, deleteRect.x - (checkboxRect.xMax + GenUI.GapTiny), rowHeight);


			bool active = entry.Active;
			Widgets.Checkbox(checkboxRect.x, checkboxRect.y, ref entry.Active);

			if (Mouse.IsOver(checkboxRect))
				TooltipHandler.TipRegion(checkboxRect, _toggleNotificationTooltip);

			if (active != entry.Active)
				_notifications.Refresh();

			rect.width -= Constants.SQUARE_BUTTON_SIZE + GenUI.GapTiny;
			string entryText = Widgets.TextField(textboxRect, entry.RegExText);
			if (entryText != entry.RegExText)
			{
				entry.SetText(entryText);
				_notifications.Refresh(entry);
			}

			if (Widgets.ButtonImage(deleteRect, Textures.Minus, tooltip: _removeTooltip))
				DeleteNotification(entry);
		}

		private void NewNotification(string value)
		{
			GameSettings.Notifications.Add(new(value));
		}

		private void ApplyFilter(NotificationEntry entry)
		{
			if (string.IsNullOrEmpty(entry.RegExText) == false)
				_applyFilterCallback?.Invoke(entry.RegExText!);
		}

		private void DeleteNotification(NotificationEntry entry)
		{
			GameSettings.Notifications.Remove(entry);
		}
	}
}
