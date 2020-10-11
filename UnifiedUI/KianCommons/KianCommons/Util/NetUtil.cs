using ColossalFramework;
using ColossalFramework.Math;
using KianCommons.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace KianCommons {
    public class NetServiceException : Exception {
        public NetServiceException(string m) : base(m) { }
        public NetServiceException() : base() { }
        public NetServiceException(string m, Exception e) : base(m, e) { }
    }

    public static class NetUtil {
        public const float SAFETY_NET = 0.02f;

        public static NetManager netMan = NetManager.instance;
        public static NetTool netTool => Singleton<NetTool>.instance;
        public static SimulationManager simMan => Singleton<SimulationManager>.instance;
        public static TerrainManager terrainMan => TerrainManager.instance;

        public const float MPU = 8f; // meter per unit

        private static NetNode[] nodeBuffer_ = netMan.m_nodes.m_buffer;
        private static NetSegment[] segmentBuffer_ = netMan.m_segments.m_buffer;
        private static NetLane[] laneBuffer_ = netMan.m_lanes.m_buffer;
        internal static ref NetNode ToNode(this ushort id) => ref nodeBuffer_[id];
        internal static ref NetSegment ToSegment(this ushort id) => ref segmentBuffer_[id];
        internal static ref NetLane ToLane(this uint id) => ref laneBuffer_[id];
        internal static NetLane.Flags Flags(this ref NetLane lane) => (NetLane.Flags)lane.m_flags;

        /// <summary>
        /// returns lane data of the given lane ID.
        /// throws exception if unsucessful.
        /// </summary>
        internal static LaneData GetLaneData(uint laneId) {
            Assertion.Assert(laneId != 0, "laneId!=0");
            var flags = laneId.ToLane().Flags();
            bool valid = (flags & NetLane.Flags.Created | NetLane.Flags.Deleted) != NetLane.Flags.Created;
            Assertion.Assert(valid, "valid");
            foreach (var laneData in IterateSegmentLanes(laneId.ToLane().m_segment))
                if (laneData.LaneID == laneId)
                    return laneData;
            throw new Exception("Unreachable code");
        }

        public static bool IsCSUR(NetInfo info) {
            if (info == null ||
                (info.m_netAI.GetType() != typeof(RoadAI) &&
                info.m_netAI.GetType() != typeof(RoadBridgeAI) &&
                info.m_netAI.GetType() != typeof(RoadTunnelAI))) {
                return false;
            }
            return info.name.Contains(".CSUR ");
        }


        public static ToolBase.ToolErrors InsertNode(NetTool.ControlPoint controlPoint, out ushort nodeId, bool test = false) {
            var ret = NetTool.CreateNode(
                controlPoint.m_segment.ToSegment().Info,
                controlPoint, controlPoint, controlPoint,
                NetTool.m_nodePositionsSimulation,
                maxSegments: 0,
                test: test, visualize: false, autoFix: true, needMoney: false,
                invert: false, switchDir: false,
                relocateBuildingID: 0,
                out nodeId, out var newSegment, out var cost, out var productionRate);
            if (!test) {
                nodeId.ToNode().m_flags |= NetNode.Flags.Middle | NetNode.Flags.Moveable;
            }
            //Log.Debug($"[InsertNode] test={test} errors:{ret} nodeId:{nodeId} newSegment:{newSegment} cost:{cost} productionRate{productionRate}");
            return ret;
        }

        internal static int CountPedestrianLanes(this NetInfo info) =>
            info.m_lanes.Count(lane => lane.m_laneType == NetInfo.LaneType.Pedestrian);

        static bool Equals(this ref NetNode node1, ushort nodeId2) {
            ref NetNode node2 = ref nodeId2.ToNode();
            return node1.m_buildIndex == node2.m_buildIndex &&
                   node1.m_position == node2.m_position;
        }

        static bool Equals(this ref NetSegment segment1, ushort segmentId2) {
            ref NetSegment segment2 = ref segmentId2.ToSegment();
            return (segment1.m_startNode == segment2.m_startNode) &
                   (segment1.m_endNode == segment2.m_endNode);
        }

        internal static ushort GetID(this NetNode node) {
            ref NetSegment seg = ref node.GetFirstSegment().ToSegment();
            bool startNode = Equals(ref node, seg.m_startNode);
            return startNode ? seg.m_startNode : seg.m_endNode;
        }

        internal static ushort GetID(this NetSegment segment) {
            foreach (var segmentID in IterateNodeSegments(segment.m_startNode)) {
                if (Equals(ref segment, segmentID)) {
                    return segmentID;
                }
            }
            throw new Exception("unreachable code");
        }

        public static ushort GetFirstSegment(ushort nodeID) => nodeID.ToNode().GetFirstSegment();
        public static ushort GetFirstSegment(this ref NetNode node) {
            ushort segmentID = 0;
            int i;
            for (i = 0; i < 8; ++i) {
                segmentID = node.GetSegment(i);
                if (segmentID != 0)
                    break;
            }
            return segmentID;
        }

        public static Vector3 GetSegmentDir(ushort segmentID, ushort nodeID) {
            bool startNode = IsStartNode(segmentID, nodeID);
            ref NetSegment segment = ref segmentID.ToSegment();
            return startNode ? segment.m_startDirection : segment.m_endDirection;
        }

        internal static float MaxNodeHW(ushort nodeId) {
            float ret = 0;
            foreach (var segmentId in IterateNodeSegments(nodeId)) {
                float hw = segmentId.ToSegment().Info.m_halfWidth;
                if (hw > ret)
                    ret = hw;
            }
            return ret;
        }

        #region Math

        /// Note: inverted flag or LHT does not influce the beizer.
        internal static Bezier3 CalculateSegmentBezier3(this ref NetSegment seg, bool bStartNode=true) {
            ref NetNode startNode = ref seg.m_startNode.ToNode();
            ref NetNode endNode = ref seg.m_endNode.ToNode();
            Bezier3 bezier = new Bezier3 {
                a = startNode.m_position,
                d = endNode.m_position,
            };
            NetSegment.CalculateMiddlePoints(
                bezier.a, seg.m_startDirection,
                bezier.d, seg.m_endDirection,
                startNode.m_flags.IsFlagSet(NetNode.Flags.Middle),
                endNode.m_flags.IsFlagSet(NetNode.Flags.Middle),
                out bezier.b,
                out bezier.c);
            if (!bStartNode)
                bezier = bezier.Invert();
            return bezier;
        }

        /// <param name="startNode"> if true the bezier is inverted so that it will be facing start node</param>
        /// Note: inverted flag or LHT does not influce the beizer.
        internal static Bezier2 CalculateSegmentBezier2(ushort segmentId, bool startNode) {
            Bezier3 bezier3 = segmentId.ToSegment().CalculateSegmentBezier3(startNode);
            Bezier2 bezier2 = bezier3.ToCSBezier2();
            return bezier2;
        }

        /// <param name="endNodeID">bezier will be facing endNodeID</param>
        internal static Bezier2 CalculateSegmentBezier2(ushort segmentId, ushort endNodeID) {
            bool startNode = !IsStartNode(segmentId, endNodeID);
            return CalculateSegmentBezier2(segmentId, startNode);
        }

        internal static float GetClosestT(this ref NetSegment segment, Vector3 position) {
            Bezier3 bezier = segment.CalculateSegmentBezier3();
            return bezier.GetClosestT(position);
        }

        /// <param name="bLeft2">if other segment is to the left side of segmentID.</param>
        /// <param name="cornerPoint">is normalized</param>
        /// <param name="cornerDir">is normalized</param>
        internal static void CalculateCorner(
            ushort segmentID, ushort nodeID, bool bLeft2,
            out Vector2 cornerPoint, out Vector2 cornerDir) {
            segmentID.ToSegment().CalculateCorner(
                segmentID,
                true,
                IsStartNode(segmentID, nodeID),
                !bLeft2, // leftSide = if this segment is to the left of the other segment = !bLeft2
                out Vector3 cornerPos,
                out Vector3 cornerDirection,
                out bool smooth);
            cornerPoint = cornerPos.ToCS2D();
            cornerDir = cornerDirection.ToCS2D().normalized;
        }

        /// <param name="bLeft2">if other segment is to the left side of segmentID.</param>
        internal static void CalculateOtherCorner(
            ushort segmentID, ushort nodeID, bool bLeft2,
            out Vector2 cornerPoint, out Vector2 cornerDir) {
            ushort otherSegmentID = bLeft2 ?
                segmentID.ToSegment().GetLeftSegment(nodeID) :
                segmentID.ToSegment().GetRightSegment(nodeID);
            CalculateCorner(otherSegmentID, nodeID, !bLeft2,
                            out cornerPoint, out cornerDir);
        }

        #endregion math

        public static float SampleHeight(Vector2 point) {
            return terrainMan.SampleDetailHeightSmooth(point.ToCS3D(0));
        }

        public static Vector3 Get3DPos(Vector2 point) {
            return point.ToCS3D(SampleHeight(point));
        }

        #region copied from TMPE
        public static bool LHT => TrafficDrivesOnLeft;
        public static bool RHT => !LHT;
        public static bool TrafficDrivesOnLeft =>
            Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic
                == SimulationMetaData.MetaBool.True;

        public static bool CanConnectPathToSegment(ushort segmentID) =>
            segmentID.ToSegment().CanConnectPath();

        public static bool CanConnectPath(this ref NetSegment segment) =>
            segment.Info.m_netAI is RoadAI & segment.Info.m_hasPedestrianLanes;

        public static bool CanConnectPath(this NetInfo info) =>
            info.m_netAI is RoadAI & info.m_hasPedestrianLanes;

        public static bool IsStartNode(ushort segmentId, ushort nodeId) =>
            segmentId.ToSegment().m_startNode == nodeId;

        public static ushort GetSegmentNode(ushort segmentID, bool startNode) =>
            segmentID.ToSegment().GetNode(startNode);

        public static ushort GetNode(this ref NetSegment segment, bool startNode) =>
            startNode ? segment.m_startNode : segment.m_endNode;

        public static bool HasNode(ushort segmentId, ushort nodeId) =>
            segmentId.ToSegment().m_startNode == nodeId || segmentId.ToSegment().m_endNode == nodeId;

        public static ushort GetSharedNode(ushort segmentID1, ushort segmentID2) =>
            segmentID1.ToSegment().GetSharedNode(segmentID2);

        public static bool IsSegmentValid(ushort segmentId) {
            if (segmentId == 0)
                return false;
            if (segmentId.ToSegment().Info == null)
                return false;
            return segmentId.ToSegment().m_flags
                .CheckFlags(required: NetSegment.Flags.Created, forbidden: NetSegment.Flags.Deleted);
        }

        public static bool IsNodeValid(ushort nodeId) {
            if (nodeId == 0)
                return false;
            if (nodeId.ToNode().Info == null)
                return false;
            return nodeId.ToNode().m_flags
                .CheckFlags(required: NetNode.Flags.Created, forbidden: NetNode.Flags.Deleted);
        }

        public static bool IsLaneValid(uint laneId) {
            if (laneId != 0) {
                return laneId.ToLane().Flags().
                    CheckFlags(required: NetLane.Flags.Created, forbidden: NetLane.Flags.Deleted);
            }
            return false;
        }

        public static ushort GetHeadNode(ref NetSegment segment) {
            // tail node>-------->head node
            bool invert = (segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
            invert = invert ^ LHT;
            if (invert) {
                return segment.m_startNode;
            } else {
                return segment.m_endNode;
            }
        }

        public static ushort GetHeadNode(ushort segmentId) =>
            GetHeadNode(ref segmentId.ToSegment());

        public static ushort GetTailNode(ref NetSegment segment) {
            bool invert = (segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
            invert = invert ^ LHT;
            if (!invert) {
                return segment.m_startNode;
            } else {
                return segment.m_endNode;
            }//endif
        }

        public static ushort GetTailNode(ushort segmentId) =>
            GetTailNode(ref segmentId.ToSegment());

        public static bool CalculateIsOneWay(ushort segmentId) {
            int forward = 0;
            int backward = 0;
            segmentId.ToSegment().CountLanes(
                segmentId,
                NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train |
                VehicleInfo.VehicleType.Tram | VehicleInfo.VehicleType.Metro |
                VehicleInfo.VehicleType.Monorail,
                ref forward,
                ref backward);
            return (forward == 0) ^ (backward == 0);
        }

        #endregion

        public struct NodeSegments {
            public ushort[] segments;
            public int count;
            void Add(ushort segmentID) {
                segments[count++] = segmentID;
            }

            public NodeSegments(ushort nodeID) {
                segments = new ushort[8];
                count = 0;

                ushort segmentID = GetFirstSegment(nodeID);
                Add(segmentID);
                while (true) {
                    segmentID = segmentID.ToSegment().GetLeftSegment(nodeID);
                    if (segmentID == segments[0])
                        break;
                    else
                        Add(segmentID);
                }
            }
        }

        /// <summary>
        /// returns a counter-clockwise list of segments of the given node ID.
        /// </summary>
        public static IEnumerable<ushort> GetCCSegList(ushort nodeID) {
            ushort segmentID0 = GetFirstSegment(nodeID);
            Assertion.Assert(segmentID0 != 0, "GetFirstSegment!=0");
            yield return segmentID0;
            ushort segmentID = segmentID0;

            // add the rest of the segments.
            while (true) {
                segmentID = segmentID.ToSegment().GetRightSegment(nodeID);
                if ((segmentID == 0) | (segmentID == segmentID0))
                    yield break;
                else
                    yield return segmentID;
            }
        }

        /// <summary>
        /// returns a clock-wise list of segments of the given node ID.
        /// </summary>
        public static IEnumerable<ushort> GetCWSegList(ushort nodeID) {
            ushort segmentID0 = GetFirstSegment(nodeID);
            Assertion.Assert(segmentID0 != 0, "GetFirstSegment!=0");
            yield return segmentID0;
            ushort segmentID = segmentID0;

            // add the rest of the segments.
            while (true) {
                segmentID = segmentID.ToSegment().GetLeftSegment(nodeID);
                if ((segmentID == 0) | (segmentID == segmentID0))
                    yield break;
                else
                    yield return segmentID;
            }
        }

        public static IEnumerable<ushort> IterateNodeSegments(ushort nodeID) {
            for (int i = 0; i < 8; ++i) {
                ushort segmentID = nodeID.ToNode().GetSegment(i);
                if (segmentID != 0) {
                    yield return segmentID;
                }
            }
        }

        [Obsolete("use IterateNodeSegments instead")]
        internal static IEnumerable<ushort> GetSegmentsCoroutine(ushort nodeID)
            => IterateNodeSegments(nodeID);

        public static void LaneTest(ushort segmentId) {
            string message = $"STRANGE LANE ISSUE: lane count mismatch for " +
                $"segment:{segmentId} Info:{segmentId.ToSegment().Info} IsSegmentValid={IsSegmentValid(segmentId)}\n";
            if (segmentId.ToSegment().Info != null) {

                var laneIDs = new List<uint>();
                for (uint laneID = segmentId.ToSegment().m_lanes;
                    laneID != 0;
                    laneID = laneID.ToLane().m_nextLane) {
                    laneIDs.Add(laneID);
                }

                var laneInfos = segmentId.ToSegment().Info.m_lanes;

                if (laneIDs.Count == laneInfos.Length) return;

                string m1 = "laneIDs=\n";
                foreach (uint laneID in laneIDs)
                    m1 += $"\tlaneID:{laneID} flags:{laneID.ToLane().m_flags} segment:{laneID.ToLane().m_segment} bezier.a={laneID.ToLane().m_bezier.a}\n";

                string m2 = "laneInfoss=\n";
                for (int i = 0; i < laneInfos.Length; ++i) {
                    m2 += $"\tlaneID:{laneInfos[i]} dir:{laneInfos[i].m_direction} laneType:{laneInfos[i].m_laneType} vehicleType:{laneInfos[i].m_vehicleType} pos:{laneInfos[i].m_position}\n";
                }

                message += m1 + m2;
            }
            Log.Error(message, true);
            Log.LogToFileSimple(file: "NodeControler.Strange.log", message: message);
        }

        public static IEnumerable<uint> IterateNodeLanes(ushort nodeId) {
            int idx = 0;
            if (nodeId.ToNode().Info == null) {
                Log.Error("null info: potentially caused by missing assets");
                yield break;
            }
            for (uint laneID = nodeId.ToNode().m_lane;
                laneID != 0;
                laneID = laneID.ToLane().m_nextLane, idx++) {
                yield return laneID;
            }
        }

        // requires testing.
        //public static bool IsLaneHeadingTowardsStartNode(uint laneID, int laneIndex) {
        //    ushort segmentID = laneID.ToLane().m_segment;
        //    var laneInfo = segmentID.ToSegment().Info.m_lanes[laneIndex];
        //    bool backward = laneInfo.m_finalDirection == NetInfo.Direction.Backward;
        //    bool inverted = segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
        //    return backward ^ inverted;
        //}


        public static IEnumerable<LaneData> IterateSegmentLanes(ushort segmentId) {
            int idx = 0;
            if (segmentId.ToSegment().Info == null) {
                Log.Error("null info: potentially caused by missing assets");
                yield break;
            }
            int n = segmentId.ToSegment().Info.m_lanes.Length;
            bool inverted = segmentId.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
            for (uint laneID = segmentId.ToSegment().m_lanes;
                laneID != 0 && idx < n;
                laneID = laneID.ToLane().m_nextLane, idx++) {
                var laneInfo = segmentId.ToSegment().Info.m_lanes[idx];
                bool forward = laneInfo.m_finalDirection == NetInfo.Direction.Forward;
                var ret = new LaneData {
                    LaneID = laneID,
                    LaneIndex = idx,
                    LaneInfo = laneInfo,
                    StartNode = forward ^ !inverted,
                };
                yield return ret;
            }
        }

        public static IEnumerable<LaneData> IterateLanes(
         ushort segmentId,
         bool? startNode = null,
         NetInfo.LaneType laneType = NetInfo.LaneType.All,
         VehicleInfo.VehicleType vehicleType = VehicleInfo.VehicleType.All) {
            foreach (LaneData laneData in IterateSegmentLanes(segmentId)) {
                if (startNode != null && startNode != laneData.StartNode)
                    continue;
                if (!laneData.LaneInfo.m_laneType.IsFlagSet(laneType))
                    continue;
                if (!laneData.LaneInfo.m_vehicleType.IsFlagSet(vehicleType))
                    continue;
                yield return laneData;
            }
        }



        /// <summary>
        /// sorted from outer lane to inner lane when heading toward <paramref name="startNode"/>
        /// </summary>
        /// <param name="segmentId"></param>
        /// <param name="startNode"></param>
        /// <param name="laneType"></param>
        /// <param name="vehicleType"></param>
        /// <returns></returns>
        public static LaneData[] GetSortedLanes(
            ushort segmentId,
            bool? startNode,
            NetInfo.LaneType laneType = NetInfo.LaneType.All,
            VehicleInfo.VehicleType vehicleType = VehicleInfo.VehicleType.All) {
            var lanes = IterateLanes(
                segmentId: segmentId,
                startNode: startNode,
                laneType: laneType,
                vehicleType: vehicleType).ToArray();

            LaneData[] ret = new LaneData[lanes.Length];
            for (int i = 0; i < lanes.Length; ++i) {
                int j = segmentId.ToSegment().Info.m_sortedLanes[i];
                ret[i] = lanes[j];
            }

            // make sure that the outmost lane is the first lane.
            bool reverse = ret[0].LaneInfo.m_direction == NetInfo.Direction.Backward;

            if (reverse) {
                // reverse order so that the first lane is the outer lane.
                ret = ret.Reverse().ToArray();
            }
            return ret;
        }

        public static int GetLaneIndex(uint laneID) {
            ushort segmentId = laneID.ToLane().m_segment;
            var id = segmentId.ToSegment().m_lanes;
            
            for(int i = 0;
                i< segmentId.ToSegment().Info.m_lanes.Length && id != 0;
                i++) {
                if (id == laneID)
                    return i;
                id = id.ToLane().m_nextLane;
            }
            return -1;
        }

    }

    public struct LaneData {
        public uint LaneID;
        public int LaneIndex;
        public NetInfo.Lane LaneInfo;
        public bool StartNode;

        public LaneData(uint laneID, int laneIndex = -1) {
            LaneID = laneID;
            if (laneIndex < 0)
                laneIndex = NetUtil.GetLaneIndex(laneID);
            LaneIndex = laneIndex;

            ushort segmentID = LaneID.ToLane().m_segment;
            LaneInfo = segmentID.ToSegment().Info.m_lanes[LaneIndex];
            bool forward = LaneInfo.m_finalDirection == NetInfo.Direction.Forward;
            bool inverted = segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
            StartNode = forward ^ !inverted;
        }

        public readonly ushort SegmentID => Lane.m_segment;
        public readonly ref NetSegment Segment => ref SegmentID.ToSegment();
        public readonly ref NetLane Lane => ref LaneID.ToLane();
        public readonly ushort NodeID => StartNode ? Segment.m_startNode : Segment.m_endNode;
        public readonly NetLane.Flags Flags {
            get => (NetLane.Flags)Lane.m_flags;
            set => LaneID.ToLane().m_flags = (ushort)value;
        }
        public readonly Bezier3 Bezier => Lane.m_bezier;
        public override string ToString() => $"LaneData:[segment:{SegmentID} node:{NodeID} lane ID:{LaneID} {LaneInfo.m_laneType} {LaneInfo.m_vehicleType}]";
    }


}

