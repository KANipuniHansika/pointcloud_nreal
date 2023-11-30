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

public class SelectCuboidPoints : MonoBehaviour
{
    public Text textString;
    [SerializeField] GameObject pointCloud;
    [SerializeField] GameObject newPoints;

    public HandEnum handEnum;

    private StreamWriter verticesWriter;
    private StreamWriter coloursWriter;
    public PointCloud pointCloudCreated;
    List<Tuple<Vector3, Color32>> pointsDataAferSearch;
    private int k;
    private PointCloudWithIndex pointCloudCreatedWithIndex;
    KDTree kdTree;

    String textReceivedFromServer = "";
    String textReceivedFromServerOld = "";

    HandState handState;
    HandGesture currentRightGesture;

    List<Vector3> verticesDataAferSearch;
    List<Color32> colorsDataAferSearch;
    List<int> indexDataAferSearch;

    public BoundBox boundbox;
    bool selectPoints = false;

    // Start is called before the first frame update
    void Start()
    {
        //pointCloudCreated = createPointCloud("./Assets/PLYFiles/Test.ply");
        //pointCloudCreated = createPointCloud("/storage/emulated/0/Nreal/Test.ply");

        //pointCloudCreatedWithIndex = createPointCloudWithIndex("./Assets/PLYFiles/Test.ply");
        //pointCloudCreatedWithIndex = createPointCloudWithIndex("./Assets/PLYFiles/Table.ply");
        //pointCloudCreatedWithIndex = createPointCloudWithIndex("/storage/emulated/0/Nreal/Test.ply");
        pointCloudCreatedWithIndex = createPointCloudWithIndex("/storage/emulated/0/Nreal/Table.ply");

        kdTree = new KDTree();

        //kdTree.Build(pointCloudCreated);
        kdTree.BuildPointCloudWithIndex(pointCloudCreatedWithIndex);


        handState = NRInput.Hands.GetHandState(handEnum);

        //Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Test.ply");     //**Nral path
        Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Table.ply");     //**Nral path
        //Mesh mesh = ImportAsMesh("./Assets/PLYFiles/Table.ply");   //**Windows path
        //Mesh mesh = ImportAsMesh("./Assets/PLYFiles/Test.ply");   //**Windows path
        //Mesh mesh = createMesh(verticesFound);
        var meshfilter = pointCloud.GetComponent<MeshFilter>();
        meshfilter.mesh = mesh;
    }

    void Update()
    {
        textReceivedFromServer = textString.text;
        textReceivedFromServer = textReceivedFromServer.Replace("Read Data: ", "");

        currentRightGesture = handState.currentGesture;

        if (textReceivedFromServer.Contains("Select") || textReceivedFromServer.Contains("select") || textReceivedFromServer.Contains("Point") || textReceivedFromServer.Contains("point"))
        //if (Input.GetKeyDown(KeyCode.Space))
        //if (NRInput.GetButtonDown(ControllerButton.HOME))
        //if (currentRightGesture == HandGesture.Point)
        {
            if (selectPoints == false)
            {
                

                //Vector3 minPoint = new Vector3(-0.5f, -1f, 0f);   //boxVertices[0]
                //Vector3 maxPoint = new Vector3(0f, -0.5f, 0.5f);   //boxVertices[6]
                Vector3[] boxVerticesFromBoundBox = boundbox.boxVertices;
                Vector3 minPoint = boxVerticesFromBoundBox[0];
                Vector3 maxPoint = boxVerticesFromBoundBox[6];

                /*
                // Calculate the minimum and maximum corners of the cuboid
                for (int i = 0; i < boxVerticesFromnoundbox.Length; i++)
                {
                    Vector3 vertex = boxVerticesFromnoundbox[i];
                    minPoint = Vector3.Min(minPoint, vertex);
                    maxPoint = Vector3.Max(maxPoint, vertex);
                }
                */

                //List<Vector3> verticesDataAferSearch = new List<Vector3>();
                //List<Color32> colorsDataAferSearch = new List<Color32>();

                //kdTree.SearchCuboidArea(minPoint, maxPoint, out verticesDataAferSearch, out colorsDataAferSearch);
                kdTree.SearchCuboidAreaWithIndex(minPoint, maxPoint, out verticesDataAferSearch, out indexDataAferSearch);

                //Mesh mesh = createMesh(verticesFound);
                //Mesh mesh = ImportAsMesh("./Assets/PLYFiles/Table.ply");     //**Windows path
                //Mesh mesh = ImportAsMesh("./Assets/PLYFiles/Test.ply");   //**Windows path
                //Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Test.ply");   //**Nral path
                Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Table.ply");   //**Nral path

                //var meshfilter = newPoints.GetComponent<MeshFilter>();
                var meshfilter = pointCloud.GetComponent<MeshFilter>();
                meshfilter.mesh = mesh;

                selectPoints = true;
            }
        }

        if (textReceivedFromServer.Contains("Stop") || textReceivedFromServer.Contains("stop"))
        {
            selectPoints = false;
        }

    }


    private PointCloud createPointCloud(string path)
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
                //pointCloud.AddPoint(verticesData[j], colorsData[j]);
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

    private PointCloudWithIndex createPointCloudWithIndex(string path)
    {
        try
        {
            var importer = new ImporterPLY();
            var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var header = importer.ReadDataHeader(new StreamReader(stream));
            var body = importer.ReadDataBody(header, new BinaryReader(stream));

            PointCloudWithIndex pointCloudWithIndex = new PointCloudWithIndex();

            List<Vector3> verticesData = body.vertices;

            for (int j = 0; j < verticesData.Count; j++)
            //foreach (var var in verticesData)
            {
                //pointCloud.AddPoint(verticesData[j], colorsData[j]);
                pointCloudWithIndex.AddPoint(verticesData[j], j);
                //verticesWriter.WriteLine(point4f.ToString());
            }
            return pointCloudWithIndex;

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

    public Mesh ImportAsMesh(string path)
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
            //mesh.SetVertices(verticesDataAferSearch);
            //mesh.SetColors(colorsDataAferSearch);
            mesh.SetVertices(body.vertices);

            if (indexDataAferSearch != null)
            {
                int vertexCount = verticesDataAferSearch.Count();
                Debug.Log("108134 Vertices Found at mesh #################### " + vertexCount);
                //var newColours = createColoursData(body.vertices, body.colors, indexDataAferSearch);
                var newColours = createColoursData(body.vertices, body.colors);
                mesh.SetColors(newColours);
            }
            else
            {
                mesh.SetColors(body.colors);
            }
            
            
            mesh.SetIndices(
                Enumerable.Range(0, header.vertexCount).ToArray(),
                MeshTopology.Points, 0
            );
            

            /*
            mesh.SetIndices(
                Enumerable.Range(0, newVertices.Count).ToArray(),
                MeshTopology.Points, 0
            );
            */

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
        //verticesWriter = new StreamWriter("./Assets/PLYFiles/verticesWriter.txt", true);   //**Windows path
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
            ///verticesWriter.WriteLine(var.ToString());
        }

        //verticesWriter2.WriteLine(str2);
        //verticesWriter.Flush();
        //verticesWriter.Close();

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
        Debug.Log("108134 Vertices  #################### " + vertexCount);

        foreach (var var in verticesFound)
        {
            Color32 colourTemp = new Color32(r, g, b, a);
            colourTemp = new Color32(255, 255, 255, 255);
            colorsDataTemp.Add(colourTemp);

        }
        return colorsDataTemp;
    }

    public List<Color32> createColoursData(List<Vector3> verticesData, List<Color32> colors)
    {
        //verticesWriter = new StreamWriter("./Assets/PLYFiles/verticesWriter.txt", true);   //**Windows path
        //coloursWriter = new StreamWriter("./Assets/PLYFiles/ColoursWriter.txt", true);   //**Windows path
        //verticesWriter1 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter3.txt");     //**Nral path
        //verticesWriter2 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter4.txt");

        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 0;
        k = 0;

        int vertexCount = verticesData.Count();
        int colorsCount = colors.Count();
        //int vertexFoundCount = verticesFound.Count();
        int indexDataAferSearchCount = indexDataAferSearch.Count();
        //List<Color32> colorsDataTemp = new List<Color32>(colorsCount);
        List<Color32> colorsFinal = new List<Color32>(colorsCount);
        Debug.Log("Vertices  #################### " + vertexCount);
        Debug.Log(" 9184 Vertices Found  #################### " + indexDataAferSearchCount);

        /*
        for (int i = 0; i < verticesData.Count; i++)
        //foreach (var var in verticesData)
        {
            Color32 colourTemp = new Color32(colors[k].r, colors[k].g, colors[k].b, colors[k].a);
            //colourTemp.r = colors[i].r;
            //colourTemp.g = colors[i].g;
            //colourTemp.b = colors[i].b;
            //colourTemp.a = colors[i].a;
            //verticesWriter.WriteLine("colorBefore: " + colourTemp.ToString());
            for (int j = 0; j < verticesFound.Count; j++)
            {
                if (verticesData[i] == verticesFound[j])
                {
                    colourTemp = new Color32(0, 255, 0, 255);
                    //verticesWriter.WriteLine("verticesData " + verticesData[i].ToString() + "verticesFound " + verticesFound[j].ToString());
                }
            }
            colorsDataTemp.Add(colourTemp);
            //verticesWriter.WriteLine("colorAfter: " + colourTemp.ToString());
            k = k + 1;
        }
        */

        List<Color32> colorsDataTemp = colors;
        Color32 colourTemp = new Color32(0, 255, 0, 255);

        for (int i = 0; i < indexDataAferSearchCount; i++)
        //foreach (var var in verticesData)
        {
            //colourTemp.r = colors[i].r;
            //colourTemp.g = colors[i].g;
            //colourTemp.b = colors[i].b;
            //colourTemp.a = colors[i].a;
            //verticesWriter.WriteLine("colorBefore: " + colourTemp.ToString());
            k = indexDataAferSearch[i];
            colorsDataTemp[k] = colourTemp;
            //verticesWriter.WriteLine("colorAfter: " + colourTemp.ToString());
            
        }


        //verticesWriter1.WriteLine(str1);
        //verticesWriter.Flush();
        //verticesWriter.Close();

        //coloursWriter.Flush();
        //coloursWriter.Close();

        colorsFinal = colorsDataTemp;


        return colorsFinal;
    }
}