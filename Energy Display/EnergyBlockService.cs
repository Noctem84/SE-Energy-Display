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
        public class EnergyBlockService
        {
            private Program program;

            readonly List<EnergyBlock> reactors;
            readonly List<EnergyBlock> batteries;
            readonly List<EnergyBlock> solarPanels;
            readonly List<EnergyBlock> engines;
            readonly List<EnergyBlock> consumer;
            readonly List<EnergyBlock> powerBlocks;
            readonly Dictionary<string, BlockType> blockTypes;

            public int scannedBlocks;
            public ulong currentOutput;
            public ulong currentOutputReactor;
            public ulong currentOutputSolar;
            public ulong currentOutputBatteries;
            public ulong currentInput;
            public ulong currentInputBatteries;
            public ulong currentInputOther;
            public ulong storedEnergy;
            public ulong maxStoredEnergy;
            public ulong maxSolarOutput;
            public ulong maxReactorOutput;
            public ulong powerBalance;


            public ulong requiredInput;

            //readonly Dictionary<int, string> energyTypes;
            readonly EnergyType TYPE_OUTPUT = new EnergyType(0, "Current Output:");
            readonly EnergyType TYPE_INPUT = new EnergyType(1, "Current Input:");
            readonly EnergyType TYPE_STORED = new EnergyType(2, "Stored power:");
            readonly EnergyType TYPE_MAX_STORED = new EnergyType(3, "Max Stored Power:");
            readonly EnergyType TYPE_MAX_OUTPUT = new EnergyType(4, "Max Output:");
            readonly EnergyType TYPE_REQUIRED_INPUT = new EnergyType(5, "Required Input:");
            readonly EnergyType TYPE_MAX_REQUIRED_INPUT = new EnergyType(6, "Max Required Input:");
            readonly String BATTERY_TYPE_ID = "MyObjectBuilder_BatteryBlock";
            readonly String SOLAR_PANEL_TYPE_ID = "MyObjectBuilder_SolarPanel";
            readonly String REACTOR_TYPE_ID = "MyObjectBuilder_Reactor";
            readonly String ENGINE_TYPE_ID = "MyObjectBuilder_Engine";

            public EnergyBlockService(Program program)
            {
                program.Echo("init energy block service");
                this.program = program;
                reactors = new List<EnergyBlock>();
                batteries = new List<EnergyBlock>();
                solarPanels = new List<EnergyBlock>();
                engines = new List<EnergyBlock>();
                consumer = new List<EnergyBlock>();
                powerBlocks = new List<EnergyBlock>();
                blockTypes = new Dictionary<string, BlockType>();
                scannedBlocks = 0;

                initBlocks();
                program.Echo("init energy block service done");
            }

            public int initBlocks()
            {
                DateTime dateTime = DateTime.Now;
                //program.Echo("init blocks");
                List<IMyFunctionalBlock> allBlocks = new List<IMyFunctionalBlock>();
                program.GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(allBlocks);
                if (allBlocks.Count == scannedBlocks)
                {
                    return 0;
                }
                reactors.Clear();
                batteries.Clear();
                solarPanels.Clear();
                consumer.Clear();
                powerBlocks.Clear();


                foreach (IMyTerminalBlock block in allBlocks)
                {
                    BlockType blockType;
                    if (!blockTypes.ContainsKey(block.BlockDefinition.TypeIdString))
                    {
                        blockType = initBlockType(block);
                        blockTypes.Add(blockType.Typ, blockType);
                    }
                    else
                    {
                        try
                        {
                            blockTypes.TryGetValue(block.BlockDefinition.TypeIdString, out blockType);
                        } catch(ArgumentNullException ex)
                        {
                            program.Echo("Error while getting block Type: " + ex.Message);
                            continue;
                        }
                    }

                    if (!blockType.Consumer && !blockType.Producer)
                    {
                        continue;
                    }

                    //program.Echo("Block type: " + blockType.Typ);

                    EnergyBlock energyBlock;

                    energyBlock = new EnergyBlock(blockType, block);
                    powerBlocks.Add(energyBlock);
                    updateEnergyBlock(energyBlock);

                    if (BATTERY_TYPE_ID.Equals(blockType.Typ))
                    {
                        //program.Echo("Block type: " + blockType.Typ);
                        batteries.Add(energyBlock);
                    }
                    else if (blockType.Consumer)
                    {
                        //program.Echo("Block type: " + blockType.Typ);
                        consumer.Add(energyBlock);
                    }
                    else if (REACTOR_TYPE_ID.Equals(blockType.Typ))
                    {
                        //program.Echo("Block type: " + blockType.Typ);
                        reactors.Add(energyBlock);
                    }
                    else if (SOLAR_PANEL_TYPE_ID.Equals(blockType.Typ))
                    {
                        //program.Echo("Block type: " + blockType.Typ);
                        reactors.Add(energyBlock);
                    }
                    else if (ENGINE_TYPE_ID.Equals(blockType.Typ))
                    {
                        engines.Add(energyBlock);
                    }
                }
                int newBlocks = allBlocks.Count - scannedBlocks;
                scannedBlocks = allBlocks.Count;
                //program.Echo(DateTime.Now.Subtract(dateTime).Milliseconds + "");
                return newBlocks;
            }

            public BlockType initBlockType(IMyTerminalBlock block)
            {
                BlockType blockType = new BlockType();
                blockType.Typ = block.BlockDefinition.TypeIdString;
                String[] lines = block.DetailedInfo.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    String line = lines[i];
                    if (line.StartsWith(TYPE_OUTPUT.getKey()))
                    {
                        EnergyProperty energyProperty = new EnergyProperty(TYPE_OUTPUT, i);
                        blockType.Producer = true;
                        blockType.addEnergyProperty(energyProperty);
                    }
                    else if (line.StartsWith(TYPE_INPUT.getKey()))
                    {
                        EnergyProperty energyProperty = new EnergyProperty(TYPE_INPUT, i);
                        blockType.Consumer = true;
                        blockType.addEnergyProperty(energyProperty);
                    }
                    else if (line.StartsWith(TYPE_STORED.getKey()))
                    {
                        EnergyProperty energyProperty = new EnergyProperty(TYPE_STORED, i);
                        blockType.addEnergyProperty(energyProperty);
                    }
                    else if (line.StartsWith(TYPE_MAX_STORED.getKey()))
                    {
                        EnergyProperty energyProperty = new EnergyProperty(TYPE_MAX_STORED, i);
                        blockType.addEnergyProperty(energyProperty);
                    }
                    else if (line.StartsWith(TYPE_MAX_OUTPUT.getKey()))
                    {
                        EnergyProperty energyProperty = new EnergyProperty(TYPE_MAX_OUTPUT, i);
                        blockType.addEnergyProperty(energyProperty);
                    }
                    else if (line.StartsWith(TYPE_REQUIRED_INPUT.getKey()))
                    {
                        EnergyProperty energyProperty = new EnergyProperty(TYPE_REQUIRED_INPUT, i);
                        blockType.addEnergyProperty(energyProperty);
                    }
                }
                return blockType;
            }

            public int getReactorCount()
            {
                return reactors.Count;
            }

            public int getBatteryCount()
            {
                return batteries.Count;
            }

            public int getSolarPanelCount()
            {
                return solarPanels.Count;
            }

            public void updateValues()
            {
                //   program.Echo("Updating values");

                reset();
                //program.Echo("updating " + powerBlocks.Count + "/" + scannedBlocks + " blocks");
                foreach (EnergyBlock block in reactors)
                {
                    updateEnergyBlock(block);
                    currentOutputReactor += block.CurrentOutput;
                    maxReactorOutput += block.MaxOutput;
                }
                foreach (EnergyBlock block in batteries)
                {
                    updateEnergyBlock(block);
                    currentOutputBatteries += block.CurrentOutput;
                    currentInputBatteries += block.CurrentInput;
                    storedEnergy += block.StoredPower;
                    maxStoredEnergy += block.MaxStoredPower;
                }
                foreach (EnergyBlock block in solarPanels)
                {
                    updateEnergyBlock(block);
                    currentOutputSolar += block.CurrentOutput;
                    maxSolarOutput += block.MaxOutput;
                }
                foreach (EnergyBlock block in consumer)
                {
                    updateEnergyBlock(block);
                    currentInputOther += block.CurrentInput;
                    requiredInput += block.RequiredInput;
                }
                currentOutput = currentOutputReactor + currentOutputBatteries + currentOutputSolar;
                currentInput = currentInputOther + currentInputBatteries;
                powerBalance = currentOutput - currentInput;
            }
            
            /**
             * Converting power string to ulong and normalize to kW
             */
            private ulong parsePower(String powerString)
            {
                //program.Echo(powerString + "");
                float power = 0;
                String[] wattAndUnit = powerString.Trim().Split(null);
                power = float.Parse(wattAndUnit[0]);
                switch (wattAndUnit[1])
                {
                    case "W":
                        power = power / 1000;
                        break;
                    case "Wh":
                        power = power / 1000;
                        break;
                    case "kW":
                        break;
                    case "kWh":
                        break;
                    case "MW":
                        power = power * 1000;
                        break;
                    case "MWh":
                        power = power * 1000;
                        break;
                    default:
                        break;
                }
                return Convert.ToUInt64(power);
            }

            private void updateEnergyBlock(EnergyBlock energyBlock)
            {
                String[] lines = energyBlock.getBlock().DetailedInfo.Split('\n');
                foreach(EnergyProperty energyProperty in energyBlock.getblockType().EnergyProperties)
                {
                    ulong value = parsePower(lines[energyProperty.Position].Substring(energyProperty.EnergyType.getKey().Length + 1));
                    //program.Echo("value: " + value);
                    switch(energyProperty.EnergyType.getIndex())
                    {
                        case 0:
                            energyBlock.CurrentOutput = value;
                            break;
                        case 1:
                            energyBlock.CurrentInput = value;
                            break;
                        case 2:
                            energyBlock.StoredPower = value;
                            break;
                        case 3:
                            energyBlock.MaxStoredPower = value;
                            break;
                        case 4:
                            energyBlock.MaxOutput = value;
                            break;
                        case 5:
                            energyBlock.RequiredInput = value;
                            break;
                        default: break;
                    }
                    //program.Echo(energyBlock.ToString());
                }
            }

            private void reset()
            {
                currentOutputReactor = 0;
                maxReactorOutput = 0;

                currentOutputSolar = 0;
                maxSolarOutput = 0;

                currentOutputBatteries = 0;
                currentInputBatteries = 0;
                storedEnergy = 0;
                maxStoredEnergy = 0;

                currentInputOther = 0;
                requiredInput = 0;
            }
        }
    }
}
