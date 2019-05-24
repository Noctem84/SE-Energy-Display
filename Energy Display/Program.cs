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
        List<IMyTextSurface> displays;
        int updateCount;
        int updateLimit;
        String dots;
        String displayName;
        double totalRuntime;
        ulong totalRuns;
        double initTime;
        readonly string DISPLAY_COMMAND = "display=";
        readonly string REMOVE_DISPLAY_COMMAND = "remove-display=";

        public Program()
        {
            totalRuntime = 0;
            totalRuns = 0;
            initTime = 0;
            displayName = "Programmable Block";
            addDisplay("display=" + displayName);
            Echo("init program");
            updateCount = 1;
            updateLimit = 3;
            displays = new List<IMyTextSurface>();
            dots = ".";
            Echo("init energy block Service");
            energyBlockService = new EnergyBlockService(this);
            Echo("energy service init done");
            // Runns Script every 100 Ticks = ~1 sec.
            Echo("init update rate");
            Runtime.UpdateFrequency |= UpdateFrequency.Update10;
            Echo("init done");
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo("Running " + dots);
            Echo("Display: " + displayName);
            DateTime dateTime = DateTime.Now;
            int newBlocks = 0;
            if (argument.Contains(DISPLAY_COMMAND))
            {
                addDisplay(argument);
            }
            if (argument.Contains(REMOVE_DISPLAY_COMMAND))
            {
                removeDisplay(argument);
            }
            if (updateCount == updateLimit - 1)
            {
                dots = "...";
                //updateCount = 0;
                //energyBlockService.updateValues();
                newBlocks = energyBlockService.initBlocks();
                //updateDisplay();
            }
            else if (updateCount % 2 == 0)
            {
                dots = ".";
                //energyBlockService.updateValues();
                //newBlocks = energyBlockService.initBlocks();
                //updateDisplay();
                //updateCount++;
            }
            else
            {
                dots = "..";
                //energyBlockService.updateValues();
                //newBlocks = energyBlockService.initBlocks();
                //updateDisplay();
                //updateCount++;
            }
            updateCount = 0;
            energyBlockService.updateValues();
            //newBlocks = energyBlockService.initBlocks();
            updateDisplay();
            if (totalRuns > 1)
            {
                Echo("Runtime: " + String.Format("{0:0.000}", Runtime.LastRunTimeMs) + "ms (" + updateCount + ")");
                totalRuntime += Runtime.LastRunTimeMs;
                totalRuns++;
                Echo("Startup Time: " + String.Format("{0:0.000}", initTime) + " ms");
                Echo("Total Runtime: " + String.Format("{0:0.000}", totalRuntime) + " ms; Runs: " + totalRuns);
                Echo("Avarage Runtime: " + String.Format("{0:0.000}", totalRuntime / totalRuns) + " ms");
                Echo(newBlocks != 0 ? newBlocks + " new Blocks" : "no new blocks");
            } else
            {
                initTime = Runtime.LastRunTimeMs;
                totalRuns++;
            }
        }

        private void updateDisplay()
        {
            Echo("writing to display");
            writeToDisplay("Current Output" + dots, false);
            //Echo("Reactors: " + energyBlockService.currentOutputReactor);
            //Echo("Solar Panels: " + energyBlockService.currentOutputSolar);
            //Echo("Batteries: " + energyBlockService.currentOutputBatteries);
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
                formatedTextBuilder.Append(line);
                formatedTextBuilder.Append("\n");
            }
            if (displays != null && displays.Count > 0)
            {
                foreach(IMyTextSurface display in displays)
                {
                    display.WriteText(formatedTextBuilder, append);
                }
            }
        }

        private void removeDisplay(string argument)
        {
            IMyTextSurface display = getDisplay(argument, REMOVE_DISPLAY_COMMAND);
            if (display != null)
            {
                displays.Remove(display);
            }

        }

        private void addDisplay(string argument)
        {
            IMyTextSurface display = getDisplay(argument, DISPLAY_COMMAND);
            if (display != null)
            {
                if (displays == null)
                {
                    Echo("initializing display list");
                    displays = new List<IMyTextSurface>();
                }
                Echo("adding display");
                displays.Add(display);
                Echo("display added");
            }
        }

        private IMyTextSurface getDisplay(string argument, string command)
        {
            int position = argument.IndexOf(command);
            string displayName = argument.Substring(position + command.Length);
            this.displayName = displayName;
            int displayIndex = 0;
            if (displayName.Contains(','))
            {
                displayIndex = int.Parse(displayName.Split(',')[1]);
                if (displayIndex < 0)
                {
                    displayIndex = 0;
                }
                displayName = displayName.Split(',')[0];
            }

            Echo("set display name to " + displayName);
            IMyTextSurface display = GridTerminalSystem.GetBlockWithName(displayName) as IMyTextSurface;
            if (display == null)
            {
                Echo("No surface found");
                IMyTextSurfaceProvider provider = GridTerminalSystem.GetBlockWithName(displayName) as IMyTextSurfaceProvider;
                if (provider != null)
                {
                    Echo("Surface provider found");
                    Echo("Surface provider has " + provider.SurfaceCount + " displays");
                    if (provider.SurfaceCount > 0)
                    {
                        int surfaceIndex = provider.SurfaceCount > displayIndex ? displayIndex : 0;
                        Echo("selecting display " + surfaceIndex);
                        display = provider.GetSurface(surfaceIndex);
                    }
                }
                else
                {
                    Echo("No surface provider found");
                }
            }
            Echo("display init " + (display == null ? "failed" : "succeded"));
            if (display != null)
            {
                display.FontSize = 240 / display.SurfaceSize.Y;
                //display.TextPadding = 2;
            }
            return display;
        }
    }
}