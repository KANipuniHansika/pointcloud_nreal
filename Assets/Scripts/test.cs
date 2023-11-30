using Microsoft.MixedReality.Toolkit.UI;
using NRKernal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit;
using plyData;
using UnityEngine.Rendering;
using System.Linq;
using TMPro;

public class test : MonoBehaviour
{
    [SerializeField] GameObject pointCloud;
    public Text extraInfo;
    public Text mainInfo;
    public Text mainInfo2;
    public Material textMat;
    public TMP_FontAsset textFont;

    public HandEnum handEnum;

    private String strExtraInfo = "";
    private String strMainInfo = "";
    private String strMainInfo2 = "";

    private string pathTest;
    //private BoundingBox bbox;
    private string strError;
    private HandGesture currentRightGesture;
    private HandState handState;

    private Vector3 objeInWorldPosition = new Vector3(0f, 0f, 0f);
    private Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);

    private bool createObj = true;
    private bool selectPoints = true;

    private int number = 0;
    private List<GameObject> objectArray;

    private List<Tuple<Vector3, Color32>> pointsDataAferSearch;
    private PointCloudWithIndex pointCloudCreatedWithIndex;
    private KDTree kdTree;
    private int k;

    private List<Vector3> verticesDataAferSearch;
    private List<Color32> colorsDataAferSearch;
    private List<int> indexDataAferSearch;
    private List<Color32> newColours;
    private List<Vector3> newVerticesData;

    private Vector3[] boundsCornersEdited = new Vector3[8];

    //Start is called before the first frame update
    void Start()
    {
        //pointCloudCreatedWithIndex = createPointCloudWithIndex("./Assets/PLYFiles/Test.ply");   //**Windows path
        pointCloudCreatedWithIndex = createPointCloudWithIndex("./Assets/PLYFiles/Table.ply");    //**Windows path
        //pointCloudCreatedWithIndex = createPointCloudWithIndex("/storage/emulated/0/Nreal/Test.ply");   //**Nral path
        //pointCloudCreatedWithIndex = createPointCloudWithIndex("/storage/emulated/0/Nreal/Table.ply");  //**Nral path

        kdTree = new KDTree();
        kdTree.BuildPointCloudWithIndex(pointCloudCreatedWithIndex);

        //Mesh mesh = ImportAsMeshAtStart("./Assets/PLYFiles/Test.ply");   //**Windows path
        Mesh mesh = ImportAsMeshAtStart("./Assets/PLYFiles/Table.ply");   //**Windows path
        //Mesh mesh = ImportAsMeshAtStart("/storage/emulated/0/Nreal/Test.ply");     //**Nral path
        //Mesh mesh = ImportAsMeshAtStart("/storage/emulated/0/Nreal/Table.ply");     //**Nral path
        var meshfilter = pointCloud.GetComponent<MeshFilter>();
        meshfilter.mesh = mesh;

        objectArray = new List<GameObject>();
        //pathTest = "/storage/emulated/0/Nreal/testProcessImage.txt"; //Nreal
        //pathTest = "./Assets/PLYFiles/ex.fbx";
        handState = NRInput.Hands.GetHandState(handEnum);
        
    }

    // Update is called once per frame
    void Update()
    {
        currentRightGesture = handState.currentGesture;

        if (objectArray != null)
        {
            strMainInfo = "";
            foreach (var x in objectArray)
            {
                strMainInfo += x.name.ToString() + " ";
            }
            //Debug.Log("Bounding box: " + objectArray.ToString());
            mainInfo.text = "BoundingBox List: " + strMainInfo;

            strExtraInfo = "";
            foreach (var x in objectArray)
            {
                if (x.GetComponent<BoundsControl>().currentPointer != null)
                {
                    BoundsControl boundsControlX = x.GetComponent<BoundsControl>();
                    strExtraInfo += "Name: " + x.name.ToString() + "\n";
                    strExtraInfo += "X0: " + boundsControlX.boundsCorners[0].x.ToString() + " Y0: " + boundsControlX.boundsCorners[0].y.ToString() + " Z0: " + boundsControlX.boundsCorners[0].z.ToString() + "\n";
                    strExtraInfo += "X1: " + boundsControlX.boundsCorners[1].x.ToString() + " Y1: " + boundsControlX.boundsCorners[1].y.ToString() + " Z1: " + boundsControlX.boundsCorners[1].z.ToString() + "\n";
                    strExtraInfo += "X2: " + boundsControlX.boundsCorners[2].x.ToString() + " Y2: " + boundsControlX.boundsCorners[2].y.ToString() + " Z2: " + boundsControlX.boundsCorners[2].z.ToString() + "\n";
                    strExtraInfo += "X3: " + boundsControlX.boundsCorners[3].x.ToString() + " Y3: " + boundsControlX.boundsCorners[3].y.ToString() + " Z3: " + boundsControlX.boundsCorners[3].z.ToString() + "\n";
                    strExtraInfo += "X4: " + boundsControlX.boundsCorners[4].x.ToString() + " Y4: " + boundsControlX.boundsCorners[4].y.ToString() + " Z4: " + boundsControlX.boundsCorners[4].z.ToString() + "\n";
                    strExtraInfo += "X5: " + boundsControlX.boundsCorners[5].x.ToString() + " Y5: " + boundsControlX.boundsCorners[5].y.ToString() + " Z5: " + boundsControlX.boundsCorners[5].z.ToString() + "\n";
                    strExtraInfo += "X6: " + boundsControlX.boundsCorners[6].x.ToString() + " Y6: " + boundsControlX.boundsCorners[6].y.ToString() + " Z6: " + boundsControlX.boundsCorners[6].z.ToString() + "\n";
                    strExtraInfo += "X7: " + boundsControlX.boundsCorners[7].x.ToString() + " Y7: " + boundsControlX.boundsCorners[7].y.ToString() + " Z7: " + boundsControlX.boundsCorners[7].z.ToString() + "\n";
                    //str1 += "Xval: " + x.transform.position.x.ToString() + " ";
                    //str1 += "Xsize: " + x.transform.localScale.x.ToString() + " ";
                }

                if (x.GetComponent<CursorContextObjectManipulator>().contextInfo.CurrentCursorAction == CursorContextInfo.CursorAction.Move)
                {
                    Vector3[] boxVerticesFromBoundBox = x.GetComponent<BoundsControl>().boundsCorners;
                    Vector3 transform = x.GetComponent<CursorContextObjectManipulator>().contextInfo.ObjectCenter.position;
                    createCuboidBoundsMoved(boxVerticesFromBoundBox, transform);
                    x.GetComponent<BoundsControl>().boundsCorners = boundsCornersEdited;
                }
            }
            extraInfo.text = "ActiveBox: " + strExtraInfo;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        //if (currentRightGesture == HandGesture.Point)
        {
            if (selectPoints == true)
            {
                strMainInfo2 = "";
                if (objectArray != null)
                {
                    foreach (var x in objectArray)
                    {
                        Vector3[] boxVerticesFromBoundBox = x.GetComponent<BoundsControl>().boundsCorners;
                        (Vector3 minPoint, Vector3 maxPoint) = GetCuboidBounds(boxVerticesFromBoundBox);
                        kdTree.SearchCuboidAreaWithIndex(minPoint, maxPoint, out verticesDataAferSearch, out indexDataAferSearch);
                        newColours = createColoursData(newVerticesData, newColours);
                    }

                    //Mesh mesh2 = ImportAsMesh("./Assets/PLYFiles/Test.ply");   //**Windows path
                    Mesh mesh2 = ImportAsMesh("./Assets/PLYFiles/Table.ply");     //**Windows path
                    //Mesh mesh2 = ImportAsMesh("/storage/emulated/0/Nreal/Test.ply");   //**Nral path
                    //Mesh mesh2 = ImportAsMesh("/storage/emulated/0/Nreal/Table.ply");   //**Nral path

                    var meshfilter = pointCloud.GetComponent<MeshFilter>();
                    meshfilter.mesh = mesh2;
                }
                mainInfo2.text = "KDTree: " + "\n" + strMainInfo2;
                //selectPoints = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.Z))
        //if (currentRightGesture == HandGesture.Victory)
        {
            if (createObj == true)
            {
                String name = "obj" + number.ToString();
                CreateBoundingBoxMesh(name);
                number = number + 1;
                //createObj = false;
            }  
        }

        if (currentRightGesture == HandGesture.OpenHand)
        {
            createObj = true;
            selectPoints = true;
        }
    }

    private void CreateBoundingBoxMesh(String name)
    {
        try
        {
            //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject obj = new GameObject(name);
            objectArray.Add(obj);

            Pose headpose = NRFrame.HeadPose;
            objeInWorldPosition = headpose.position;           
            obj.transform.localScale = scale;
            obj.transform.position = new Vector3(objeInWorldPosition.x, objeInWorldPosition.y, (objeInWorldPosition.z + 1f));
            
            //Add Components
            //obj.AddComponent<Rigidbody>();
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<BoxCollider>();
            obj.AddComponent<MeshRenderer>();

            obj.AddComponent<TextMeshPro>();
            obj.GetComponent<TextMeshPro>().autoSizeTextContainer = true;
            obj.GetComponent<TextMeshPro>().font = textFont;
            obj.GetComponent<TextMeshPro>().renderer.material = textMat;
            obj.GetComponent<TextMeshPro>().fontMaterial = textMat;
            obj.GetComponent<TextMeshPro>().fontSize = 1;
            obj.GetComponent<TextMeshPro>().text = name;

            obj.AddComponent<BoundsControl>();
            obj.AddComponent<CursorContextObjectManipulator>();
            obj.AddComponent<RotationAxisConstraint>();
            obj.AddComponent<ObjectManipulator>();
            obj.AddComponent<NearInteractionGrabbable>();
            obj.AddComponent<MinMaxScaleConstraint>();
            //obj.AddComponent<PhysicalPressEventRouter>();

            //bbox = obj.AddComponent<BoundingBox>();
            //bbox.ScaleHandleSize = 0.1f;
            //bbox.ShowRotationHandleForX = true;
            //bbox.ShowRotationHandleForY = true;
            //bbox.ShowRotationHandleForZ = true;
            //bbox.ScaleHandleSize = 0.016f;
            //bbox.RotationHandleSize = 0.016f;

        }
        catch (Exception e)
        {
            //writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed CreateBoundingBoxMesh :" + e.Message;
            //writerErrorLog.WriteLine(strError);
            //writerErrorLog.Close();
            return;
        }
    }



    private void createCuboidBoundsMoved(Vector3[] corners, Vector3 transform)
    {
        Vector3 centerBoundsControl = GetCuboidCenter(corners);
        Vector3 movementVector = transform - centerBoundsControl;
        Vector3[] cornersTemp = new Vector3[8];

        for (int j = 0; j < corners.Length; j++)
        {
            cornersTemp[j] = corners[j] + movementVector;
        }

        boundsCornersEdited = cornersTemp;
    }


    private Vector3 GetCuboidCenter(Vector3[] corners)
    {
        Vector3 centerBoundsControl = new Vector3(0f, 0f, 0f);
        Vector3 centerBoundsControlTemp = new Vector3(0f, 0f, 0f);

        foreach (var corner in corners)
        {
            centerBoundsControlTemp += corner;
        }

        centerBoundsControl = centerBoundsControlTemp / 8;

        return centerBoundsControl;
    }

    private (Vector3 minPoint, Vector3 maxPoint) GetCuboidBounds(Vector3[] corners)
    {
        Vector3 minPoint = new Vector3(corners[0].x, corners[0].y, corners[0].z);
        Vector3 maxPoint = new Vector3(corners[0].x, corners[0].y, corners[0].z);

        foreach (var corner in corners)
        {
            if (corner.x < minPoint.x && corner.y < minPoint.y && corner.z < minPoint.z)
            {
                minPoint = corner;
            }

            if (corner.x > maxPoint.x && corner.y > maxPoint.y && corner.z > maxPoint.z)
            {
                maxPoint = corner;
            }

            //min = Vector3.Min(min, corner);
            //max = Vector3.Max(max, corner);
        }

        return (minPoint, maxPoint);
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

    public Mesh ImportAsMeshAtStart(string path)
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

            newVerticesData = body.vertices;
            mesh.SetVertices(newVerticesData);
            newColours = body.colors;
            mesh.SetColors(newColours);

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
            mesh.SetVertices(newVerticesData);
            mesh.SetColors(newColours);
            newColours = body.colors;

            /*
            if (indexDataAferSearch != null)
            {
                int vertexCount = verticesDataAferSearch.Count();
                Debug.Log("108134 verticesDataAferSearch Vertices Found at mesh ########### " + vertexCount);
                strMainInfo2 += "108134 verticesDataAferSearch Vertices Found at mesh: " + vertexCount + "\n";    //StrMainInfo2 Print
                //var newColours = createColoursData(body.vertices, body.colors, indexDataAferSearch);
                newColours = createColoursData(newVerticesData, newColours);
                mesh.SetColors(newColours);
                newColours = body.colors;
            }
            else
            {
                mesh.SetColors(newColours);
                newColours = body.colors;
            }
            */

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

    public List<Color32> createColoursData(List<Vector3> verticesData, List<Color32> colors)
    {
        //verticesWriter = new StreamWriter("./Assets/PLYFiles/verticesWriter.txt", true);   //**Windows path
        //coloursWriter = new StreamWriter("./Assets/PLYFiles/ColoursWriter.txt", true);   //**Windows path
        //verticesWriter1 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter3.txt");     //**Nral path
        //verticesWriter2 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter4.txt");

        //byte r = 0;
        //byte g = 0;
        //byte b = 0;
        //byte a = 0;
        k = 0;

        int vertexCount = verticesData.Count();
        int colorsCount = colors.Count();
        //int vertexFoundCount = verticesFound.Count();
        int indexDataAferSearchCount = indexDataAferSearch.Count();
        //List<Color32> colorsDataTemp = new List<Color32>(colorsCount);
        List<Color32> colorsFinal = new List<Color32>(colorsCount);
        Debug.Log("108134 Total Vertices  ########## " + vertexCount);
        strMainInfo2 += "108134 Total Vertices: " + vertexCount + "\n";    //StrMainInfo2 Print
        Debug.Log("108134 indexDataAferSearch Vertices Found  ######### " + indexDataAferSearchCount);
        strMainInfo2 += "108134 indexDataAferSearch Vertices Found: " + indexDataAferSearchCount + "\n";    //StrMainInfo2 Print

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
