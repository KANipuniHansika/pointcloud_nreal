using NRKernal;
using NRKernal.NRExamples;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using plyData;
using UnityEngine.Rendering;
using System;
using System.Linq;

namespace pointcloud_nreal
{
    public class HandDetector : MonoBehaviour
    {
        [SerializeField] GameObject cubeRed;
        [SerializeField] GameObject cubeYellow;
        [SerializeField] GameObject pointCloud;
        [SerializeField] GameObject newPoints;

        public HandModelsManager handModelsManager;
        public HandEnum handEnum;

        StreamWriter writer;
        HandsManager handsManager;
        HandState rightHandState;
        HandState leftHandState;
        HandGesture currentRightGesture;
        HandState handState;

        Vector3 cubeRedInWorldPosition = new Vector3(0.9f, 0f, 5f);
        Vector3 cubeYellowInWorldPosition = new Vector3(-0.9f, 0f, 5f);
        Vector3 LeftHandPosition = new Vector3(-0.9f, 0f, 4f);
        Vector3 RightHandPosition = new Vector3(0.9f, 0f, 4f);
        Quaternion LeftHandRotation = new Quaternion(0f, 0f, 0f, 0f);
        Quaternion RightHandRotation = new Quaternion(0f, 0f, 0f, 0f);
        Pose LeftHandPose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));
        Pose RightHandPose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));

        private StreamWriter writerErrorLog;
        private StreamWriter verticesWriter;
        public PointCloud pointCloudCreated;
        List<Vector3> verticesFound;
        KDTree kdTree;
        int i;

        String strError = "";

        // Start is called before the first frame update
        void Start()
        {
            Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Test.ply", verticesFound);     //**Nral path            
            var meshfilter = pointCloud.GetComponent<MeshFilter>();
            meshfilter.mesh = mesh;

            //pointCloudCreated = createPointCloud("./Assets/PLYFiles/Test.ply");
            pointCloudCreated = createPointCloud("/storage/emulated/0/Nreal/Test.ply"); //**Nral path
            kdTree = new KDTree();
            kdTree.Build(pointCloudCreated);

            StartHandTracking();

            string pathTest = "/storage/emulated/0/Nreal/testProcessImage.txt";
            writer = new StreamWriter(pathTest, true);

            string pathErrorLog = "/storage/emulated/0/Nreal/errorLog.txt";
            writerErrorLog = new StreamWriter(pathErrorLog, true);

            handsManager = new HandsManager();
            //rightHandState = new HandState(HandEnum.RightHand);
            //leftHandState = new HandState(HandEnum.LeftHand);
            rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
            leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
            handState = NRInput.Hands.GetHandState(handEnum);
        }

        // Update is called once per frame
        void Update()
        {
            currentRightGesture = handState.currentGesture;
            if (currentRightGesture == HandGesture.Victory)
            {
                string str = "";
                RightHandPosition = rightHandState.GetJointPose(HandJointID.IndexTip).position;
                LeftHandPosition = leftHandState.GetJointPose(HandJointID.IndexTip).position;

                cubeYellowInWorldPosition = new Vector3(RightHandPosition.x, RightHandPosition.y, RightHandPosition.z);
                cubeYellow.transform.position = new Vector3(RightHandPosition.x, RightHandPosition.y, RightHandPosition.z);

                cubeRedInWorldPosition = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z);
                //cubeRed.transform.position = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z);

                str = str + "\n" + "******Data********" + "\n";

                Vector3 queryPoint = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z);
                float radius = 0.005f;
                verticesFound = kdTree.Search(queryPoint, radius);
                str = str + "verticesFound: " + verticesFound.ToString() + "\n";

                Mesh mesh = createMesh(verticesFound);
                var meshfilter = newPoints.GetComponent<MeshFilter>();
                meshfilter.mesh = mesh;
                //Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Test.ply", verticesFound);     //**Nral path            
                //var meshfilter = pointCloud.GetComponent<MeshFilter>();
                //meshfilter.mesh = mesh;

                str = str + "cubeRedPosition: " + cubeRedInWorldPosition + "\n";
                str = str + "localPositionRed: " + cubeRedInWorldPosition.x.ToString() + "," + cubeRedInWorldPosition.y.ToString() + "," + cubeRedInWorldPosition.z.ToString() + "\n";

                str = str + "cubeYellowPosition: " + cubeYellowInWorldPosition + "\n";
                str = str + "localPositionYellow: " + cubeYellowInWorldPosition.x.ToString() + "," + cubeYellowInWorldPosition.y.ToString() + "," + cubeYellowInWorldPosition.z.ToString() + "\n";

                writer.WriteLine(str);
                writer.Close();
            }

        }

        public void StartHandTracking()
        {
            Debug.Log("HandTrackingExample: StartHandTracking");
            NRInput.SetInputSource(InputSourceEnum.Hands);
        }

        public void StopHandTracking()
        {
            Debug.Log("HandTrackingExample: StopHandTracking");
            NRInput.SetInputSource(InputSourceEnum.Controller);
        }

        public void SwitchHandVisual()
        {
            Debug.Log("HandTrackingExample: SwitchHandVisual");
            handModelsManager.ToggleHandModelsGroup();
        }

        public void ResetItems()
        {
            Debug.LogWarning("HandTrackingExample: ResetItems");

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
                var newColours = createColoursData(body.vertices, body.colors, verticesFound);
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
                strError = strError + "Failed importing Mesh " + path + ". " + e.Message;
                writerErrorLog.WriteLine(strError);
                writerErrorLog.Close();
                return null;
            }
        }

        public PointCloud createPointCloud(string path)
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
                strError = strError + "Failed importing Pointcloud " + path + ". " + e.Message;
                writerErrorLog.WriteLine(strError);
                writerErrorLog.Close();
                //Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }

        public Mesh createMesh(List<Vector3> verticesFound)
        {
            try
            {

                Mesh mesh = new Mesh();
                mesh.name = "NewPoints";
                int vertexcount = verticesFound.Count();

                mesh.indexFormat = vertexcount > 65535 ?
                        IndexFormat.UInt32 : IndexFormat.UInt16;

                mesh.SetVertices(verticesFound);
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
                strError = strError + "Failed creating Mesh" + e.Message;
                writerErrorLog.WriteLine(strError);
                writerErrorLog.Close();
                //Debug.LogError("Failed importing " + e.Message);
                return null;
            }
        }

        public List<Color32> createNewColoursData(List<Vector3> verticesFound)
        {
            try
            {

                int vertexCount = verticesFound.Count();
                List<Color32> colorsData = new List<Color32>(vertexCount);
                //Debug.Log("Vertices  #################### " + vertexCount);

                foreach (var var in verticesFound)
                {
                    Color32 colourTemp = new Color32(255, 255, 255, 255);
                    colorsData.Add(colourTemp);
                }
                return colorsData;

            }
            catch (Exception e)
            {
                strError = strError + "Failed creating Clours Data " + e.Message;
                writerErrorLog.WriteLine(strError);
                writerErrorLog.Close();
                //Debug.LogError("Failed importing " + e.Message);
                return null;
            }
        }

        public List<Color32> createColoursData(List<Vector3> verticesData, List<Color32> colors, List<Vector3> verticesFound)
        {
            try
            {

                //verticesWriter = new StreamWriter("./Assets/PLYFiles/ColoursWriter.txt", true);   //**Windows path
                //verticesWriter = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter.txt");     //**Nral path
                //verticesWriter2 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter4.txt");

                byte r = 0;
                byte g = 0;
                byte b = 0;
                byte a = 0;
                i = 0;

                int vertexCount = verticesData.Count();
                List<Color32> colorsDataTemp = new List<Color32>(vertexCount);
                //Debug.Log("Vertices  #################### " + vertexCount);

                foreach (var var in verticesData)
                {
                    Color32 colourTemp = new Color32(r, g, b, a);
                    colourTemp = colors.ElementAt(i);
                    if (verticesFound != null)
                    {
                        foreach (var x in verticesFound)
                        {
                            if (x == var)
                            {
                                colourTemp = new Color32(255, 255, 255, 255);
                                //verticesWriter.WriteLine("X " + x.ToString() + "Var " + var.ToString());
                            }
                        }
                    }
                    colorsDataTemp.Add(colourTemp);
                    i = i + 1;
                }
                //verticesWriter1.WriteLine(str1);
                //verticesWriter.Flush();
                //verticesWriter.Close();

                colors = colorsDataTemp;
                return colors;
            }
            catch (Exception e)
            {
                strError = strError + "Failed creating Clours Data " + e.Message;
                writerErrorLog.WriteLine(strError);
                writerErrorLog.Close();
                //Debug.LogError("Failed importing " + e.Message);
                return null;
            }
            
        }




    }
}

    
