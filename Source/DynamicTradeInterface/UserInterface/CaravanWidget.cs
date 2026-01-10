using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface
{
	internal class CaravanWidget
	{
		private bool _inCaravan;
		private List<Thing> _allPawnsAndItems;

		private string _visibilityExplanation;
		private (float, float) _daysWorthOfFood;
		private (ThingDef, float) _foragedFoodPerDay;
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


		private int _playerTile;
		private BiomeDef _playerBiome;

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


		public (float, float) DaysWorthOfFood
		{
			get
			{
				if (daysWorthOfFoodDirty)
				{
					daysWorthOfFoodDirty = false;
					TradeSession.deal.UpdateCurrencyCount();
					float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, _playerTile, IgnorePawnsInventoryMode.Ignore, Faction.OfPlayer);
					_daysWorthOfFood = (first, DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, _playerTile, IgnorePawnsInventoryMode.Ignore));
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
					if (caravan.Shuttle != null)
					{
						_tilesPerDayExplanation = "CaravanMovementSpeedShuttle".Translate();
						return 0f;
					}

					StringBuilder stringBuilder = new StringBuilder();
					_tilesPerDay = TilesPerDayCalculator.ApproxTilesPerDayLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, MassUsage, MassCapacity, _playerTile, (caravan != null && caravan.pather.Moving) ? caravan.pather.nextTile : (-1), false, stringBuilder);
					_tilesPerDayExplanation = stringBuilder.ToString();
				}
				return _tilesPerDay;
			}
		}

		public (ThingDef, float) ForagedFoodPerDay
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
					Building_PassengerShuttle shuttle = TradeSession.playerNegotiator.GetCaravan().Shuttle;
					if (shuttle != null)
					{
						_massCapacity = shuttle.TransporterComp.MassCapacity;
						return _massCapacity;
					}
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


					Building_PassengerShuttle shuttle = TradeSession.playerNegotiator.GetCaravan().Shuttle;
					if (shuttle != null)
					{
						_massUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, IgnorePawnsInventoryMode.Ignore, includePawnsMass: true);
						_massUsage -= shuttle.GetStatValue(StatDefOf.Mass);
					}
					else
						_massUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(_allPawnsAndItems, _tradeables, IgnorePawnsInventoryMode.Ignore);
				}
				return _massUsage;
			}
		}



		public CaravanWidget(List<Tradeable> tradeables, Tradeable currency)
		{
			_tradeables = tradeables.ToList();

			if (currency != null)
				_tradeables.Add(currency);

			_visibilityExplanation = string.Empty;
			_foragedFoodPerDayExplanation = string.Empty;
			_massCapacityExplanation = string.Empty;
			_tilesPerDayExplanation = string.Empty;
			_allPawnsAndItems = new List<Thing>();
			_playerTile = TradeSession.playerNegotiator.Tile;
			_playerBiome = Find.WorldGrid[_playerTile].PrimaryBiome;
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
			CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, _massCapacityExplanation, TilesPerDay, _tilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, _foragedFoodPerDayExplanation, Visibility, _visibilityExplanation), null, _playerTile, null, -9999f, inRect);
		}
	}
}
