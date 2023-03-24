using System.Collections.Generic;
using UnityEngine;

namespace pointcloud_nreal
{
    public class TestDataImporter : MonoBehaviour
    {
        [SerializeField] GameObject pointCloud;

        void Start()
        {
            var importer = new ImporterPLY();
            //List<Vector3> vertices = importer.GetVerticesOfPointcloud("./Assets/PLYFiles/Meeting_room_4010_LiDAR_binary.ply");              //**Windows path
            //List<Color32> colors = importer.GetColorsOfPointcloud("./Assets/PLYFiles/Meeting_room_4010_LiDAR_binary.ply");                  //**Windows path
            //Mesh mesh = importer.ImportAsMesh("./Assets/PLYFiles/Meeting_room_4010_LiDAR_binary.ply", vertices, colors);                    //**Windows path
            List<Vector3> vertices = importer.GetVerticesOfPointcloud("/storage/emulated/0/Nreal/Meeting_room_4010_LiDAR_binary.ply");     //**Nral path
            List<Color32> colors = importer.GetColorsOfPointcloud("/storage/emulated/0/Nreal/Meeting_room_4010_LiDAR_binary.ply");         //**Nral path   
            Mesh mesh = importer.ImportAsMesh("/storage/emulated/0/Nreal/Meeting_room_4010_LiDAR_binary.ply", vertices, colors);           //**Nral path
            var meshfilter = pointCloud.GetComponent<MeshFilter>();
            meshfilter.mesh = mesh;
        }
    }
}