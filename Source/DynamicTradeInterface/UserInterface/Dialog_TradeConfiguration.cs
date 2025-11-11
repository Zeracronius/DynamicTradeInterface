using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.Defs;
using DynamicTradeInterface.InterfaceComponents;
using DynamicTradeInterface.InterfaceComponents.TableBox;
using DynamicTradeInterface.InterfaceComponents.TreeBox;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
		public event EventHandler<bool>? OnClosed;

		const float COLUMN_BUTTON_SIZE = 30f;
		static Vector2 MAIN_BUTTON_SIZE = new Vector2(160f, 40f);

		Table<TableRow<TradeColumnDef>>? _selectedColumnsTable;
		Table<TableRow<TradeColumnDef>>? _availableColumnsTable;
		Mod.DynamicTradeInterfaceSettings _settings;
		HashSet<TradeColumnDef> _validColumnDefs;
		List<TradeColumnDef> _visibleColumns;
		string? _cancelButtonText;
		string? _acceptButtonText;
		string? _windowTitle;

		float _headerHeight;

		FilterTreeBox? _optionsBox;

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
				_windowTitle = "ConfigurationWindowTitle".Translate();

				List<TreeNode_FilterBox> options = new List<TreeNode_FilterBox>
				{
					new TreeNode_FilterBox("ConfigurationWindowEnableProfiling".Translate(), null,
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.ProfilingEnabled, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowHideUnwilling".Translate(), "ConfigurationWindowHideUnwillingTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.ExcludeUnwillingItems, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowEnableGhostButtons".Translate(), "ConfigurationWindowEnableGhostButtonsTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.GhostButtons, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowEnableBulkDurability".Translate(), "ConfigurationWindowEnableBulkDurabilityTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.StackDurability, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowRememberSortings".Translate(), "ConfigurationWindowRememberSortingsTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.RememberSortings, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowAutoRefocus".Translate(), "ConfigurationWindowAutoRefocusTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.AutoRefocus, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowOpenAsDefault".Translate(), "ConfigurationWindowOpenAsDefaultTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.OpenAsDefault, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowPauseAfterTrade".Translate(), "ConfigurationWindowPauseAfterTradeTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.PauseAfterTrade, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowShowAvailableOnMap".Translate(), "ConfigurationWindowShowAvailableOnMapTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.ShowAvailableOnMap, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowAlternatingRowColors".Translate(), "ConfigurationWindowAlternatingRowColorsTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.AlternatingRowColor, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowDynamicButtons".Translate(), "ConfigurationWindowDynamicButtonsTooltip".Translate(),
						(in Rect x) => Widgets.Checkbox(x.xMax - x.height, x.y, ref _settings.DynamicButtons, x.height)),

					new TreeNode_FilterBox("ConfigurationWindowRowFont".Translate(), "ConfigurationWindowRowFontTooltip".Translate(),
						(in Rect x) => {
							if (Widgets.ButtonText(x, ("ConfigurationWindowFont" + _settings.RowFont).Translate()))
								Find.WindowStack.Add(
									new FloatMenu(
										Enum.GetValues(typeof(GameFont))
										.Cast<GameFont>()
										.Select(size => new FloatMenuOption(("ConfigurationWindowFont" + size).Translate(), () => _settings.RowFont = size))
										.ToList()
								));
						}),

					new TreeNode_FilterBox("ConfigurationWindowResetWidths".Translate(), null,
						(in Rect x) => {
							if (Widgets.ButtonText(x, "ConfigurationWindowResetWidths".Translate()))
								_settings.ClearColumnCustomization();
						})
					{
						HideLabel = true,
					},
				};
				_optionsBox = new FilterTreeBox(options);
				_optionsBox.ValueColumnRatio = 0.2f;

				_headerHeight = Text.LineHeightOf(GameFont.Medium) + GenUI.GapSmall;


				_selectedColumnsTable = InitializeTable(_visibleColumns);
				_availableColumnsTable = InitializeTable(_validColumnDefs.Except(_visibleColumns));

				_selectedColumnsTable.Caption = "ConfigurationWindowSelectedColumns".Translate();
				_selectedColumnsTable.SelectionChanged += Table_SelectionChanged;
				_selectedColumnsTable.AllowSorting = false;
				_selectedColumnsTable.DrawSearchBox = false;

				_availableColumnsTable.Caption = "ConfigurationWindowAvailableColumns".Translate();
				_availableColumnsTable.SelectionChanged += Table_SelectionChanged;

				_selectedColumnsTable.Refresh();
				_availableColumnsTable.Refresh();
			}
			catch
			{
				Close();
				throw;
			}
		}

		private void Table_SelectionChanged(object sender, IReadOnlyList<TableRow<TradeColumnDef>> e)
		{
			if (sender == _selectedColumnsTable)
				_availableColumnsTable?.ClearSelection();
			else
				_selectedColumnsTable?.ClearSelection();
		}

		private Table<TableRow<TradeColumnDef>> InitializeTable(IEnumerable<TradeColumnDef> rows)
		{
			Table<TableRow<TradeColumnDef>> result = new Table<TableRow<TradeColumnDef>>((row, value) => row.SearchString.Contains(value));
			result.MultiSelect = true;
			result.DrawBorder = true;

			TableColumn<TableRow<TradeColumnDef>> colDef = result.AddColumn("ConfigurationWindowColumnDef".Translate(), 0.5f);
			colDef.IsFixedWidth = false;
			TableColumn<TableRow<TradeColumnDef>> colLabel = result.AddColumn("ConfigurationWindowColumnCaption".Translate(), 0.5f);
			colLabel.IsFixedWidth = false;

			TableColumn<TableRow<TradeColumnDef>> colProfiledAvg = result.AddColumn("ConfigurationWindowColumnAvgMs".Translate(), 50f, callback: ProfilingAverageCallback);
			TableColumn<TableRow<TradeColumnDef>> colProfiledMax = result.AddColumn("ConfigurationWindowColumnMaxMs".Translate(), 50f, callback: ProfilingMaxCallback);

			foreach (var item in rows)
			{
				var row = new TableRow<TradeColumnDef>(item, item.defName + " " + item.label);
				row[colDef] = item.defName;
				row[colLabel] = item.LabelCap;

				row.Tooltip = item.description;
				result.AddRow(row);
			}
			return result;
		}


		private void ProfilingAverageCallback(ref Rect boundingBox, TableRow<TradeColumnDef> item)
		{
			if (_settings.ProfilingEnabled)
			{
				// Timings are measured in ticks, and 1ms is 10.000 ticks.
				if (_settings.TradeColumnProfilings.TryGetValue(item.RowObject, out Queue<long> timings))
					Widgets.Label(boundingBox, (timings.Average() / 10000d).ToString("N3"));
			}
		}


		private void ProfilingMaxCallback(ref Rect boundingBox, TableRow<TradeColumnDef> item)
		{
			if (_settings.ProfilingEnabled)
			{
				// Timings are measured in ticks, and 1ms is 10.000 ticks.
				if (_settings.TradeColumnProfilings.TryGetValue(item.RowObject, out Queue<long> timings))
					Widgets.Label(boundingBox, (timings.Max() / 10000d).ToString("N3"));
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (Event.current.type == EventType.Layout) // this gets sent every frame but can only draw behind every window
				return;

			inRect.SplitHorizontallyWithMargin(out Rect header, out Rect body, out _, GenUI.GapTiny, topHeight: _headerHeight);

			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(header, _windowTitle);
			Text.Font = _settings.RowFont;
			Text.Anchor = TextAnchor.UpperLeft;

			body.SplitHorizontallyWithMargin(out body, out Rect footer, out _, GenUI.GapTiny, bottomHeight: MAIN_BUTTON_SIZE.y + GenUI.GapTiny);

			body.SplitVerticallyWithMargin(out body, out Rect configurations, out _, GenUI.GapTiny, rightWidth: 250);

			Text.Anchor = TextAnchor.UpperLeft;
			float optionEntry = Text.LineHeightOf(Text.Font) + GenUI.GapTiny;

			_optionsBox?.Draw(configurations);


			float margin = COLUMN_BUTTON_SIZE + GenUI.GapSmall;
			body.SplitVerticallyWithMargin(out Rect left, out Rect right, out _, margin, leftWidth: body.width / 2 - margin / 2);
			_selectedColumnsTable!.LineFont = _settings.RowFont;
			_selectedColumnsTable!.Draw(left);

			_availableColumnsTable!.LineFont = _settings.RowFont;
			_availableColumnsTable!.Draw(right);

			Rect buttonRect = new Rect(left.xMax + GenUI.GapSmall / 2, left.center.y - (COLUMN_BUTTON_SIZE * 2), COLUMN_BUTTON_SIZE, COLUMN_BUTTON_SIZE);

			if (Widgets.ButtonImage(buttonRect, TexButton.ReorderUp))
			{
				IReadOnlyList<TableRow<TradeColumnDef>> selectedRows = _selectedColumnsTable.SelectedRows;
				IList<TableRow<TradeColumnDef>> rows = _selectedColumnsTable.RowItems;
				for (int i = 0; i < rows.Count; i++)
				{
					if (i > 0 && selectedRows.Contains(rows[i]))
					{
						(rows[i], rows[i - 1]) = (rows[i - 1], rows[i]);
					}
				}
				_selectedColumnsTable.Refresh();
			}
			buttonRect.y += COLUMN_BUTTON_SIZE + GenUI.GapTiny;

			if (Widgets.ButtonImage(buttonRect, TexButton.Plus))
			{
				// Available to Selected
				IReadOnlyList<TableRow<TradeColumnDef>> selectedRows = _availableColumnsTable.SelectedRows;
				if (selectedRows.Count > 0)
				{
					for (int i = selectedRows.Count - 1; i >= 0; i--)
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
					_selectedColumnsTable.SelectRows(selectedRows);
					_selectedColumnsTable.Refresh();
					_availableColumnsTable.Refresh();

				}
			}
			buttonRect.y += COLUMN_BUTTON_SIZE + GenUI.GapTiny;

			if (Widgets.ButtonImage(buttonRect, TexButton.Minus))
			{
				// Selected to Available
				IReadOnlyList<TableRow<TradeColumnDef>> selectedRows = _selectedColumnsTable.SelectedRows;
				if (selectedRows.Count > 0)
				{
					for (int i = selectedRows.Count - 1; i >= 0; i--)
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
					_availableColumnsTable.SelectRows(selectedRows);
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
					if (i < rows.Count - 1 && selectedRows.Contains(rows[i]))
					{
						(rows[i], rows[i + 1]) = (rows[i + 1], rows[i]);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DrawCheckbox(ref Rect boundingBox, ref bool value, string? label = "", string? tooltip = null)
		{
			Widgets.CheckboxLabeled(boundingBox, label, ref value);
			if (tooltip != null && Mouse.IsOver(boundingBox))
				TooltipHandler.TipRegion(boundingBox, tooltip);

			boundingBox.y = boundingBox.yMax;
		}
	}
}
