using System.Collections.Generic;
using UnityEngine;

namespace ZAnatomy{
    public static class ZAnatomyUtils{
        public static string UpperFirstChar(string str){
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        
        // Copied from OVRCommon
        public static Transform FindChildRecursiveInChildren(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains(name))
                    return child;

                var result = child.FindChildRecursiveInChildren(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private static void CheckConnectedObjects(bool useKDTree, ZAnatomy.MeshInteractiveController original, List<ZAnatomy.DataList.DataModel> connectedModels,  List<ZAnatomy.DataList.DataModel> fullList, float radiusNeighbours, string layerMask = ""){

            Vector3 p1;
            Vector3 p2;
            Vector3 originalPosCloseToCenter;
            Vector3 colliderPosCloseToCenter;

            //float lastClosestDistance = -1;
            if(useKDTree){
                p1 = original.GetMeshKDTree().ClosestPointOnSurface(original.GetCenterPosition());
            }else{
                p1 = original.Collider.ClosestPoint(original.GetCenterPosition());
            }
            //ResourcesDataProfile.DataList.DataModel closestConnectedObject = null;
            foreach(ZAnatomy.DataList.DataModel colliderDataModel in fullList){
                if(colliderDataModel.Model != original){
                    
                    if(useKDTree){
                        p2 = colliderDataModel.Model.GetMeshKDTree().ClosestPointOnSurface(p1);//colliderDataModel.Model.GetCenterPosition());
                    }else{
                        p2 = colliderDataModel.Model.Collider.ClosestPoint(p1);//colliderDataModel.Model.GetCenterPosition());
                    }

                    Vector3 centerPosition = (p1 + p2)*0.5f;
                    if(useKDTree){
                        originalPosCloseToCenter = original.GetMeshKDTree().ClosestPointOnSurface(centerPosition);
                        colliderPosCloseToCenter = colliderDataModel.Model.GetMeshKDTree().ClosestPointOnSurface(centerPosition);
                    }else{
                        originalPosCloseToCenter = original.Collider.ClosestPoint(centerPosition);
                        colliderPosCloseToCenter = colliderDataModel.Model.Collider.ClosestPoint(centerPosition);
                    }

                    float distanceBetweenClosestPoint = Vector3.Distance(originalPosCloseToCenter, colliderPosCloseToCenter);
                    //Debug.Log("Raycast "+colliderDataModel.Model.gameObject.activeInHierarchy +" - "+original.gameObject.activeInHierarchy);
                    
                    //if(lastClosestDistance!= -1){
                        bool isNeighbour = false;// distanceBetweenClosestPoint <= lastClosestDistance;
                        RaycastHit raycast;
                        LayerMask mask;
                    /* if(!string.IsNullOrEmpty(layerMask)){
                            mask = LayerMask.NameToLayer(layerMask);
                        }else{*/
                            mask = ~0;
                        //}
                        
                        //Debug.DrawLine(originalPosCloseToCenter, (colliderPosCloseToCenter-originalPosCloseToCenter).normalized * radiusNeighbours, Color.red, Mathf.Infinity);
                        if(Physics.Raycast(originalPosCloseToCenter, colliderPosCloseToCenter-originalPosCloseToCenter, out raycast, radiusNeighbours, mask)){
                            if(raycast.collider.gameObject == colliderDataModel.Model.gameObject)
                                isNeighbour = true;
                        }
                        
                        if(Physics.Raycast(colliderPosCloseToCenter, originalPosCloseToCenter-colliderPosCloseToCenter, out raycast, radiusNeighbours, mask)){
                            if(raycast.collider.gameObject == original.gameObject)
                                isNeighbour = true;
                        }

                        /*if(colliderDataModel.Name.Equals("Rib7"))
                            Debug.DrawLine(originalPosCloseToCenter, colliderPosCloseToCenter, isNeighbour?Color.green:Color.red, Mathf.Infinity);
                        */

                        if(isNeighbour){
                            connectedModels.Add(colliderDataModel);

                        }
                    /*}else{
                        if(distanceBetweenClosestPoint < lastClosestDistance){
                            lastClosestDistance = distanceBetweenClosestPoint;
                        }
                    }*/

                }
            }

            //return -1;//lastClosestDistance;
        }

        public static void SetAllModelsCollider(bool enable, List<ZAnatomy.DataList.DataModel> fullList ){
            foreach (ZAnatomy.DataList.DataModel item in fullList)
            {
                item.Model.SetCollider(enable);
            }
        }
        public static ZAnatomy.ZAnatomyController.OrganType GetOrganTypeFromPart(string BodyPart){
            string[] PieceTypeNames = System.Enum.GetNames (typeof(ZAnatomy.ZAnatomyController.OrganType));
            for(int i = 0; i < PieceTypeNames.Length; i++){
                if(BodyPart.ToLower().Contains(PieceTypeNames[i].ToLower())){
                    return (ZAnatomy.ZAnatomyController.OrganType)i;
                }
            }

            return ZAnatomy.ZAnatomyController.OrganType.NotRecognized;
        }

        public static string GetIDModelFromPath(string Path){
            string[] splittedPath = Path.Split("/");
            return splittedPath[splittedPath.Length-1];
        }

        public static string GetAssetPathFromFullPath(string path){
            return path.Substring(path.IndexOf("Assets"));
        }

    }
}