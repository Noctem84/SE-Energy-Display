using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class EnergyProperty
        {
            private ulong value;
            private int position;
            private EnergyType energyType;

            public EnergyProperty(EnergyType energyType, IMyTerminalBlock block)
            {
                value = 0;
                position = -1;
                String[] lines = block.DetailedInfo.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith(energyType.getKey()))
                    {
                        position = i;
                        break;
                    }
                }
                this.energyType = energyType;
            }

            public String getTerm()
            {
                return energyType.getKey();
            }

            public int getIndex()
            {
                return energyType.getIndex();
            }

            public int getPosition()
            {
                return position;
            }

            public ulong getValue()
            {
                return value;
            }

            public void setValue(ulong value)
            {
                this.value = value;
            }
        }
    }
}
