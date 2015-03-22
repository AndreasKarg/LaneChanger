﻿using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace LaneChanger
{
    
    public class LaneChangerPathManager : PathManager
    {
        LaneChangerPathFind[] replacementPathFinds;
        
        //On waking up, replace the stock pathfinders with the custom one
        new void Awake()
        {
            PathFind[] stockPathFinds = GetComponents<PathFind>();
            replacementPathFinds = new LaneChangerPathFind[stockPathFinds.Length];
            for (int i = 0; i < stockPathFinds.Length; i++)
            {
                replacementPathFinds[i] = gameObject.AddComponent<LaneChangerPathFind>();
                Destroy(stockPathFinds[i]);
            }
            typeof(PathManager).GetField("m_pathfinds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, replacementPathFinds);
        }

        public void UpdateWithPathManagerValues(PathManager stockPathManager)
        {
            // Needed fields come from joaofarias' csl-traffic
            // https://github.com/joaofarias/csl-traffic
             
            this.m_simulationProfiler = stockPathManager.m_simulationProfiler;
            this.m_drawCallData = stockPathManager.m_drawCallData;
            this.m_properties = stockPathManager.m_properties;
            this.m_pathUnitCount = stockPathManager.m_pathUnitCount;
            this.m_renderPathGizmo = stockPathManager.m_renderPathGizmo;
            this.m_pathUnits = stockPathManager.m_pathUnits;
            this.m_bufferLock = stockPathManager.m_bufferLock;
        }

        //
        // BEGIN STOCK CODE
        //
        
        public new bool CreatePath(out uint unit, ref Randomizer randomizer, uint buildIndex, PathUnit.Position startPos, PathUnit.Position endPos, NetInfo.LaneType laneTypes, VehicleInfo.VehicleType vehicleTypes, float maxLength)
        {
            PathUnit.Position position = default(PathUnit.Position);
            return this.CreatePath(out unit, ref randomizer, buildIndex, startPos, position, endPos, position, position, laneTypes, vehicleTypes, maxLength, false, false, false, false);
        }

        public new bool CreatePath(out uint unit, ref Randomizer randomizer, uint buildIndex, PathUnit.Position startPosA, PathUnit.Position startPosB, PathUnit.Position endPosA, PathUnit.Position endPosB, NetInfo.LaneType laneTypes, VehicleInfo.VehicleType vehicleTypes, float maxLength)
        {
            return this.CreatePath(out unit, ref randomizer, buildIndex, startPosA, startPosB, endPosA, endPosB, default(PathUnit.Position), laneTypes, vehicleTypes, maxLength, false, false, false, false);
        }

        public new bool CreatePath(out uint unit, ref Randomizer randomizer, uint buildIndex, PathUnit.Position startPosA, PathUnit.Position startPosB, PathUnit.Position endPosA, PathUnit.Position endPosB, NetInfo.LaneType laneTypes, VehicleInfo.VehicleType vehicleTypes, float maxLength, bool isHeavyVehicle, bool ignoreBlocked, bool stablePath, bool skipQueue)
        {
            return this.CreatePath(out unit, ref randomizer, buildIndex, startPosA, startPosB, endPosA, endPosB, default(PathUnit.Position), laneTypes, vehicleTypes, maxLength, isHeavyVehicle, ignoreBlocked, stablePath, skipQueue);
        }

        public new bool CreatePath(out uint unit, ref Randomizer randomizer, uint buildIndex, PathUnit.Position startPosA, PathUnit.Position startPosB, PathUnit.Position endPosA, PathUnit.Position endPosB, PathUnit.Position vehiclePosition, NetInfo.LaneType laneTypes, VehicleInfo.VehicleType vehicleTypes, float maxLength, bool isHeavyVehicle, bool ignoreBlocked, bool stablePath, bool skipQueue)
        {
            while (!Monitor.TryEnter(this.m_bufferLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            uint num;
            try
            {
                if (!this.m_pathUnits.CreateItem(out num, ref randomizer))
                {
                    unit = 0u;
                    bool result = false;
                    return result;
                }
                this.m_pathUnitCount = (int)(this.m_pathUnits.ItemCount() - 1u);
            }
            finally
            {
                Monitor.Exit(this.m_bufferLock);
            }
            unit = num;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_simulationFlags = 1;
            if (isHeavyVehicle)
            {
                PathUnit[] expr_92_cp_0 = this.m_pathUnits.m_buffer;
                UIntPtr expr_92_cp_1 = (UIntPtr)unit;
                expr_92_cp_0[(int)expr_92_cp_1].m_simulationFlags = (byte)(expr_92_cp_0[(int)expr_92_cp_1].m_simulationFlags | 16);
            }
            if (ignoreBlocked)
            {
                PathUnit[] expr_BB_cp_0 = this.m_pathUnits.m_buffer;
                UIntPtr expr_BB_cp_1 = (UIntPtr)unit;
                expr_BB_cp_0[(int)expr_BB_cp_1].m_simulationFlags = (byte)(expr_BB_cp_0[(int)expr_BB_cp_1].m_simulationFlags | 32);
            }
            if (stablePath)
            {
                PathUnit[] expr_E4_cp_0 = this.m_pathUnits.m_buffer;
                UIntPtr expr_E4_cp_1 = (UIntPtr)unit;
                expr_E4_cp_0[(int)expr_E4_cp_1].m_simulationFlags = (byte)(expr_E4_cp_0[(int)expr_E4_cp_1].m_simulationFlags | 64);
            }
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_pathFindFlags = 0;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_buildIndex = buildIndex;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_position00 = startPosA;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_position01 = endPosA;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_position02 = startPosB;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_position03 = endPosB;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_position11 = vehiclePosition;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_nextPathUnit = 0u;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_laneTypes = (byte)laneTypes;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_vehicleTypes = (byte)vehicleTypes;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_length = maxLength;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_positionCount = 20;
            this.m_pathUnits.m_buffer[(int)((UIntPtr)unit)].m_referenceCount = 1;
            int num2 = 10000000;
            LaneChangerPathFind pathFind = null;
            for (int i = 0; i < this.replacementPathFinds.Length; i++)
            {
                LaneChangerPathFind pathFind2 = this.replacementPathFinds[i];
                if (pathFind2.IsAvailable && pathFind2.m_queuedPathFindCount < num2)
                {
                    num2 = pathFind2.m_queuedPathFindCount;
                    pathFind = pathFind2;
                }
            }
            if (pathFind != null && pathFind.CalculatePath(unit, skipQueue))
            {
                return true;
            }
            this.ReleasePath(unit);
            return false;
        }
    }
}
