using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicTradeInterface.Mod;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface
{
	internal class CaravanWidget
	{
		private bool _inCaravan;
		private List<Thing> _allPawnsAndItems;

		private string _visibilityExplanation;
		private Pair<float, float> _daysWorthOfFood;
		private Pair<ThingDef, float> _foragedFoodPerDay;
		private string _foragedFoodPerDayExplanation;
		private float _massCapacity;
		private string _massCapacityExplanation;
		private float _massUsage;
		private float _tilesPerDay;
		private string _tilesPerDayExplanation;
		private float _visibility;

		private bool tilesPerDayDirty = true;
		private bool massCapacityDirty = true;
		private bool massUsageDirty = true;
		private bool visibilityDirty = true;
		private bool foragedFoodPerDayDirty = true;
		private bool daysWorthOfFoodDirty = true;

		List<Tradeable> _tradeables;


		/// <summary>
		/// World tile occupied by the current trade negotiator of the player. This is lazily initialized before draw.
		/// </summary>
		private int _playerTile = Tile.Invalid;

		/// <summary>
		/// Biome in which the transaction is taking place. This is lazily initialized before draw.
		/// </summary>
		private BiomeDef? _playerBiome;

		public bool InCaravan => _inCaravan;


		public void SetDirty()
		{
			massUsageDirty = true;
			massCapacityDirty = true;
			tilesPerDayDirty = true;
			daysWorthOfFoodDirty = true;
			foragedFoodPerDayDirty = true;
			visibilityDirty = true;
		}


		public Pair<float, float> DaysWorthOfFood
		{
			get
			{
				if (daysWorthOfFoodDirty)
				{
					daysWorthOfFoodDirty = false;
					TradeSession.deal.UpdateCurrencyCount();
					float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, _playerTile, IgnorePawnsInventoryMode.Ignore, Faction.OfPlayer);
					_daysWorthOfFood = new Pair<float, float>(first, DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, _playerTile, IgnorePawnsInventoryMode.Ignore));
				}
				return _daysWorthOfFood;
			}
		}

		public float TilesPerDay
		{
			get
			{
				if (tilesPerDayDirty)
				{
					tilesPerDayDirty = false;
					TradeSession.deal.UpdateCurrencyCount();
					Caravan caravan = TradeSession.playerNegotiator.GetCaravan();
					StringBuilder stringBuilder = new StringBuilder();
					_tilesPerDay = TilesPerDayCalculator.ApproxTilesPerDayLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, MassUsage, MassCapacity, _playerTile, (caravan != null && caravan.pather.Moving) ? caravan.pather.nextTile : (-1), stringBuilder);
					_tilesPerDayExplanation = stringBuilder.ToString();
				}
				return _tilesPerDay;
			}
		}

		public Pair<ThingDef, float> ForagedFoodPerDay
		{
			get
			{
				if (foragedFoodPerDayDirty)
				{
					foragedFoodPerDayDirty = false;
					TradeSession.deal.UpdateCurrencyCount();
					StringBuilder stringBuilder = new StringBuilder();
					_foragedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDayLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, _playerBiome, Faction.OfPlayer, stringBuilder);
					_foragedFoodPerDayExplanation = stringBuilder.ToString();
				}
				return _foragedFoodPerDay;
			}
		}



		public float Visibility
		{
			get
			{
				if (visibilityDirty)
				{
					visibilityDirty = false;
					TradeSession.deal.UpdateCurrencyCount();
					StringBuilder stringBuilder = new StringBuilder();
					_visibility = CaravanVisibilityCalculator.VisibilityLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, stringBuilder);
					_visibilityExplanation = stringBuilder.ToString();
				}
				return _visibility;
			}
		}


		public float MassCapacity
		{
			get
			{
				if (massCapacityDirty)
				{
					massCapacityDirty = false;
					TradeSession.deal.UpdateCurrencyCount();
					StringBuilder stringBuilder = new StringBuilder();
					_massCapacity = CollectionsMassCalculator.CapacityLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, stringBuilder);
					_massCapacityExplanation = stringBuilder.ToString();
				}
				return _massCapacity;
			}
		}


		public float MassUsage
		{
			get
			{
				if (massUsageDirty)
				{
					massUsageDirty = false;
					TradeSession.deal.UpdateCurrencyCount();
					_massUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, IgnorePawnsInventoryMode.Ignore);
				}
				return _massUsage;
			}
		}

		public CaravanWidget(List<Tradeable> tradeables, Tradeable currency)
		{
			_tradeables = tradeables.ToList();
			_tradeables.Add(currency);

			_visibilityExplanation = string.Empty;
			_foragedFoodPerDayExplanation = string.Empty;

			_massCapacityExplanation = string.Empty;
			_tilesPerDayExplanation = string.Empty;
			_allPawnsAndItems = new List<Thing>();
		}

		public void Initialize()
		{
			_allPawnsAndItems.Clear();
			Caravan caravan = TradeSession.playerNegotiator.GetCaravan();
			if (caravan != null)
			{
				_inCaravan = true;
				List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
				for (int i = 0; i < pawnsListForReading.Count; i++)
				{
					_allPawnsAndItems.Add(pawnsListForReading[i]);
				}
				_allPawnsAndItems.AddRange(CaravanInventoryUtility.AllInventoryItems(caravan));

				caravan.Notify_StartedTrading();
			}
			else
			{
				_inCaravan = false;
			}
		}


		public void Draw(Rect inRect)
		{
			if (InitializeTradeLocation())
			{
				CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, _massCapacityExplanation, TilesPerDay, _tilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, _foragedFoodPerDayExplanation, Visibility, _visibilityExplanation), null, _playerTile, null, -9999f, inRect);
			}
			else
			{
				Logging.ErrorOnce("Could not find trade location. Caravan info widget cannot be drawn.");
			}
		}

		private bool InitializeTradeLocation()
		{
			if (_playerTile == Tile.Invalid)
			{
				var tile = TradeSession.playerNegotiator.Tile;
				if (tile != Tile.Invalid)
				{
					_playerTile = tile;
					_playerBiome = Find.WorldGrid[_playerTile].biome;
				}
			}

			return _playerBiome != null;
		}
	}
}
