using DynamicTradeInterface.Attributes;
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
using Verse.Noise;

namespace DynamicTradeInterface.UserInterface
{
	[HotSwappable]
	internal class Dialog_Notifications : Window
	{
		private string? _windowTitle;
		private string? _filterToThisTooltip;
		private string? _toggleNotificationTooltip;
		private string? _addTooltip;
		private string? _removeTooltip;
		private string? _newRowText;
		private ListBox<NotificationEntry>? _notificationListBoxCurrent;
		private ListBox<NotificationEntry>? _notificationListBoxShared;
		private ListBox<NotificationEntry>? _notificationListBox;
		private Action<string>? _applyFilterCallback;
		private InterfaceComponents.Notifications _notifications;
		private int selectedTab = 0;
		private List<TabRecord> _tabs = new List<TabRecord>();

		public Dialog_Notifications(Action<string> applyFilterCallback, InterfaceComponents.Notifications notifications)
        {
			resizeable = true;
			closeOnClickedOutside = true;
			doCloseButton = false;
			doCloseX = false;
			absorbInputAroundWindow = true;
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

			_notificationListBoxCurrent = new ListBox<NotificationEntry>(GameSettings.Notifications);
			_notificationListBoxCurrent.RowSpacing = 7;

			_notificationListBoxShared = new ListBox<NotificationEntry>(DynamicTradeInterfaceMod.Settings.Notifications);
			_notificationListBoxShared.RowSpacing = 7;

			_notificationListBox = _notificationListBoxCurrent;


			base.windowRect.width = 400;
			base.windowRect.height = Text.LineHeight * 20;

			Vector2 mousePosition = UI.MousePositionOnUIInverted;

			if (mousePosition.x + InitialSize.x > (float)UI.screenWidth)
				mousePosition.x = (float)UI.screenWidth - InitialSize.x;

			if (mousePosition.y > (float)UI.screenHeight)
				mousePosition.y = UI.screenHeight;

			base.windowRect.x = mousePosition.x;
			base.windowRect.y = mousePosition.y + 20;

			_tabs.Add(new("DynamicTradeWindowNotificationsTabsCurrent".Translate(), 
				() => _notificationListBox = _notificationListBoxCurrent, 
				() => _notificationListBox == _notificationListBoxCurrent));

			_tabs.Add(new("DynamicTradeWindowNotificationsTabsShared".Translate(), 
				() => _notificationListBox = _notificationListBoxShared, 
				() => _notificationListBox == _notificationListBoxShared));
		}

		public override void PreClose()
		{
			base.PreClose();
		}


		public override void DoWindowContents(Rect inRect)
		{
			if (Event.current.type == EventType.Layout) // this gets sent every frame but can only draw behind every window
				return;

			inRect.SplitHorizontallyWithMargin(out Rect top, out Rect bottom, out _, GenUI.GapTiny, Constants.SQUARE_BUTTON_SIZE);

			DrawTitle(top);

			bottom.SplitHorizontallyWithMargin(out Rect tabs, out bottom, out _, topHeight: TabDrawer.TabHeight);
			Widgets.DrawMenuSection(bottom);
			tabs.height += GenUI.GapTiny - 1;

			Widgets.BeginGroup(tabs);
			TabDrawer.DrawTabs(tabs, _tabs, tabs.width);
			Widgets.EndGroup();


			bottom.ContractedBy(GenUI.GapSmall).SplitHorizontallyWithMargin(out Rect listRect, out Rect newRowRect, out _, GenUI.GapTiny, bottomHeight: Text.LineHeight);

			float height = 0;
			_notificationListBox?.Draw(listRect, out height, DrawNotificationLine);

			newRowRect.y = listRect.y + height + GenUI.GapTiny;
			DrawNewRowLine(newRowRect);
		}


		private void DrawTitle(Rect inRect)
		{
			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(inRect, _windowTitle);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawNewRowLine(Rect inRect)
		{
			inRect.SplitVerticallyWithMargin(out Rect left, out Rect right, out _, GenUI.GapTiny, leftWidth: inRect.height);
			if (Widgets.ButtonImage(left, Textures.Plus))
			{
				if (String.IsNullOrWhiteSpace(_newRowText) == false)
				{
					NewNotification(_newRowText!);
					_newRowText = "";
				}
			}

			if (Mouse.IsOver(left))
				TooltipHandler.TipRegion(left, _addTooltip);

			_newRowText = Widgets.TextField(right, _newRowText);
		}

		private void DrawNotificationLine(Rect rect, NotificationEntry entry)
		{
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;

			float rowHeight = rect.height;

			// Left-most
			Rect checkboxRect = new Rect(rect.x, rect.y, rowHeight, rowHeight);

			// Right of checkbox button
			float width = _applyFilterCallback != null ? rowHeight : 0;
			Rect inspectButtonRect = new Rect(checkboxRect.xMax + GenUI.GapTiny, rect.y, width, rowHeight);

			// Right-most
			Rect deleteRect = new Rect(rect.xMax - rowHeight, rect.y, rowHeight, rowHeight);

			// Place between inspect and delete
			Rect textboxRect = new Rect(inspectButtonRect.xMax + GenUI.GapTiny, rect.y, deleteRect.x - (inspectButtonRect.xMax + GenUI.GapTiny), rowHeight);

			bool active = entry.Active;
			Widgets.Checkbox(checkboxRect.x, checkboxRect.y, ref entry.Active, checkboxRect.height, texChecked: Textures.CheckboxOn, texUnchecked: Textures.CheckboxOff);

			if (_applyFilterCallback != null)
			{
				if (entry.Active)
				{
					GameFont fontSize = GameFont.Medium;
					if (_notifications.TotalHits > 9)
						fontSize = GameFont.Small;

					Text.Font = fontSize;
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
					if (Widgets.ButtonImage(inspectButtonRect, Textures.Inspect))
						ApplyFilter(entry);
				}
			}

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

			if (Widgets.ButtonImage(deleteRect, Textures.Remove))
				DeleteNotification(entry);

			if (Mouse.IsOver(deleteRect))
				TooltipHandler.TipRegion(deleteRect, _removeTooltip);
		}

		private void NewNotification(string value)
		{
			NotificationEntry entry = new(value);
			_notificationListBox?.Items.Add(entry);
			_notifications.Refresh(entry);
		}

		private void ApplyFilter(NotificationEntry entry)
		{
			if (string.IsNullOrEmpty(entry.RegExText) == false)
				_applyFilterCallback?.Invoke(entry.RegExText!);
		}

		private void DeleteNotification(NotificationEntry entry)
		{
			_notificationListBox?.Items.Remove(entry);
			_notifications.Refresh(entry);
		}
	}
}
