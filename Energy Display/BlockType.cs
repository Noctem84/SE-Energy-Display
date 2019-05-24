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
        public class BlockType
        {
            public string Typ { get; set; }
            public List<EnergyProperty> EnergyProperties{ get; set;}
            public Boolean Consumer { get; set; }
            public Boolean Producer { get; set; }

            public BlockType()
            {
                this.EnergyProperties = new List<EnergyProperty>();
            }

            public void addEnergyProperty(EnergyProperty property) 
            {
                if (property.EnergyType.getIndex() == 1 || property.EnergyType.getIndex() == 5)
                {
                    Consumer = true;
                }
                else
                {
                    Producer = true;
                }
                EnergyProperties.Add(property);
            }
            
        }
    }
}
