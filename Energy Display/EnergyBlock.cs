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
            readonly private BlockType blockType;
            readonly private IMyTerminalBlock block;
            public ulong CurrentInput { get; set; }
            public ulong RequiredInput { get; set; }
            public ulong StoredPower { get; set; }
            public ulong MaxStoredPower { get; set; }
            public ulong CurrentOutput { get; set; }
            public ulong MaxOutput { get; set; }

            public EnergyBlock(BlockType blockType, IMyTerminalBlock block)
            {
                this.blockType = blockType;
                this.block = block;
                CurrentInput = 0;
                RequiredInput = 0;
                StoredPower = 0;
                MaxStoredPower = 0;
                CurrentOutput = 0;
                MaxOutput = 0;
            }

            public BlockType getblockType()
            {
                return blockType;
            }

            public IMyTerminalBlock getBlock()
            {
                return block;
            }

            public override string ToString()
            {
                StringBuilder objectString = new StringBuilder();
                objectString.Append(blockType.Typ);
                objectString.Append("=CI:").Append(CurrentInput);
                objectString.Append("|CI:").Append(RequiredInput);
                objectString.Append("|CI:").Append(StoredPower);
                objectString.Append("|CI:").Append(MaxStoredPower);
                objectString.Append("|CI:").Append(CurrentOutput);
                objectString.Append("|CI:").Append(MaxOutput);
                return objectString.ToString();
            }
        }
    }
}
