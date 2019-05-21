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
    partial class Program : MyGridProgram
    {
        readonly String SEPERATOR = "------------------------------";

        EnergyBlockService energyBlockService;
        IMyTextSurface display;
        int updateCount;
        int updateLimit;

        public Program()
        {
            display = GridTerminalSystem.GetBlockWithName("Bug LCD Panel Maschinenraum 1") as IMyTextSurface;
            Echo("init program");
            updateCount = 1;
            updateLimit = 30;
            energyBlockService = new EnergyBlockService(this);
            // Runns Script every 100 Ticks = ~1 sec.
            Runtime.UpdateFrequency |= UpdateFrequency.Update100;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            string DISPLAY_COMMAND = "display=";
            if (argument.Contains(DISPLAY_COMMAND))
            {
                int position = argument.IndexOf(DISPLAY_COMMAND);
                string displayName = argument.Substring(position + DISPLAY_COMMAND.Length);
                Echo("set display name to " + displayName);
                display = GridTerminalSystem.GetBlockWithName(displayName) as IMyTextSurface;
                Echo("display init " + (display == null ? "failed" : "succeded"));
            }
            if (updateCount == updateLimit)
            {
                updateCount = 0;
                energyBlockService.initBlocks();
                energyBlockService.updateValues();
                updateDisplay();
            }
            else if (updateCount % 2 == 0)
            {
                updateCount++;
                energyBlockService.updateValues();
                updateDisplay();
            }
            else
            {
                updateCount++;
            }
        }

        private void updateDisplay()
        {
            // Echo("writing to display");
            writeToDisplay("Current Output", false);
            writeToDisplay("+ " + energyBlockService.getReactorCount() + " Reactors: " + energyBlockService.currentOutputReactor + " / " + energyBlockService.maxReactorOutput + " kW", true);
            writeToDisplay("+ " + energyBlockService.getSolarPanelCount() + " Solar Panels: " + energyBlockService.currentOutputSolar + " kW / max: " + energyBlockService.maxSolarOutput + " kW", true);
            writeToDisplay("+ " + energyBlockService.getBatteryCount() + " Batteries: " + energyBlockService.currentOutputBatteries + " kW", true);
            writeToDisplay("+ Sum: " + energyBlockService.currentOutput + " kW", true);
            writeToDisplay(SEPERATOR, true);
            writeToDisplay("Current Input", true);
            writeToDisplay("+ Batteries: " + energyBlockService.currentInputBatteries + " kW", true);
            writeToDisplay("+ Other: " + energyBlockService.currentInputOther + " kW", true);
            writeToDisplay(SEPERATOR, true);
            writeToDisplay("Battery Status", true);
            writeToDisplay("+ Stored Power: " + energyBlockService.storedEnergy + " / " + energyBlockService.maxStoredEnergy + " kWh", true);
            //          writeToDisplay("Charging Batteries: " + energyBlockService.chargingBatteries.Count + " / " + dischargingBatteries.Count, true);
            writeToDisplay(SEPERATOR, true);
            writeToDisplay("Power Balance: " + energyBlockService.powerBalance, true);
        }

        public void writeToDisplay(String text, bool append)
        {
            StringBuilder formatedTextBuilder = new StringBuilder();
            foreach (var line in text.Split('\n'))
            {
                formatedTextBuilder.Append("\n   ");
                formatedTextBuilder.Append(line);
            }
            if (display != null)
            {
                display.WriteText(formatedTextBuilder, append);
            }
        }
    }
}