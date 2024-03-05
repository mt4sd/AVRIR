#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor.SceneManagement;
using System.IO;
using Mirror;
using UnityEditor.Build.Reporting;
using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

//[CustomEditor(typeof(TrackedPoseDriverDissection))]
public class EditorDissectionManagement: OdinEditorWindow{
    // public KeyCode Shortcut_LeftHand;
    // public KeyCode Shortcut_RightHand;

    [MenuItem("CustomTools/Editor Management")]
    private static void OpenWindow()
    {
        EditorDissectionManagement window = GetWindow<EditorDissectionManagement>();
        window.Show();
    }

    [FoldoutGroup("Create builds")]
    [FolderPath]
    public string BuildFolder;
    [FoldoutGroup("Create builds")]
    public string AppIdentificator = "com.GTMA.AVRIR";
    [FoldoutGroup("Create builds")]
    public string AppVersion = "1.0";
    [FoldoutGroup("Create builds")]
    public string CompanyName = "ULPGC - GTMA";
    [FoldoutGroup("Create builds")]
    public string ProductName = "AVRIR";
    [FoldoutGroup("Create builds")]
    public string NameExecutable = "AVRIR";
    [FoldoutGroup("Create builds")]
    public List<string> ScenesToInclude = new List<string>(new string[] {
        "Assets/_CustomContent/Scenes/InitScene.unity",
        "Assets/_CustomContent/Scenes/Dissection.unity", 
        "Assets/_CustomContent/Scenes/EmptyScene.unity"
    });
    [FoldoutGroup("Create builds")]
    public string OculusSDKFolder = "Assets/Oculus";

    [FoldoutGroup("Scene")]
    [ButtonGroup("Scene/Head")]
    [Button(ButtonSizes.Gigantic)]
    public void SelectHead(){
        List<Camera> cameras = new List<Camera>(GameObject.FindObjectsOfType<Camera>());                    
        Camera cam = cameras.Find((x)=>x.name.Equals("Main Camera") && CheckAuthority(x.gameObject));
        if(cam !=null){
            Selection.activeGameObject = cam.transform.gameObject;
        }
    }
    [FoldoutGroup("Scene")]
    [ButtonGroup("Scene/Head")]
    [Button(ButtonSizes.Gigantic)]
    public void SetHeadOnTable(){
        SelectLeftHand();
        Vector3 currentPos = Selection.activeGameObject.transform.position;
        currentPos.y = 1f;
        Selection.activeGameObject.transform.position = currentPos;

        SelectRightHand();
        currentPos = Selection.activeGameObject.transform.position;
        currentPos.y = 1f;
        Selection.activeGameObject.transform.position = currentPos;

        SelectHead();        
        currentPos = Selection.activeGameObject.transform.position;
        currentPos.y = 1.25f;
        Selection.activeGameObject.transform.position = currentPos;
    }
    
    [FoldoutGroup("Scene")]
    [ButtonGroup("Scene/Head")]
    [Button(ButtonSizes.Gigantic)]
    public void SelectOffset(){

        List<GameObject> hands = new List<GameObject>(GameObject.FindGameObjectsWithTag("RealHand"));                    
        GameObject hand = hands.Find((x)=>x.name.Contains("Right") && CheckAuthority(x));
        if(hand !=null){
            Selection.activeGameObject = hand.transform.parent.gameObject;
        }
    }

    [FoldoutGroup("Scene/Hands Management", true)]
    [ButtonGroup("Scene/Hands Management/Buttons")]
    [Button(ButtonSizes.Gigantic)]
    public void SelectLeftHand(){
        List<GameObject> hands = new List<GameObject>(GameObject.FindGameObjectsWithTag("RealHand"));                    
        GameObject hand = hands.Find((x)=>x.name.Contains("Left") && CheckAuthority(x));
        if(hand !=null){
            Selection.activeGameObject = hand;
        }
    }
    
    [FoldoutGroup("Scene/Hands Management")]
    [ButtonGroup("Scene/Hands Management/Buttons")]
    [Button(ButtonSizes.Gigantic)]
    public void SelectRightHand(){

        List<GameObject> hands = new List<GameObject>(GameObject.FindGameObjectsWithTag("RealHand"));                    
        GameObject hand = hands.Find((x)=>x.name.Contains("Right") && CheckAuthority(x));
        if(hand !=null){
            Selection.activeGameObject = hand;
        }
    }

    private bool CheckAuthority(GameObject hand)
    {
        NetworkIdentity identity = hand.GetComponentInParent<NetworkIdentity>();
        return (identity == null || identity.hasAuthority);
    }

    [FoldoutGroup("Scene/Objects", true)]
    [ButtonGroup("Scene/Objects/Buttons")]
    [Button("Morgue Controller", ButtonSizes.Gigantic)]
    public void SelectMorgueController(){
        Selection.activeGameObject = GameObject.Find("_MorgueController");
    }

    [FoldoutGroup("Scene/Objects", true)]
    [ButtonGroup("Scene/Objects/Buttons")]
    [Button("Reorder Controller", ButtonSizes.Gigantic)]
    public void SelectReorderController(){
        Selection.activeGameObject = GameObject.Find("ReorderController");
    }

    [FoldoutGroup("ChangeScene")]
    [ButtonGroup("ChangeScene/LoadNew")]
    [Button(ButtonSizes.Large)]
    public void SelectInitScene()
    {
        EditorSceneManager.OpenScene("Assets/_CustomContent/Scenes/InitScene.unity");
    }

    [ButtonGroup("ChangeScene/LoadNew")]
    [Button(ButtonSizes.Large)]
    public void SelectDissectionScene()
    {
        EditorSceneManager.OpenScene("Assets/_CustomContent/Scenes/Dissection.unity");
    }


    private string currentZAnatomyPath = "";
    [FoldoutGroup("zAnatomy Adapter")]
    [Button(ButtonSizes.Gigantic)]
    public void AdaptAllZAnatomyPrefabsToOnline(){
        NetworkManager manager = GameObject.Find("_MirrorRoomManager").GetComponent<NetworkManager>();
        // Search all prefabs of zAnatomy in project
        Debug.Log("Find Assets Script started...");
        Debug.Log("Finding all Prefabs that have an zAnatomyController Component in /zAnatomy/ and /Resources/ folder...");
        string[] folderList = {"Assets/zAnatomy", "Assets/Resources"};
        string[] guids = AssetDatabase.FindAssets("t:Object", folderList); //new[] { "Assets/Prefabs" });

        foreach (string guid in guids)
        {
            //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object[] myObjs = AssetDatabase.LoadAllAssetsAtPath(myObjectPath);

            //Debug.Log("printing myObs now...");
            foreach (UnityEngine.Object thisObject in myObjs)
            {
                //Debug.Log(thisObject.name);
                //Debug.Log(thisObject.GetType().Name); 
                string myType = thisObject.GetType().Name;
                if (myType == "ZAnatomyController")
                {
                    Debug.Log("ZAnatomyController Found in...  " + thisObject.name + " at " + myObjectPath);
                    ZAnatomy.ZAnatomyController zAnatomyController = thisObject as ZAnatomy.ZAnatomyController;
                    currentZAnatomyPath = myObjectPath.Replace(".prefab","");//AssetDatabase.GetAssetPath(zAnatomyController);
                    currentZAnatomyPath = Path.GetDirectoryName(currentZAnatomyPath).Replace("\\","/");
                    Debug.Log("CurrentZAnatomyPath = "+currentZAnatomyPath);
                    // Iterate mesh interactives
                    zAnatomyController.IterateDataListWithFunction((ZAnatomy.DataList.DataModel data)=>{
                        if(!zAnatomyController.UseOrganPrefabs){
                            // Reference to MeshInteractivePrefab
                            if(data.Model != null){
                                // Direct reference to childObject
                                AddCustomScripts(manager, data.Model);
                            }
                        }else{
                            ZAnatomy.ZAnatomyController.OrganPrefab organ = zAnatomyController.OrganPrefabsList.Find((x)=>x.ID.Equals(data.NameSource));
                            if(organ!=null){
                                if(organ.UnifiedPrefab != null){
                                    AddCustomScripts(manager,organ.UnifiedPrefab);
                                }else{
                                    string path = currentZAnatomyPath + "/"+organ.UnifiedPrefabRelativePath+".prefab";
                                    Debug.Log("--- Updating meshInteractive prefab in "+path);
                                    ZAnatomy.MeshInteractiveController meshLoadedByPath = AssetDatabase.LoadAssetAtPath<ZAnatomy.MeshInteractiveController>(path);
                                    if(meshLoadedByPath !=null){
                                        AddCustomScripts(manager,meshLoadedByPath);
                                    }else{
                                        Debug.LogError("------ Cannot find meshInteractive prefab on "+path);
                                    }
                                }
                            }else{
                                Debug.LogError("--- Cannot find organ prefab named "+data.NameSource);
                            }
                        }
                    });
                }
            }
        }
    }


    private void AddCustomScripts(NetworkManager manager, ZAnatomy.MeshInteractiveController meshInteractivePrefab){
        // Add online scripts

        // Check if exists or add new scripts to MeshInteractiveController
        // meshInteractivePrefab.AddComponent...
        Debug.Log("--- Adding new scripts to "+meshInteractivePrefab.name);
        if (meshInteractivePrefab.GetComponent<NetworkIdentity>() == null)
            meshInteractivePrefab.gameObject.AddComponent<NetworkIdentity>();
        if (meshInteractivePrefab.GetComponent<WorldNetworkTransform>() == null)
        {
            WorldNetworkTransform net = meshInteractivePrefab.gameObject.AddComponent<WorldNetworkTransform>();
            net.clientAuthority = true;
            net.sendInterval = 0.001f;
            net.interpolatePosition = false;
            net.interpolateRotation = false;
            net.positionSensitivity = 0;
            net.rotationSensitivity = 0;
        }
        if (meshInteractivePrefab.GetComponent<MeshInteractableOnline>() == null)
            meshInteractivePrefab.gameObject.AddComponent<MeshInteractableOnline>();

        if (!manager.spawnPrefabs.Contains(meshInteractivePrefab.gameObject))
            manager.spawnPrefabs.Add(meshInteractivePrefab.gameObject);
        // Iterate divided prefabs

        if (meshInteractivePrefab.DividedPrefab!=null){
            foreach(ZAnatomy.DividedOrganPiece piecePrefab in meshInteractivePrefab.DividedPrefab.Pieces){
                // AddCustomScripts(piecePrefab);
            }
        }else{
            // Load DividedPrefabPath
            string path = currentZAnatomyPath + "/"+meshInteractivePrefab.DividedPrefabRelativePath+".prefab";
            Debug.Log("--- Updating meshInteractive prefab in "+path);
            ZAnatomy.DividedOrganPiece loadedPiecePrefab = AssetDatabase.LoadAssetAtPath<ZAnatomy.DividedOrganPiece>(path);
                                
            AddCustomScripts(loadedPiecePrefab);
        }
    }
    private void AddCustomScripts(ZAnatomy.DividedOrganPiece dividedOrganPiecePrefab){
        // Add online scripts
        // Check if exists or add new scripts to DividedOrganPiece
        // dividedOrganPiecePrefab.AddComponent...
        Debug.Log("--- Adding new scripts to "+dividedOrganPiecePrefab.name);
    }

    //     protected override void OnGUI() {

    //         if(Event.current.type == EventType.KeyDown && Event.current.keyCode == Shortcut_LeftHand)
    // //        if(Input.GetKeyDown(Shortcut_LeftHand))//if (Event.current.keyCode == (KeyCode.LeftArrow))
    //         {
    //             SelectLeftHand();
    //         }

    //         if(Event.current.type == EventType.KeyDown && Event.current.keyCode == Shortcut_RightHand)
    //         // if(Input.GetKeyDown(Shortcut_RightHand))//if (Event.current.keyCode == (KeyCode.RightArrow))
    //         {
    //             SelectRightHand();
    //         }

    //         base.OnGUI();
    //     }
    
    // new string[] {
    //     "Assets/_CustomContent/Scenes/InitScene.unity",
    //     "Assets/_CustomContent/Scenes/Dissection.unity", 
    //     "Assets/_CustomContent/Scenes/EmptyScene.unity"}
    
    private void LoadBuildData(){
        // string[] scenes = new string[] {
        //     "Assets/_CustomContent/Scenes/InitScene.unity",
        //     "Assets/_CustomContent/Scenes/Dissection.unity", 
        //     "Assets/_CustomContent/Scenes/EmptyScene.unity"};
        
        // ScenesToInclude = new List<string>(scenes);
        AppIdentificator = PlayerSettings.applicationIdentifier;
        AppVersion = PlayerSettings.bundleVersion;
        CompanyName = PlayerSettings.companyName;
        ProductName = PlayerSettings.productName;
    }

    private void SetBuildData(){
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.productName = ProductName;
        PlayerSettings.applicationIdentifier = AppIdentificator+"v"+(AppVersion.Replace(".",""));
        PlayerSettings.bundleVersion = AppVersion;
        PlayerSettings.SplashScreen.show = false;
        Debug.Log("-- Setting build data:");
        Debug.Log("---- AppIdentificator: "+AppIdentificator);
        Debug.Log("---- BundleVersion: "+AppVersion);
    }

    [FoldoutGroup("Create builds")]
    [HorizontalGroup("Create builds/Buttons")]
    [Button(ButtonSizes.Gigantic)]
    public void Windows(){
        UseOculusSDK(true);        
        BuildVersion(ScenesToInclude.ToArray(), NameExecutable, BuildTarget.StandaloneWindows64);
    }

    [FoldoutGroup("Create builds")]
    [HorizontalGroup("Create builds/Buttons")]
    [Button(ButtonSizes.Gigantic)]
    public void Android(){
        UseOculusSDK(true);
        BuildVersion(ScenesToInclude.ToArray(), NameExecutable, BuildTarget.Android);
    }

    [FoldoutGroup("Create builds")]
    [HorizontalGroup("Create builds/Buttons")]
    [Button(ButtonSizes.Gigantic)]
    public void WebGL(){
        
        Debug.Log(" WEBGL export not supported by now");
        return; // Temporary disabled feature
        UseOculusSDK(false);
        BuildVersion(ScenesToInclude.ToArray(), NameExecutable, BuildTarget.WebGL);
    }

    [FoldoutGroup("Create builds")]
    // [HorizontalGroup("Create builds/Buttons")]
    [Button(ButtonSizes.Gigantic)]
    public void AllBuilds(){
        WebGL();
        Windows();
        Android();
        OpenBuildFolder();
    }

    [FoldoutGroup("Create builds")]
    [Button()]
    private void UseOculusSDK(bool enableSDK){
        return; // Temporary disabled feature

        string disabledFolder = OculusSDKFolder+"~";
        string enabledFolder = OculusSDKFolder;

        Debug.Log("--- Set Oculus SDK: "+enableSDK);
        bool oculusEnabled = Directory.Exists(OculusSDKFolder);
        Debug.Log("----- OculusEnabled? "+oculusEnabled);

        if(oculusEnabled){
            if(!enableSDK){
                Debug.Log("----- Renaming from "+enabledFolder+ " to "+disabledFolder);
                Directory.Move(enabledFolder, disabledFolder);
            }else{
                Debug.Log("----- No action needed. Oculus already enabled.");
            }
        }else{
            if(enableSDK){
                Debug.Log("----- Renaming from "+disabledFolder+ " to "+enabledFolder);
                Directory.Move(disabledFolder, enabledFolder);
            }else{
                Debug.Log("----- No action needed. Oculus already disabled.");
            }
        }

        // AssetDatabase.Refresh();
    }
    
    private void BuildVersion(string[] levels, string nameExe, BuildTarget target = BuildTarget.StandaloneWindows){
        Debug.Log("Creating build - "+target.ToString());

        SetBuildData();

        if(string.IsNullOrEmpty(BuildFolder)){
            Debug.LogError("BuildFolder is empty. Select a folder to create builds and start again.");
            return;
        }

        string version = AppVersion.Replace(".","");
        string finalFolderPath = BuildFolder;// Path.Combine(BuildFolder, "v"+version);
        Debug.Log("Checking parent folder: "+ finalFolderPath);
        if(!Directory.Exists(finalFolderPath)){
            Debug.Log("--- Not exist. Creating...");
            Directory.CreateDirectory(finalFolderPath);
        }

        Debug.Log("Start building");
        // Get filename.
        string path = finalFolderPath;
        string extension = target == BuildTarget.StandaloneWindows64?".exe":".apk";
        if(target == BuildTarget.StandaloneLinux64){
            extension = "";
        }
        // Build player.
        string parentVersion = nameExe+"-v"+version;
        string targetPlatform = target == BuildTarget.StandaloneWindows64?"Desktop":target.ToString();
        string folder = target == BuildTarget.Android?"":"/"+targetPlatform;
        string fileName = target == BuildTarget.Android?nameExe+"v"+version:nameExe;
        string finalFolder = path + "/"+parentVersion+folder+"/";
        string shortcutFolder = path + "/"+parentVersion;

        BuildReport buildReport = BuildPipeline.BuildPlayer(levels, finalFolder+fileName+extension, target, BuildOptions.None);
        Debug.Log("------- "+target.ToString()+ " - COMPLETED");
        DeleteBurstDebugInformationFolder(buildReport);

        if(target == BuildTarget.StandaloneWindows64){
            string nameFile = "AVRIR-Desktop.bat";
            string origin =Application.dataPath+"/"+nameFile ;
            string destiny = shortcutFolder+"/"+nameFile;
            Debug.Log("Copy bat file from "+ origin + " to "+destiny);
            File.Copy(origin, destiny, true);

            nameFile = "AVRIR-AdminRequest.bat";
            origin =Application.dataPath+"/"+nameFile ;
            destiny = finalFolder+"/"+nameFile;
            Debug.Log("Copy bat file from "+ origin + " to "+destiny);
            File.Copy(origin, destiny, true);
        }

        Debug.Log(target.ToString()+" version finished succesfully");
    }

    [FoldoutGroup("Create builds")]
    [Button()]
    public void OpenBuildFolder(){
        EditorUtility.RevealInFinder(BuildFolder);
    }

    static void DeleteBurstDebugInformationFolder([NotNull] BuildReport buildReport)
    {
        string outputPath = buildReport.summary.outputPath;
     
        try
        {
            string applicationName = Path.GetFileNameWithoutExtension(outputPath);
            string outputFolder = Path.GetDirectoryName(outputPath);
            Assert.IsNotNull(outputFolder);
     
            outputFolder = Path.GetFullPath(outputFolder);
     
            string burstDebugInformationDirectoryPath = Path.Combine(outputFolder, $"{applicationName}_BurstDebugInformation_DoNotShip");
     
            if (Directory.Exists(burstDebugInformationDirectoryPath))
            {
                Debug.Log($" > Deleting Burst debug information folder at path '{burstDebugInformationDirectoryPath}'...");
     
                Directory.Delete(burstDebugInformationDirectoryPath, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"An unexpected exception occurred while performing build cleanup: {e}");
        }
    }

}
#endif