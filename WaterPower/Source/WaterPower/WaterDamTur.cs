using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
namespace WaterDamTur
{
	public class Building_WaterDamTur: Building
	{
		private int updateEveryXTicks = 500;
		private int ticksSinceUpdate;
		private bool disablePowerRandomness = false;
		private int modifier;
		protected CompPowerTrader powerComp;
		Map map;


		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.map = map;

			powerComp = base.GetComp<CompPowerTrader>();
			powerComp.PowerOn = true;
			powerComp.PowerOutput = 0;

		}
		public override void Tick()
		{

			// Call work function
			DoTickerWork(1);

			base.TickRare();
		}

		private bool bCheck(IntVec3 cell)
		{
			Building edifice = cell.GetFirstBuilding(base.Map);
			if (!edifice.ToString().Contains("WaterDamTur") && !edifice.ToString().Contains("WaterDamGen") && !edifice.ToString().Contains("WaterDamBase"))
				return false;
			return true;		
		}
		private int bCount(List<String> ls, String phrase)
		{
			int count = 0;
			foreach (String str in ls)
			{
				if (str.Contains(phrase))
					count++;
			}
			return count;
		}
		private List<String> checkConnected(IntVec3 cell,Rot4 rot){

			List<String> connected = new List<string>();
			if (rot.ToString() == "0")
			{
				IntVec3 c;
				c = new IntVec3(cell.x,0,cell.y-1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x, 0, cell.y + 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x+1, 0, cell.y - 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x+1, 0, cell.y + 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
			}

			if (rot.ToString() == "1")
			{
				IntVec3 c;
				c = new IntVec3(cell.x-1, 0, cell.y - 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x+1, 0, cell.y-1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x - 1, 0, cell.y);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x + 1, 0, cell.y);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());

			}
			if (rot.ToString() == "2")
			{
				IntVec3 c;
				c = new IntVec3(cell.x, 0, cell.y - 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x, 0, cell.y + 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x - 1, 0, cell.y - 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x - 1, 0, cell.y + 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());

			}
					
			if (rot.ToString() == "3")
			{
				IntVec3 c;
				c = new IntVec3(cell.x - 1, 0, cell.y + 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x + 1, 0, cell.y + 1);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x - 1, 0, cell.y);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());
				c = new IntVec3(cell.x + 1, 0, cell.y);
				if (bCheck(c))
					connected.Add(c.GetFirstBuilding(base.Map).ToString());

			}

			return connected;
		
		}
		private void DoTickerWork(int ticks)
		{

			ticksSinceUpdate += ticks;

			if (ticksSinceUpdate >= updateEveryXTicks)
			{
				IntVec3 old_position = this.Position;
				TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(old_position);
				Rot4 rot = this.Rotation;
				if (terrainDef == TerrainDef.Named("WaterMovingShallow"))
				{
					modifier = 1250;
				}
				else {
					if (terrainDef == TerrainDef.Named("WaterMovingDeep"))
					{
						modifier = 1750;
					}
					else {
						modifier = 0;
					}
				}
				if (!disablePowerRandomness)
					modifier += Rand.RangeInclusive(-50, 50);
				if (terrainDef != TerrainDef.Named("WaterMovingShallow") && terrainDef != TerrainDef.Named("WaterMovingDeep"))
				{
					modifier = modifier / 5;
				}
				Log.Error(this.Rotation + " " + this.Position);
				List<String> connected = checkConnected(old_position, rot);
				if (connected.Count == 4)
				{
					int turs = bCount(connected, "WaterDamTur");
					int bas = bCount(connected, "WaterDamBase");
					int gen = bCount(connected, "WaterDamGen");
					switch (turs)
					{
						case 0:
							if (bas == 2 && gen == 2)
							{
								//Error here don't know how to get the poweroutput value from other turbines or base
								modifier = Building_WaterDamTur
							}
						case 2:
						case 4:
						default:
							modifier = 0;

					}

					if (turs == 4)
					{
					}

					if (turs == 2)
					{

					}
					if (turs == 0)
					{
						if (bas == 2 &&
					}
				}
				else {
					modifier = 0;
				}
			}


		}
		//defined poweroutput function so other turbines/generator can grab the value
		public int poweroutput{
			get{
				return modifier;
			}
		}
	}

}
