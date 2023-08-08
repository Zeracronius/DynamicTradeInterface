using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.Defs;
using DynamicTradeInterface.InterfaceComponents.TableBox;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DynamicTradeInterface.UserInterface
{
	[HotSwappable]
	internal class Dialog_TradeConfiguration : Window
	{
		public event EventHandler<bool> OnClosed;

		const float COLUMN_BUTTON_SIZE = 30f;
		static Vector2 MAIN_BUTTON_SIZE = new Vector2(160f, 40f);

		Table<TableRow<TradeColumnDef>>? _selectedColumnsTable;
		Table<TableRow<TradeColumnDef>>? _availableColumnsTable;
		Mod.DynamicTradeInterfaceSettings _settings;
		HashSet<TradeColumnDef> _validColumnDefs;
		List<TradeColumnDef> _visibleColumns;
		string _cancelButtonText;
		string _acceptButtonText;
		string _windowTitle;

		float _headerHeight;

		public bool Accepted { get; private set; }

		public Dialog_TradeConfiguration()
		{
			_settings = Mod.DynamicTradeInterfaceMod.Settings;
			_validColumnDefs = _settings.ValidColumns;
			_visibleColumns = _settings.VisibleColumns;
			forcePause = true;
			resizeable = true;
			draggable = true;
			forcePause = true;
			absorbInputAroundWindow = true;
		}

		public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.5f, UI.screenHeight * 0.8f);

		public override void PostOpen()
		{
			base.PostOpen();

			try
			{
				_acceptButtonText = "AcceptButton".Translate();
				_cancelButtonText = "CancelButton".Translate();
				_windowTitle = "Trade window Configuration";
				_headerHeight = Text.LineHeightOf(GameFont.Medium) + GenUI.GapSmall;


				_selectedColumnsTable = InitializeTable(_visibleColumns);
				_availableColumnsTable = InitializeTable(_validColumnDefs.Except(_visibleColumns));

				_selectedColumnsTable.Caption = "Selected columns";
				_selectedColumnsTable.AllowSorting = false;

				_availableColumnsTable.Caption = "Available columns";

				_selectedColumnsTable.Refresh();
				_availableColumnsTable.Refresh();
			}
			catch
			{
				Close();
				throw;
			}
		}

		private Table<TableRow<TradeColumnDef>> InitializeTable(IEnumerable<TradeColumnDef> rows)
		{
			Table<TableRow<TradeColumnDef>> result = new Table<TableRow<TradeColumnDef>>((row, value) => row.SearchString.Contains(value));
			result.MultiSelect = true;
			result.DrawBorder = true;

			TableColumn<TableRow<TradeColumnDef>> colDef = result.AddColumn("Def", 0.5f);
			colDef.IsFixedWidth = false;
			TableColumn<TableRow<TradeColumnDef>> colLabel = result.AddColumn("Caption", 0.5f);
			colLabel.IsFixedWidth = false;

			foreach (var item in rows)
			{
				var row = new TableRow<TradeColumnDef>(item, item.defName + " " + item.label);
				row[colDef] = item.defName;
				row[colLabel] = item.LabelCap;
				result.AddRow(row);
			}
			return result;
		}

		public override void DoWindowContents(Rect inRect)
		{
			inRect.SplitHorizontallyWithMargin(out Rect header, out Rect body, out _, GenUI.GapTiny, topHeight: _headerHeight);

			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(header, _windowTitle);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;

			body.SplitHorizontallyWithMargin(out body, out Rect footer, out _, GenUI.GapTiny, bottomHeight: MAIN_BUTTON_SIZE.y + GenUI.GapTiny);

			float margin = COLUMN_BUTTON_SIZE + GenUI.GapSmall;
			body.SplitVerticallyWithMargin(out Rect left, out Rect right, out _, margin, leftWidth: inRect.width / 2 - margin / 2);

			_selectedColumnsTable!.Draw(left);
			_availableColumnsTable!.Draw(right);

			Rect buttonRect = new Rect(left.xMax + GenUI.GapSmall / 2, left.center.y - (COLUMN_BUTTON_SIZE * 2), COLUMN_BUTTON_SIZE, COLUMN_BUTTON_SIZE);

			if (Widgets.ButtonImage(buttonRect, TexButton.ReorderUp))
			{
				IReadOnlyList<TableRow<TradeColumnDef>> selectedRows = _selectedColumnsTable.SelectedRows;
				IList<TableRow<TradeColumnDef>> rows = _selectedColumnsTable.RowItems;
				for (int i = 0; i < rows.Count; i++)
				{
					TableRow<TradeColumnDef> row = rows[i];
					if (i > 0 && selectedRows.Contains(row))
					{
						rows[i] = rows[i - 1];
						rows[i - 1] = row;
					}
				}
				_selectedColumnsTable.Refresh();
			}
			buttonRect.y += COLUMN_BUTTON_SIZE + GenUI.GapTiny;

			if (Widgets.ButtonImage(buttonRect, TexButton.Plus))
			{
				IReadOnlyList<TableRow<TradeColumnDef>> selectedRows = _availableColumnsTable.SelectedRows;
				if (selectedRows.Count > 0)
				{
					for (int i = 0; i < selectedRows.Count; i++)
					{
						TableRow<TradeColumnDef> row = selectedRows[i];
						_availableColumnsTable.DeleteRow(row);
						_selectedColumnsTable.AddRow(row);


						for (int columnIndex = 0; columnIndex < _availableColumnsTable.Columns.Count; columnIndex++)
						{
							var sourceColumn = _availableColumnsTable.Columns[columnIndex];
							var targetColumn = _selectedColumnsTable.Columns[columnIndex];
							row[targetColumn] = row[sourceColumn];
						}
					}
					_availableColumnsTable.ClearSelection();

					_selectedColumnsTable.Refresh();
					_availableColumnsTable.Refresh();
				}
			}
			buttonRect.y += COLUMN_BUTTON_SIZE + GenUI.GapTiny;

			if (Widgets.ButtonImage(buttonRect, TexButton.Minus))
			{
				IReadOnlyList<TableRow<TradeColumnDef>> selectedRows = _selectedColumnsTable.SelectedRows;
				if (selectedRows.Count > 0)
				{
					for (int i = 0; i < selectedRows.Count; i++)
					{
						TableRow<TradeColumnDef> row = selectedRows[i];
						_selectedColumnsTable.DeleteRow(row);
						_availableColumnsTable.AddRow(row);

						for (int columnIndex = 0; columnIndex < _selectedColumnsTable.Columns.Count; columnIndex++)
						{
							var sourceColumn = _selectedColumnsTable.Columns[columnIndex];
							var targetColumn = _availableColumnsTable.Columns[columnIndex];
							row[targetColumn] = row[sourceColumn];
						}
					}
					_selectedColumnsTable.ClearSelection();

					_selectedColumnsTable.Refresh();
					_availableColumnsTable.Refresh();
				}
			}
			buttonRect.y += COLUMN_BUTTON_SIZE + GenUI.GapTiny;

			if (Widgets.ButtonImage(buttonRect, TexButton.ReorderDown))
			{
				IReadOnlyList<TableRow<TradeColumnDef>> selectedRows = _selectedColumnsTable.SelectedRows;
				IList<TableRow<TradeColumnDef>> rows = _selectedColumnsTable.RowItems;
				for (int i = rows.Count - 1; i >= 0; i--)
				{
					TableRow<TradeColumnDef> row = rows[i];
					if (i < rows.Count - 1 && selectedRows.Contains(row))
					{
						rows[i] = rows[i + 1];
						rows[i + 1] = row;
					}
				}
				_selectedColumnsTable.Refresh();
			}


			float width = MAIN_BUTTON_SIZE.x * 2 + GenUI.GapTiny;
			Rect mainButtonRect = new Rect(footer.center.x - width / 2, footer.yMax - GenUI.GapTiny - MAIN_BUTTON_SIZE.y, MAIN_BUTTON_SIZE.x, MAIN_BUTTON_SIZE.y);

			// Accept
			if (Widgets.ButtonText(mainButtonRect, _acceptButtonText))
			{
				_settings.VisibleColumns.Clear();
				_settings.VisibleColumns.AddRange(_selectedColumnsTable.RowItems.Select(x => x.RowObject));
				Accepted = true;
				Close();
				OnClosed?.Invoke(this, Accepted);
			}
			mainButtonRect.x += mainButtonRect.width + GenUI.GapTiny;

			// Reset
			if (Widgets.ButtonText(mainButtonRect, _cancelButtonText))
			{
				Accepted = false;
				Close();
				OnClosed?.Invoke(this, Accepted);
			}
		}
	}
}
