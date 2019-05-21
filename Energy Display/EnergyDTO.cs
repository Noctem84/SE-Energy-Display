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
        public class EnergyDTO
        {
            ulong output;
            ulong maxOutput;

            ulong stored;
            ulong maxStored;

            ulong input;
            ulong inputRequired;

            public EnergyDTO()
            {
            }

            public void setOutput(ulong output)
            {
                this.output = output;
            }

            public ulong getOutput()
            {
                return output;
            }

            public void setMaxOutput(ulong maxOutput)
            {
                this.maxOutput = maxOutput;
            }

            public ulong getMaxOutput()
            {
                return maxOutput;
            }

            public void setStored(ulong stored)
            {
                this.stored = stored;
            }

            public ulong getStored()
            {
                return stored;
            }

            public void setMaxStored(ulong maxStored)
            {
                this.maxStored = maxStored;
            }

            public ulong getMaxStored()
            {
                return maxStored;
            }

            public void setInput(ulong input)
            {
                this.input = input;
            }

            public ulong getInput()
            {
                return input;
            }

            public void setInputRequired(ulong inputRequired)
            {
                this.inputRequired = inputRequired;
            }

            public ulong getInputRequired()
            {
                return inputRequired;
            }

            public void reset()
            {
                output = 0;
                maxOutput = 0;
                stored = 0;
                maxStored = 0;
                input = 0;
                inputRequired = 0;
            }
        }
    }
}
