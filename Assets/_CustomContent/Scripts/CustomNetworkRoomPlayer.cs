using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkRoomPlayer : NetworkRoomPlayer
{
    public static System.Action<int, bool> OnReadyStateChanged;

    [SyncVar(hook = nameof(SelectedModelChanged))]
    public int SelectedModelID = -1;

    [SyncVar(hook = nameof(DesktopModeChanged))]
    public bool isDesktopEnabled = false;

    [SyncVar] // To keep references of enabled players when other player leaves room
    public bool isAdminTabletEnabled = false;

    [SyncVar(hook = nameof(AvatarChanged))]
    public int avatarId = 1;

    private void DesktopModeChanged(bool oldIndex, bool newIndex) {
        Debug.Log("DesktopModeChanged to "+newIndex);
        
        CustomNetworkRoomManager room = NetworkManager.singleton as CustomNetworkRoomManager;
        if (room != null)
        {
            room.SetDesktopMode(newIndex);
            // room.SelectedModelID = newModelID;
            room.UpdateClientIcons();
        }
    }

    private void SelectedModelChanged(int oldIndex, int newIndex) {
        Debug.Log("SelectedModelID = "+newIndex);
        
        CustomNetworkRoomManager room = NetworkManager.singleton as CustomNetworkRoomManager;
        if (room != null)
        {
            room.SetSelectedModel(newIndex);
            // room.SelectedModelID = newModelID;
        }
    }
    private void AvatarChanged(int oldIndex, int newIndex) {
        Debug.Log("AvatarChanged = "+newIndex);
        
        CustomNetworkRoomManager room = NetworkManager.singleton as CustomNetworkRoomManager;
        if(room != null){
            room.UpdateClientIcons();
        }
    }

    [Command]
    public void CmdSetNewModelID(int newModelID)
    {
        SelectedModelID = newModelID;
        // CustomNetworkRoomManager room = NetworkManager.singleton as CustomNetworkRoomManager;
        // if (room != null)
        // {
        //     room.SetSelectedModel(SelectedModelID);
        //     // room.SelectedModelID = newModelID;
        // }
    }

    [Command]
    public void CmdSetDesktop(bool isDesktop){
        isDesktopEnabled = isDesktop;
    }

    [Command]
    public void CmdInitMorgue()
    {
        CustomNetworkRoomManager room = NetworkManager.singleton as CustomNetworkRoomManager;
        if (room != null)
        {
            room.AfterAllPlayersLoadedDissection();
            // room.SelectedModelID = newModelID;
        }
    }

    #region SyncVar Hooks

    /// <summary>
    /// This is a hook that is invoked on clients when the index changes.
    /// </summary>
    /// <param name="oldIndex">The old index value</param>
    /// <param name="newIndex">The new index value</param>
    public override void IndexChanged(int oldIndex, int newIndex) {
        Debug.Log("IndexChanged");
    }

    /// <summary>
    /// This is a hook that is invoked on clients when a RoomPlayer switches between ready or not ready.
    /// <para>This function is called when the a client player calls CmdChangeReadyState.</para>
    /// </summary>
    /// <param name="newReadyState">New Ready State</param>
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) {
        Debug.Log("ReadyStateChanged index: "+index + " - new: "+newReadyState);
        
        if(OnReadyStateChanged!=null){
            OnReadyStateChanged(index,newReadyState);
        }
    }

    #endregion

    #region Room Client Virtuals

    /// <summary>
    /// This is a hook that is invoked on clients for all room player objects when entering the room.
    /// <para>Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called.</para>
    /// </summary>
    public override void OnClientEnterRoom() {
        Debug.Log("OnClientEnterRoom");
    }

    /// <summary>
    /// This is a hook that is invoked on clients for all room player objects when exiting the room.
    /// </summary>
    public override void OnClientExitRoom() {
        Debug.Log("OnClientExitRoom");
        
        CustomNetworkRoomManager room = NetworkManager.singleton as CustomNetworkRoomManager;
        if(room != null){
            room.UpdateClientIcons();
        }
        
        InitSceneCanvasController canvasController = GameObject.FindObjectOfType<InitSceneCanvasController>();
        if(canvasController != null && !isServer){
            canvasController.OnClientConnectedToServer();
        }
    }

    #endregion
}
