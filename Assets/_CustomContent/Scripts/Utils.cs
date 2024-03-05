using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class Utils{
    public static void CloseApp(){
        Utils.SaveSessionFile();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public static Color ColorFromHex(string hexColor){
        Color newCol;
        if (ColorUtility.TryParseHtmlString(hexColor, out newCol))
            return newCol;

        return Color.white;
    }
    
    public static string GetTimeString(float timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public static string GetNowDateTimeString(){
        return System.DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss");
    }

    public static string UpperFirstChar(string str){
        return char.ToUpper(str[0]) + str.Substring(1);
    }
    
    public static Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        Color newColor;
        if(correctionFactor < 0)
            newColor =  Color.Lerp(color, Color.black, -correctionFactor); 
        else
            newColor = Color.Lerp(color, Color.white, correctionFactor);

        //Debug.Log("ChangeColor from " + color +" to " + newColor + " - " + correctionFactor);
        return newColor;
    }

    public static void Shuffle<T>(this IList<T> list)  
    {  
        System.Random rng = new System.Random();  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static void ClearChilds(Transform parent, List<string> nameKeepObjects = null){
        foreach(Transform child in parent){
            if(nameKeepObjects == null || !nameKeepObjects.Contains(child.gameObject.name)){
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    public static void MeshInteractiveMorgueSetOnPositionByLeanTween(ZAnatomy.MeshInteractiveController meshInteractive, bool setInPlace, bool backToLastPos=false){
        LeanTween.cancel(meshInteractive.gameObject);
        
        // Do action and save transform data
        Vector3 startPosition = meshInteractive.transform.position; 
        Quaternion startRotation = meshInteractive.transform.rotation; 
        meshInteractive.MorgueSetOnPosition(setInPlace);

        Vector3 finalPosition = meshInteractive.transform.position;
        Quaternion finalRotation = meshInteractive.transform.rotation;

        // Reset meshInteractive
        meshInteractive.transform.position = startPosition;
        meshInteractive.transform.rotation = startRotation;

        // Animate by leantween 
        LeanTween.move(meshInteractive.gameObject, finalPosition, 1).setEaseOutCirc().setOnComplete(()=>{
            meshInteractive.transform.position = finalPosition;
        });

        LeanTween.rotate(meshInteractive.gameObject, finalRotation.eulerAngles, 1).setEaseOutCirc().setOnComplete(()=>{
            meshInteractive.transform.rotation = finalRotation;
        });
    }

    public static void DividedOrganSetPositionsByLeanTween(ZAnatomy.DividedOrganController dividedOrgan, List<Transform> positions){
        // Do & Save data
        List<Vector3> startPositions = new List<Vector3>(); 
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++){
            startPositions.Add(dividedOrgan.Pieces[i].transform.position);
        }

        dividedOrgan.SetPositions(positions);

        List<Vector3> finalPositions = new List<Vector3>(); 
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++){
            finalPositions.Add(dividedOrgan.Pieces[i].transform.position);
        }
        
        // Reset
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++)
        {
            ZAnatomy.DividedOrganPiece piece = dividedOrgan.Pieces[i];
            piece.gameObject.transform.position = startPositions[i];
        }

        // Leantween
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++)
        {
            ZAnatomy.DividedOrganPiece piece = dividedOrgan.Pieces[i];
            LeanTween.cancel(piece.gameObject);
            LeanTween.move(piece.gameObject, finalPositions[i], 1f);
        }
    }

    public static void DividedOrganGroupConnectToByLeanTween(ZAnatomy.DividedOrganGroup organGroup, ZAnatomy.DividedOrganPiece targetPiece, ZAnatomy.DividedOrganPiece newPiece){ 
        Vector3 startPosition = newPiece.transform.position; 
        Quaternion startRotation = newPiece.transform.rotation; 

        organGroup.ConnectTo(targetPiece, newPiece);

        Vector3 finalPosition = newPiece.transform.position;
        Quaternion finalRotation = newPiece.transform.rotation;
        
        newPiece.transform.position = startPosition;
        newPiece.transform.rotation = startRotation;

        LeanTween.cancel(newPiece.gameObject);
        LeanTween.move(newPiece.gameObject, finalPosition, 1).setEaseOutCirc().setOnComplete(() => {
            newPiece.transform.position = finalPosition;
        });

        LeanTween.rotate(newPiece.gameObject, finalRotation.eulerAngles, 1).setEaseOutCirc().setOnComplete(() => {
            newPiece.transform.rotation = finalRotation;
        });
    }

    public static void DividedOrganResetDataByLeanTween(ZAnatomy.DividedOrganController dividedOrgan){
        // Do & Save data
        List<Vector3> startPositions = new List<Vector3>(); 
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++){
            startPositions.Add(dividedOrgan.Pieces[i].transform.localPosition);
        }
        List<Vector3> startRotations = new List<Vector3>(); 
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++){
            startRotations.Add(dividedOrgan.Pieces[i].transform.localEulerAngles);
        }

        dividedOrgan.ResetData();

        List<Vector3> finalPositions = new List<Vector3>(); 
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++){
            finalPositions.Add(dividedOrgan.Pieces[i].transform.localPosition);
        }
        List<Vector3> finalRotations = new List<Vector3>(); 
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++){
            finalRotations.Add(dividedOrgan.Pieces[i].transform.localEulerAngles);
        }

        // Reset
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++)
        {
            ZAnatomy.DividedOrganPiece piece = dividedOrgan.Pieces[i];
            piece.gameObject.transform.localPosition = startPositions[i];
            piece.gameObject.transform.localEulerAngles = startRotations[i];
        }

        // Leantween
        for (int i = 0; i < dividedOrgan.Pieces.Count; i++)
        {
            ZAnatomy.DividedOrganPiece piece = dividedOrgan.Pieces[i];
            ZAnatomy.MeshInteractiveController meshInteractive = piece.GetComponent<ZAnatomy.MeshInteractiveController>();
            meshInteractive.CanBeGrabbed = false;
            
            LeanTween.cancel(piece.gameObject);
            LeanTween.moveLocal(piece.gameObject, finalPositions[i], 1f);
            LeanTween.rotateLocal(piece.gameObject, finalPositions[i], 1f);
            
            Vector3 finalPos = finalPositions[i];
            LeanTween.moveLocal(piece.gameObject, finalPos, 1f).setOnComplete(() =>
            {
                piece.transform.localPosition = finalPos;
            });

            Vector3 finalEuler = finalRotations[i];
            LeanTween.rotateLocal(piece.gameObject, finalEuler, 1f).setOnComplete(() => { 
                meshInteractive.CanBeGrabbed = true;
                piece.transform.localEulerAngles = finalEuler;
            });
        }
    }

    public static Vector3 GetDifferenceFromMeshCenter(ZAnatomy.MeshInteractiveController mesh){
        return mesh.transform.position-mesh.GetRenderer().bounds.center;
    }

    public static string GetProjectDataFolder(string fileName){
        
        string projectFolder = Application.platform == RuntimePlatform.WindowsPlayer ?Application.dataPath:Application.persistentDataPath;

        if(Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor){
            projectFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);//Application.dataPath;

            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.GetParent(Application.dataPath);

            projectFolder = directoryInfo.FullName;
        }

        projectFolder = System.IO.Directory.GetParent(projectFolder).FullName;
        string nameAvrirFolder = "AVRIR-SessionData";
        projectFolder = System.IO.Path.Join(projectFolder,nameAvrirFolder);

        if(!System.IO.Directory.Exists(projectFolder)){
            System.IO.Directory.CreateDirectory(projectFolder);
        }

        string finalFile = System.IO.Path.Join(projectFolder,fileName); 
        
        return finalFile;
    }

    public static string GetNameFromAnalytics(UserInitializerController.UserData userData){
        UserAnalytics userAnalytic = AnalyticsManager.GetInstance().userAnalytics.Find((x)=>x.appID== userData.XRObject.UserDataID);
        string finalName = userData.ColorName;
        if(userAnalytic != null){
            finalName = userAnalytic.colorName;
        }

        return finalName;
    }
    public static void SaveSessionFile(){
        // if(Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
        if(Application.isMobilePlatform)
            return;

        string datetime = AnalyticsManager.GetInstance().GetDateTimeStart();// Utils.GetNowDateTimeString();//System.DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss");
        string fileName = "DataSession_"+ datetime+".json";

        string sessionData = AnalyticsManager.GetInstance().GetDataJson();

        // string path = Application.platform == RuntimePlatform.WindowsPlayer?Application.dataPath:Application.persistentDataPath;
        // string finalPath = System.IO.Path.Join(path,fileName);

        string finalFile = Utils.GetProjectDataFolder(fileName);

        try{
            // Overwrite already created file with this name
            System.IO.File.WriteAllText(finalFile, sessionData);

            Debug.Log("Save session on file: "+finalFile);
            Debug.Log("data: "+sessionData);
        }
        catch (System.IO.IOException e) 
        {
            // recover from exception
            Debug.LogError("Exception write file. Trying again: "+e);
            SaveSessionFile();
        }
    }
}