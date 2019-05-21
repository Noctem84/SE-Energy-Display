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
            readonly List<EnergyBlock> consumer;

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

            readonly EnergyType TYPE_OUTPUT = new EnergyType(0, "Current Output:");
            readonly EnergyType TYPE_INPUT = new EnergyType(1, "Current Input:");
            readonly EnergyType TYPE_STORED = new EnergyType(2, "Stored power:");
            readonly EnergyType TYPE_MAX_STORED = new EnergyType(3, "Max Stored Power:");
            readonly EnergyType TYPE_MAX_OUTPUT = new EnergyType(4, "Max Output:");
            readonly EnergyType TYPE_REQUIRED_INPUT = new EnergyType(5, "Required Input:");
            readonly String NAME_BATTERY = "Battery";

            public EnergyBlockService(Program program)
            {
                this.program = program;
                reactors = new List<EnergyBlock>();
                batteries = new List<EnergyBlock>();
                solarPanels = new List<EnergyBlock>();
                consumer = new List<EnergyBlock>();
                initBlocks();
            }

            public void initBlocks()
            {
                program.Echo("init blocks");

                List<IMyTerminalBlock> terminalReactors = new List<IMyTerminalBlock>();
                List<IMyTerminalBlock> terminalBatteries = new List<IMyTerminalBlock>();
                List<IMyTerminalBlock> terminalSolarPanels = new List<IMyTerminalBlock>();
                List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
                program.GridTerminalSystem.GetBlocksOfType<IMyReactor>(terminalReactors);
                program.GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(terminalSolarPanels);
                program.GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(terminalBatteries);
                program.GridTerminalSystem.GetBlocks(allBlocks);

                reactors.Clear();
                foreach (IMyTerminalBlock reactor in terminalReactors)
                {
                    reactors.Add(new EnergyBlock(new List<EnergyType>() { TYPE_OUTPUT, TYPE_MAX_OUTPUT }, reactor));
                }
                batteries.Clear();
                foreach (IMyTerminalBlock battery in terminalBatteries)
                {
                    batteries.Add(new EnergyBlock(new List<EnergyType>() { TYPE_OUTPUT, TYPE_STORED, TYPE_INPUT, TYPE_MAX_STORED }, battery));
                }
                solarPanels.Clear();
                foreach (IMyTerminalBlock solarPanel in terminalSolarPanels)
                {
                    solarPanels.Add(new EnergyBlock(new List<EnergyType>() { TYPE_OUTPUT, TYPE_MAX_OUTPUT }, solarPanel));
                }
                //program.Echo(program.SEPERATOR);
                consumer.Clear();
                foreach (IMyTerminalBlock block in allBlocks)
                {
                    if (!block.CustomName.Contains(NAME_BATTERY))
                    {
                        if (!String.IsNullOrEmpty(block.DetailedInfo))
                        {
                            //program.Echo(block.DetailedInfo);
                        }
                        bool isCunsumer = false;
                        EnergyBlock energyBlock = new EnergyBlock(block);
                        EnergyProperty inputProperty = new EnergyProperty(TYPE_INPUT, block);
                        EnergyProperty requeiredInputProperty = new EnergyProperty(TYPE_REQUIRED_INPUT, block);
                        if (inputProperty.getPosition() > -1)
                        {
                            isCunsumer = true;
                            energyBlock.addEnergyProperty(TYPE_INPUT.getIndex(), inputProperty);
                        }
                        if (requeiredInputProperty.getPosition() > -1)
                        {
                            isCunsumer = true;
                            energyBlock.addEnergyProperty(TYPE_REQUIRED_INPUT.getIndex(), requeiredInputProperty);
                        }
                        if (isCunsumer)
                        {
                            consumer.Add(energyBlock);
                        }
                        //program.Echo(program.SEPERATOR);
                    }
                }
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

                EnergyDTO energyDTO = new EnergyDTO();
                foreach (EnergyBlock block in reactors)
                {
                    setPowers(block, energyDTO);
                    currentOutputReactor += energyDTO.getOutput();
                    maxReactorOutput += energyDTO.getMaxOutput();
                }
                foreach (EnergyBlock block in batteries)
                {
                    setPowers(block, energyDTO);
                    currentOutputBatteries += energyDTO.getOutput();
                    currentInputBatteries += energyDTO.getInput();
                    storedEnergy += energyDTO.getStored();
                    maxStoredEnergy += energyDTO.getMaxStored();
                }
                foreach (EnergyBlock block in solarPanels)
                {
                    setPowers(block, energyDTO);
                    currentOutputSolar += energyDTO.getOutput();
                    maxSolarOutput += energyDTO.getMaxOutput();
                }
                foreach (EnergyBlock block in consumer)
                {
                    setPowers(block, energyDTO);
                    currentInputOther += energyDTO.getInput();
                    currentInputOther += energyDTO.getInputRequired();
                }
                currentOutput = currentOutputReactor + currentOutputBatteries + currentOutputSolar;
                currentInput = currentInputOther + currentInputBatteries;
                powerBalance = currentOutput - currentInput;
            }

            private void setPowers(EnergyBlock energyBlock, EnergyDTO energyDTO)
            {
                energyDTO.reset();
                String[] lines = energyBlock.getBlock().DetailedInfo.Split('\n');
                foreach (EnergyProperty energyProperty in energyBlock.getEnergyPropertis())
                {
                    ulong value = parsePower(lines[energyProperty.getPosition()].Substring(energyProperty.getTerm().Length + 1));
                    energyProperty.setValue(value);
                    switch (energyProperty.getIndex())
                    {
                        // output
                        case 0:
                            energyDTO.setOutput(value);
                            break;
                        // input
                        case 1:
                            energyDTO.setInput(value);
                            break;
                        // stored power
                        case 2:
                            energyDTO.setStored(value);
                            break;
                        // max stored power
                        case 3:
                            energyDTO.setMaxStored(value);
                            break;
                        // max output
                        case 4:
                            energyDTO.setMaxOutput(value);
                            break;
                        // required input
                        case 5:
                            energyDTO.setInputRequired(value);
                            //    input += value;
                            break;
                        // unknown type
                        default:
                            break;
                    }
                }
            }

            /**
             * Converting power string to ulong and normalize to kW
             */
            private ulong parsePower(String powerString)
            {
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
            }
        }
    }
}
