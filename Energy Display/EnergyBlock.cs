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
        public class EnergyBlock
        {
            readonly private Dictionary<int, EnergyProperty> energyProperties;
            readonly private IMyTerminalBlock block;

            public EnergyBlock(List<EnergyType> energyTypes, IMyTerminalBlock block)
            {
                energyProperties = new Dictionary<int, EnergyProperty>();
                foreach (EnergyType energyType in energyTypes)
                {
                    energyProperties.Add(energyType.getIndex(), new EnergyProperty(energyType, block));
                }
                this.block = block;
            }

            public EnergyBlock(IMyTerminalBlock block)
            {
                this.block = block;
                energyProperties = new Dictionary<int, EnergyProperty>();
            }

            public void addEnergyProperty(int key, EnergyProperty energyProperty)
            {
                energyProperties.Add(key, energyProperty);
            }

            public ulong getValue(int key)
            {
                EnergyProperty property;
                energyProperties.TryGetValue(key, out property);
                return property.getValue();
            }

            public void setValue(int key, ulong value)
            {
                EnergyProperty property;
                energyProperties.TryGetValue(key, out property);
                property.setValue(value);
            }

            public List<EnergyProperty> getEnergyPropertis()
            {
                return energyProperties.Values.ToList();
            }

            public IMyTerminalBlock getBlock()
            {
                return block;
            }
        }
    }
}
