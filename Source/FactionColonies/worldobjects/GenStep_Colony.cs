﻿using System;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace FactionColonies
{
    public class GenStep_Colony : GenStep_Scatterer
    {
        private SettlementFC Settlement { get; set; }
        
        public override int SeedPart => 1806208471;

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map) || !c.Standable(map) || c.Roofed(map) || !map.reachability.CanReachMapEdge(c,
                TraverseParms.For(TraverseMode.PassDoors)))
            {
                return false;
            }

            if (Settlement == null)
            {
                FactionFC settlementFaction = Find.World.GetComponent<FactionFC>();
                Settlement = settlementFaction.getSettlement(map.Tile, Find.World.info.name);
            }
            int min = 36 + Settlement.settlementLevel * 2 - 2;
            return new CellRect(c.x - min / 2, c.z - min / 2, min, min).FullyContainedWithin(new CellRect(0, 0,
                map.Size.x, map.Size.z));
        }

        protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            if (Settlement == null)
            {
                FactionFC settlementFaction = Find.World.GetComponent<FactionFC>();
                Settlement = settlementFaction.getSettlement(map.Tile, Find.World.info.name);
            }


            int middle = 36 + Settlement.settlementLevel * 2;
            IntRange range = new IntRange(middle - 2, middle + 2);
            int randomInRange1 = range.RandomInRange;
            int randomInRange2 = range.RandomInRange;
            CellRect cellRect = new CellRect(c.x - randomInRange1 / 2, c.z - randomInRange2 / 2, randomInRange1,
                randomInRange2);
            Faction faction = Faction.OfPlayer;
            cellRect.ClipInsideMap(map);
            ResolveParams resolveParams = new ResolveParams();
            resolveParams.rect = cellRect;
            resolveParams.faction = faction;
            BaseGen.globalSettings.map = map;
            resolveParams.stockpileMarketValue = (float) Settlement.totalIncome;
            double defenseBuildings = 0;
            foreach (BuildingFCDef building in Settlement.buildings)
            {
                double baseIncrease = 0;
                double multiplier = 1;
                foreach(FCTraitEffectDef traitEffect in building.traits)
                {
                    baseIncrease += traitEffect.productionBaseWeapons;
                    multiplier += traitEffect.productionMultiplierWeapons;
                }

                defenseBuildings += baseIncrease * multiplier;
            }

            int defenseCount = (int) Math.Round(defenseBuildings);
            resolveParams.edgeDefenseMortarsCount = defenseCount;
            resolveParams.edgeDefenseTurretsCount = defenseCount;
            Log.Message("Settlement level: " + Settlement.settlementLevel + ", defense count: " + defenseCount);
            BaseGen.globalSettings.minBuildings = Settlement.settlementLevel;
            BaseGen.globalSettings.minBarracks = Settlement.settlementLevel;
            BaseGen.symbolStack.Push("colony", resolveParams);
            BaseGen.Generate();
        }
    }
}