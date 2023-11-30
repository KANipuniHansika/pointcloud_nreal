using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using plyData;
using NRKernal;
using OpenCvSharp;
using UnityEngine.UI;
using System;
using System.IO;
using OpenCvSharp.Aruco;
using System.Linq;
using UnityEngine.Rendering;

public class SelectPoints : MonoBehaviour
{
    [SerializeField] GameObject pointCloud;
    [SerializeField] GameObject newPoints;

    private StreamWriter verticesWriter;
    public PointCloud pointCloudCreated;
    List<Vector3> verticesFound;
    int i;

    // Start is called before the first frame update
    void Start()
    {
        pointCloudCreated = createPointCloud("./Assets/PLYFiles/Test.ply");

        KDTree kdTree = new KDTree();
        kdTree.Build(pointCloudCreated);

        Vector3 queryPoint = new Vector3(-0.181285f, -0.572000f, -0.022453f);
        float radius = 0.05f;
        verticesFound = kdTree.Search(queryPoint, radius);

        //Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Test.ply");     //**Nral path
        Mesh mesh = ImportAsMesh("./Assets/PLYFiles/Test.ply", verticesFound);   //**Windows path
        //Mesh mesh = createMesh(verticesFound);
        //Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Meeting_room_4010_LiDAR_binary.ply");
        var meshfilter = pointCloud.GetComponent<MeshFilter>();
        meshfilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        Mesh mesh = createMesh(verticesFound);
        //Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Meeting_room_4010_LiDAR_binary.ply");
        var meshfilter = newPoints.GetComponent<MeshFilter>();
        meshfilter.mesh = mesh;
    }

    public PointCloud createPointCloud (string path)
    {
        try
        {
            var importer = new ImporterPLY();
            var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var header = importer.ReadDataHeader(new StreamReader(stream));
            var body = importer.ReadDataBody(header, new BinaryReader(stream));

            PointCloud pointCloud = new PointCloud();

            List<Vector3> verticesData = body.vertices;
            List<Color32> colorsData = body.colors;

            for (int j = 0; j < verticesData.Count; j++)
            //foreach (var var in verticesData)
            {
                pointCloud.AddPoint(verticesData[j], colorsData[j]);
                //verticesWriter.WriteLine(point4f.ToString());
            }
            return pointCloud;

        }
        catch (Exception e)
        {
            Debug.LogError("Failed importing " + path + ". " + e.Message);
            return null;
        }
    }

    public Mesh createMesh(List<Vector3> verticesFound)
    {
        try
        {

            Mesh mesh = new Mesh();
            mesh.name = "New";
            int vertexcount = verticesFound.Count();

            mesh.indexFormat = vertexcount > 65535 ?
                    IndexFormat.UInt32 : IndexFormat.UInt16;

            mesh.SetVertices(verticesFound);
            //var newColours = createColoursData(body.vertices, body.colors, verticesFound);
            var newColours = createNewColoursData(verticesFound);
            mesh.SetColors(newColours);

            mesh.SetIndices(
                Enumerable.Range(0, vertexcount).ToArray(),
                MeshTopology.Points, 0
            );

            mesh.UploadMeshData(true);
            return mesh;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed importing " + e.Message);
            return null;
        }
    }

    public Mesh ImportAsMesh(string path, List<Vector3> verticesFound)
    {
        try
        {
            var importer = new ImporterPLY();
            var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var header = importer.ReadDataHeader(new StreamReader(stream));
            var body = importer.ReadDataBody(header, new BinaryReader(stream));

            var mesh = new Mesh();
            mesh.name = Path.GetFileNameWithoutExtension(path);

            mesh.indexFormat = header.vertexCount > 65535 ?
                    IndexFormat.UInt32 : IndexFormat.UInt16;

            //var newVertices = createVerticesData(body.vertices);
            mesh.SetVertices(body.vertices);
            //var newColours = createColoursData(body.vertices, body.colors, verticesFound);
            mesh.SetColors(body.colors);

            mesh.SetIndices(
                Enumerable.Range(0, header.vertexCount).ToArray(),
                MeshTopology.Points, 0
            );
            mesh.UploadMeshData(true);
            return mesh;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed importing " + path + ". " + e.Message);
            return null;
        }
    }

    public List<Vector3> createVerticesData(List<Vector3> verticesData)
    {
        verticesWriter = new StreamWriter("./Assets/PLYFiles/verticesWriter.txt", true);   //**Windows path
        //verticesWriter1 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter3.txt");     //**Nral path
        //verticesWriter2 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter4.txt");

        int vertexCount = verticesData.Count();
        List<Vector3> verticesDataTemp = new List<Vector3>(vertexCount);
        Debug.Log("Vertices  #################### " + vertexCount);

        //String str1 = "";
        //String str2 = "";

        //foreach (var var in verticesData)
        //{
        //str1 = str1 + "\n" + "**************" + "\n";
        //str1 = str1 + "Vertex: " + var.ToString() + "\n";
        //str1 = str1 + "localPosition: " + var.x.ToString() + "," + var.y.ToString() + "," + var.z.ToString() + "\n";

        //}

        //for (int n = 0; n < 5; n++)
        foreach (var var in verticesData)
        {
            Vector3 vertexTemp = new Vector3(0f, 0f, 0f);
            //vertexTemp = matrixFinal.MultiplyPoint(var);
            vertexTemp = var;
            verticesDataTemp.Add(vertexTemp);
            //verticesWriter.WriteLine(point4f.ToString());
        }
        //verticesWriter1.WriteLine(str1);
        //verticesWriter1.Flush();
        //verticesWriter1.Close();

        verticesData = verticesDataTemp;
        foreach (var var in verticesData)
        {
            //str2 = str2 + "\n" + "**************" + "\n";
            //str2 = str2 + "Vertex: " + var.ToString() + "\n";
            //str2 = str2 + "localPosition: " + var.x.ToString() + "," + var.y.ToString() + "," + var.z.ToString() + "\n";
            //Debug.Log(x.ToString());
            verticesWriter.WriteLine(var.ToString());
        }

        //verticesWriter2.WriteLine(str2);
        verticesWriter.Flush();
        verticesWriter.Close();

        return verticesData;
    }

    public List<Color32> createNewColoursData(List<Vector3> verticesFound)
    {
        

        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 0;


        int vertexCount = verticesFound.Count();
        List<Color32> colorsDataTemp = new List<Color32>(vertexCount);
        Debug.Log("Vertices  #################### " + vertexCount);

        foreach (var var in verticesFound)
        {
            Color32 colourTemp = new Color32(r, g, b, a);
            colourTemp = new Color32(255, 255, 255, 255);    
            colorsDataTemp.Add(colourTemp);

        }
        return colorsDataTemp;
    }

    public List<Color32> createColoursData(List<Vector3> verticesData, List<Color32> colors, List<Vector3> verticesFound)
    {
        verticesWriter = new StreamWriter("./Assets/PLYFiles/ColoursWriter.txt", true);   //**Windows path
        //verticesWriter1 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter3.txt");     //**Nral path
        //verticesWriter2 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter4.txt");

        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 0;
        i = 0;

        int vertexCount = verticesData.Count();
        List<Color32> colorsDataTemp = new List<Color32>(vertexCount);
        Debug.Log("Vertices  #################### " + vertexCount);

        foreach (var var in verticesData)
        {
            Color32 colourTemp = new Color32(r, g, b, a);
            colourTemp = colors.ElementAt(i);
            foreach (var x in verticesFound)
            {
                if (x == var)
                {
                    colourTemp = new Color32(255, 255, 255, 255);
                    verticesWriter.WriteLine("X " + x.ToString() + "Var " + var.ToString());
                }
            }          
            colorsDataTemp.Add(colourTemp);
            i = i + 1;
        }
        //verticesWriter1.WriteLine(str1);
        verticesWriter.Flush();
        verticesWriter.Close();

        colors = colorsDataTemp;
        

        return colors;
    }



}
