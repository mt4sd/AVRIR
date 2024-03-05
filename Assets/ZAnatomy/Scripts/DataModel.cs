using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZAnatomy{
    [System.Serializable]
    public class DataList
    {
        [System.Serializable]
        public class DataModel{
            public int ID;
            public string Name;
            public string NameSource;
            public string Path;
            public string Color;
            public string Type;
            public ZAnatomy.ZAnatomyController.OrganType BodyPart;
            public List<string> ConnectedModelIDs_Anterior;
            public List<string> ConnectedModelIDs_Posterior;
            
            // Referenced after instantiation
            public ZAnatomy.MeshInteractiveController Model;
            public ZAnatomy.DividedOrganController DividedModelPrefab;

            public int CompareVolumeSize(DataModel newVolume){
                return Model.GetVolumeSize()>newVolume.Model.GetVolumeSize()?1:-1;
            }
        }

        public List<DataModel> DataModelsList;

        public List<DataModel> GetActiveModels(){
            List<DataModel> activeModelsList = new List<DataModel>();
            
            foreach(DataModel model in DataModelsList){
                if(model.Model.gameObject.activeSelf){
                    activeModelsList.Add(model);
                }
            }

            return activeModelsList;
        }

        public List<MeshInteractiveController> GetInteractiveMeshList(){
            List<MeshInteractiveController> newList = new List<MeshInteractiveController>();
            foreach(DataModel model in DataModelsList){
                newList.Add(model.Model);
            }

            return newList;
        }

        public static string ToJSON(DataList model){
            return JsonUtility.ToJson(model);
        }
    }
}