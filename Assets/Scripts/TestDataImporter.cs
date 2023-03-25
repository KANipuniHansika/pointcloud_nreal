using System;
using System.Collections.Generic;
using UnityEngine;

namespace pointcloud_nreal
{
    public class TestDataImporter : MonoBehaviour
    {
        [SerializeField] GameObject pointCloud;

        void Start()
        {
            importAndAlignPointCloud();
        }

        public void importPointCloud()
        {
            try
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
            catch (Exception e)
            {
                Debug.LogError("Failed importing: " + e.Message);
            }
        }

        public void importAndAlignPointCloud()
        {
            try
            {
                var importer = new ImporterPLY();

                List<Vector3> vertices = importer.GetVerticesOfPointcloud("./Assets/PLYFiles/Table.ply");              //**Windows path
                List<Color32> colors = importer.GetColorsOfPointcloud("./Assets/PLYFiles/Table.ply");                  //**Windows path

                //List<Vector3> vertices = importer.GetVerticesOfPointcloud("/storage/emulated/0/Nreal/Table.ply");     //**Nral path
                //List<Color32> colors = importer.GetColorsOfPointcloud("/storage/emulated/0/Nreal/Table.ply");         //**Nral path 

                var newVertexCreator = new NewVertexCreator();
                var newVertices = newVertexCreator.createVerticesData(vertices);

                Mesh mesh = importer.ImportAsMesh("./Assets/PLYFiles/Table.ply", newVertices, colors);                    //**Windows path
                //Mesh mesh = importer.ImportAsMesh("/storage/emulated/0/Nreal/Table.ply", newVertices, colors);           //**Nral path

                var meshfilter = pointCloud.GetComponent<MeshFilter>();
                meshfilter.mesh = mesh;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing: " + e.Message);
            }
        }

        public void testVertices()
        {
            try
            {
                List<Vector3> TestVerticesData = new List<Vector3> {
                new Vector3(0.4f, -1.3f, -1.6f),
                new Vector3(0.5f, -1.3f, -1.6f),
                new Vector3(0.6f, -1.3f, -1.5f),
                new Vector3(0.7f, -1.3f, -1.5f),
                new Vector3(1.2f, -0.6f, -0.8f),
                new Vector3(0.6f, -0.9f, -0.4f),
                };
                var newVertexCreator = new NewVertexCreator();
                var newVertices = newVertexCreator.createVerticesData(TestVerticesData);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing: " + e.Message);
            }
        }

    }
}