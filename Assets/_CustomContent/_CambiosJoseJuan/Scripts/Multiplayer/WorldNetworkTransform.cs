using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Clase propia
namespace Mirror
{
    public class WorldNetworkTransform : NetworkTransformBase
    {
        protected override Transform targetComponent => transform;

        [Header("Local/Global coordinates")]
        public bool isLocal = false;

        protected override NTSnapshot ConstructSnapshot()
        {
            // NetworkTime.localTime for double precision until Unity has it too
            return new NTSnapshot(
                // our local time is what the other end uses as remote time
                NetworkTime.localTime,
                // the other end fills out local time itself
                0,
                isLocal ? targetComponent.localPosition : targetComponent.position,
                isLocal ? targetComponent.localRotation : targetComponent.rotation,
                targetComponent.localScale
            );
        }

        protected override void ApplySnapshot(NTSnapshot start, NTSnapshot goal, NTSnapshot interpolated)
        {
            // local position/rotation for VR support
            //
            // if syncPosition/Rotation/Scale is disabled then we received nulls
            // -> current position/rotation/scale would've been added as snapshot
            // -> we still interpolated
            // -> but simply don't apply it. if the user doesn't want to sync
            //    scale, then we should not touch scale etc.
            if (syncPosition)
                if (isLocal) targetComponent.localPosition = interpolatePosition ? interpolated.position : goal.position;
                else targetComponent.position = interpolatePosition ? interpolated.position : goal.position;

            if (syncRotation)
                if (isLocal) targetComponent.localRotation = interpolateRotation ? interpolated.rotation : goal.rotation;
                else targetComponent.rotation = interpolateRotation ? interpolated.rotation : goal.rotation;

            if (syncScale)
                targetComponent.localScale = interpolateScale ? interpolated.scale : goal.scale;
        }

        protected override void OnClientToServerSync(Vector3? position, Quaternion? rotation, Vector3? scale)
        {
            // only apply if in client authority mode
            if (!clientAuthority) return;

            // protect against ever growing buffer size attacks
            if (serverBuffer.Count >= bufferSizeLimit) return;

            // only player owned objects (with a connection) can send to
            // server. we can get the timestamp from the connection.
            double timestamp = connectionToClient.remoteTimeStamp;
#if onlySyncOnChange_BANDWIDTH_SAVING
            if (onlySyncOnChange)
            {
                double timeIntervalCheck = bufferResetMultiplier * sendInterval;

                if (serverBuffer.Count > 0 && serverBuffer.Values[serverBuffer.Count - 1].remoteTimestamp + timeIntervalCheck < timestamp)
                {
                    Reset();
                }
            }
#endif
            // position, rotation, scale can have no value if same as last time.
            // saves bandwidth.
            // but we still need to feed it to snapshot interpolation. we can't
            // just have gaps in there if nothing has changed. for example, if
            //   client sends snapshot at t=0
            //   client sends nothing for 10s because not moved
            //   client sends snapshot at t=10
            // then the server would assume that it's one super slow move and
            // replay it for 10 seconds.
            if (!position.HasValue) position = serverBuffer.Count > 0 ? serverBuffer.Values[serverBuffer.Count - 1].position : 
                    (isLocal ? targetComponent.localPosition : targetComponent.position);
            if (!rotation.HasValue) rotation = serverBuffer.Count > 0 ? serverBuffer.Values[serverBuffer.Count - 1].rotation :
                    (isLocal ? targetComponent.localRotation : targetComponent.rotation);
            if (!scale.HasValue) scale = serverBuffer.Count > 0 ? serverBuffer.Values[serverBuffer.Count - 1].scale : targetComponent.localScale;

            // construct snapshot with batch timestamp to save bandwidth
            NTSnapshot snapshot = new NTSnapshot(
                timestamp,
                NetworkTime.localTime,
                position.Value, rotation.Value, scale.Value
            );

            // add to buffer (or drop if older than first element)
            SnapshotInterpolation.InsertIfNewEnough(snapshot, serverBuffer);
        }
        protected virtual void OnServerToClientSync(Vector3? position, Quaternion? rotation, Vector3? scale)
        {
            // in host mode, the server sends rpcs to all clients.
            // the host client itself will receive them too.
            // -> host server is always the source of truth
            // -> we can ignore any rpc on the host client
            // => otherwise host objects would have ever growing clientBuffers
            // (rpc goes to clients. if isServer is true too then we are host)
            if (isServer) return;

            // don't apply for local player with authority
            if (IsClientWithAuthority) return;

            // protect against ever growing buffer size attacks
            if (clientBuffer.Count >= bufferSizeLimit) return;

            // on the client, we receive rpcs for all entities.
            // not all of them have a connectionToServer.
            // but all of them go through NetworkClient.connection.
            // we can get the timestamp from there.
            double timestamp = NetworkClient.connection.remoteTimeStamp;
#if onlySyncOnChange_BANDWIDTH_SAVING
            if (onlySyncOnChange)
            {
                double timeIntervalCheck = bufferResetMultiplier * sendInterval;

                if (clientBuffer.Count > 0 && clientBuffer.Values[clientBuffer.Count - 1].remoteTimestamp + timeIntervalCheck < timestamp)
                {
                    Reset();
                }
            }
#endif
            // position, rotation, scale can have no value if same as last time.
            // saves bandwidth.
            // but we still need to feed it to snapshot interpolation. we can't
            // just have gaps in there if nothing has changed. for example, if
            //   client sends snapshot at t=0
            //   client sends nothing for 10s because not moved
            //   client sends snapshot at t=10
            // then the server would assume that it's one super slow move and
            // replay it for 10 seconds.
            if (!position.HasValue) position = clientBuffer.Count > 0 ? clientBuffer.Values[clientBuffer.Count - 1].position :
                    (isLocal ? targetComponent.localPosition : targetComponent.position);
            if (!rotation.HasValue) rotation = clientBuffer.Count > 0 ? clientBuffer.Values[clientBuffer.Count - 1].rotation : 
                    (isLocal ? targetComponent.localRotation : targetComponent.rotation);
            if (!scale.HasValue) scale = clientBuffer.Count > 0 ? clientBuffer.Values[clientBuffer.Count - 1].scale : targetComponent.localScale;

            // construct snapshot with batch timestamp to save bandwidth
            NTSnapshot snapshot = new NTSnapshot(
                timestamp,
                NetworkTime.localTime,
                position.Value, rotation.Value, scale.Value
            );

            // add to buffer (or drop if older than first element)
            SnapshotInterpolation.InsertIfNewEnough(snapshot, clientBuffer);
        }


        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            // sync target component's position on spawn.
            // fixes https://github.com/vis2k/Mirror/pull/3051/
            // (Spawn message wouldn't sync NTChild positions either)
            if (initialState)
            {
                if (syncPosition) writer.WriteVector3(isLocal ? targetComponent.localPosition : targetComponent.position);
                if (syncRotation) writer.WriteQuaternion(isLocal ? targetComponent.localRotation : targetComponent.rotation);
                if (syncScale) writer.WriteVector3(targetComponent.localScale);
                return true;
            }
            return false;
        }


        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            // sync target component's position on spawn.
            // fixes https://github.com/vis2k/Mirror/pull/3051/
            // (Spawn message wouldn't sync NTChild positions either)
            if (initialState)
            {
                if (isLocal)
                {
                    if (syncPosition) targetComponent.localPosition = reader.ReadVector3();
                    if (syncRotation) targetComponent.localRotation = reader.ReadQuaternion();
                } else
                {
                    if (syncPosition) targetComponent.position = reader.ReadVector3();
                    if (syncRotation) targetComponent.rotation = reader.ReadQuaternion();
                }
                if (syncScale) targetComponent.localScale = reader.ReadVector3();
            }
        }
    }
}
