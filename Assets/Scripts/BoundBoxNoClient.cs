using NRKernal;
using NRKernal.NRExamples;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BoundBoxNoClient : MonoBehaviour
{
    public Text textString;

    [SerializeField] GameObject cubeRed;
    [SerializeField] GameObject cubeYellow;
    [SerializeField] GameObject gameObjectLineSet1;
    [SerializeField] GameObject gameObjectLineSet2;


    public HandModelsManager handModelsManager;
    public HandEnum handEnum;

    StreamWriter writer;
    StreamWriter writerErrorLog;
    StreamWriter writerUpdate;
    HandsManager handsManager;
    HandState rightHandState;
    HandState leftHandState;
    HandGesture currentRightGesture;
    HandState handState;

    Vector3 cubeRedInWorldPosition = new Vector3(0.9f, 0f, 5f);
    Vector3 cubeYellowInWorldPosition = new Vector3(-0.9f, 0f, 5f);
    Vector3 LeftHandPosition = new Vector3(-0.9f, 0f, 4f);
    Vector3 RightHandPosition = new Vector3(0.9f, 0f, 4f);

    Vector3 LeftHandPositionGrab = new Vector3(-0.9f, 0f, 4f);
    Vector3 RightHandPositionGrab = new Vector3(0.9f, 0f, 4f);
    Vector3 LeftHandPositionScale = new Vector3(-0.9f, 0f, 4f);
    Vector3 RightHandPositionScale = new Vector3(0.9f, 0f, 4f);
    Vector3 LeftHandPositionRotate = new Vector3(-0.9f, 0f, 4f);
    Vector3 RightHandPositionRotate= new Vector3(0.9f, 0f, 4f);

    //public Color lineColor = new Color(0f, 1f, 0.4f, 0.74f);

    //public Vector3 minPoint;
    //public Vector3 maxPoint;

    //public Color boxColor = Color.red;
    public Color boxColor = Color.green;

    private Vector3 minPoint;
    private Vector3 maxPoint;

    private Vector3[] boxVertices;
    private int[] boxIndices;
    private Vector3[] boxVerticesToUpdate;
    private Vector3[] boxVerticesInOrder;
    private Matrix4x4[] matricesToUpdateVertices;
    private Material boxMaterial;

    private Vector3[] nearestPointToIndices;
    private float[] distanceToIndices;

    String strError = "";
    String str = "";
    String strUpdate = "";
    string pathTest;
    string pathErrorLog;
    string pathUpdate;

    int iterationGrab = 1;
    int iterationScale = 1;
    int iterationRotate = 1;
    bool grabbedItem = false;
    bool scaledItem = false;
    bool rotatedItem = false;

    String textReceivedFromServer = "";


    void Start()
    {
        // Create the bounding box mesh
        CreateBoundingBoxMesh();

        // Create and apply the box material
        boxMaterial = new Material(Shader.Find("Standard"));
        boxMaterial.color = boxColor;
        GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

        InitBoundingBox();

        StartHandTracking();

        pathTest = "/storage/emulated/0/Nreal/testProcessImage.txt"; //Nreal
        //pathTest = "./Assets/PLYFiles/testProcessImage.txt";

        pathErrorLog = "/storage/emulated/0/Nreal/errorLog.txt"; //Nreal
        //pathErrorLog = "./Assets/PLYFiles/errorLog.txt";

        pathUpdate = "/storage/emulated/0/Nreal/Update.txt";  //Nreal
        //pathUpdate = "./Assets/PLYFiles/Update.txt";

        handsManager = new HandsManager();
        //rightHandState = new HandState(HandEnum.RightHand);
        //leftHandState = new HandState(HandEnum.LeftHand);
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        handState = NRInput.Hands.GetHandState(handEnum);
    }


    void Update()
    {

        //writerUpdate = new StreamWriter(pathUpdate, true);



        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //ScaleBoundingBox();
        //}
        textReceivedFromServer = textString.text;
        textReceivedFromServer = textReceivedFromServer.Replace("Read Data: ", ""); ;

        currentRightGesture = handState.currentGesture;

        if (textReceivedFromServer.Contains("Scale") || textReceivedFromServer.Contains("scale") || textReceivedFromServer.Contains("stop"))
        {
            //boxMaterial.color = Color.black;
            //GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
             
            writerUpdate = new StreamWriter(pathUpdate, true);
            strUpdate = "***************UpdateScale**************************";
            writerUpdate.WriteLine(strUpdate);

            RightHandPosition = rightHandState.GetJointPose(HandJointID.IndexTip).position;
            LeftHandPosition = leftHandState.GetJointPose(HandJointID.IndexTip).position;
            strUpdate = "RightHandPosition: " + RightHandPosition;
            writerUpdate.WriteLine(strUpdate);
            strUpdate = "LeftHandPosition: " + LeftHandPosition;
            writerUpdate.WriteLine(strUpdate);

            cubeYellow.transform.position = new Vector3(RightHandPosition.x, RightHandPosition.y, RightHandPosition.z);

            cubeRed.transform.position = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z);

            //Vector3 point = new Vector3(0.5f, 0.5f, 3.0f);
            float minimumDistance = GetMinimumDistanceToBoundingBox(RightHandPosition);
            Debug.Log("minimumDistanceFloat: " + minimumDistance.ToString());
            strUpdate = "minimumDistanceFloat: " + minimumDistance.ToString();
            writerUpdate.WriteLine(strUpdate);

            //int minimumDistanceIndex = Array.IndexOf(distanceToIndices, minimumDistance);
            //Debug.Log("minimumDistanceIndex: " + minimumDistanceIndex.ToString());
            //strUpdate = "minimumDistanceIndex: " + minimumDistanceIndex.ToString();
            //writerUpdate.WriteLine(strUpdate);

            

            if (scaledItem != true)
            {
                if (minimumDistance < 0.2f)             //0.03
                {
                    boxMaterial.color = Color.green;
                    GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

                    //GetVerticesToUpdate(minimumDistanceIndex);

                    RightHandPositionScale = RightHandPosition;
                    scaledItem = true;
                    //Vector3 pointNew = new Vector3(1f, 2f, 3.2f);
                    //Vector3 displacementVector = pointNew - point;

                    //strUpdate = "minimumDistance: " + minimumDistance;
                    //writerUpdate.WriteLine(strUpdate);
                    //writerUpdate.Close();
                }
            }
            else
            {
                if (((iterationScale) % (2)) == 0)
                {
                    float minimumDistanceNow = GetMinimumDistanceToBoundingBox(RightHandPositionScale);
                    int minimumDistanceIndexNow = Array.IndexOf(distanceToIndices, minimumDistanceNow);

                    strUpdate = "minimumDistanceNow: " + minimumDistanceNow.ToString();
                    writerUpdate.WriteLine(strUpdate);
                    strUpdate = "minimumDistanceIndexNow: " + minimumDistanceIndexNow.ToString();
                    writerUpdate.WriteLine(strUpdate);

                    GetVerticesToUpdate(minimumDistanceIndexNow);

                    Vector3 RightHandPositionNew = rightHandState.GetJointPose(HandJointID.IndexTip).position;
                    Vector3 displacementVector = RightHandPositionNew - RightHandPositionScale;
                    RightHandPositionScale = RightHandPositionNew;

                    strUpdate = displacementVector.ToString();
                    writerUpdate.WriteLine("displacementVector: " + strUpdate);
                    writerUpdate.WriteLine("x: " + displacementVector.x.ToString() + " y: " + displacementVector.y.ToString() + " z: " + displacementVector.z.ToString());


                    // Update the bounding box positions
                    ChangeBoundingBox(displacementVector);

                    boxMaterial.color = Color.magenta;
                    GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
                    AddLines(Color.magenta);
                }

                iterationScale += 1;
            }

            writerUpdate.Close();
        }

        /*
        if (textReceivedFromServer.Contains("Done") || textReceivedFromServer.Contains("done"))
        {
            scaledItem = false;
            iterationScale = 0;
        }
        */

        if (textReceivedFromServer.Contains("Done") || textReceivedFromServer.Contains("done"))
        {
            boxMaterial.color = Color.gray;
            GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
            AddLines(Color.gray);
            grabbedItem = false;
            iterationGrab = 0;
            scaledItem = false;
            iterationScale = 0;
            rotatedItem = false;
            iterationRotate = 0;
        }

        /*
        if (currentRightGesture == HandGesture.Point)
        {
            if (grabbedItem == true)
            {
                Vector3 RightHandPositionNew = rightHandState.GetJointPose(HandJointID.IndexTip).position;
                Vector3 displacementVector = RightHandPositionNew - RightHandPosition;

                // Update the bounding box positions
                UpdateBoundingBox(displacementVector);
                boxMaterial.color = Color.blue;
                GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
                grabbedItem = false; 

                //strUpdate = "RightHandPositionNew: " + RightHandPositionNew;
                //writerUpdate.WriteLine(strUpdate);
            }
        }
        */

        if (currentRightGesture == HandGesture.Grab)
        {
            //boxMaterial.color = Color.green;
            //GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

            writerUpdate = new StreamWriter(pathUpdate, true);
            strUpdate = "***************UpdateGrab**************************";
            writerUpdate.WriteLine(strUpdate);

            RightHandPosition = rightHandState.GetJointPose(HandJointID.IndexTip).position;
            LeftHandPosition = leftHandState.GetJointPose(HandJointID.IndexTip).position;
            strUpdate = "RightHandPosition: " + RightHandPosition;
            writerUpdate.WriteLine(strUpdate);
            strUpdate = "LeftHandPosition: " + LeftHandPosition;
            writerUpdate.WriteLine(strUpdate);

            cubeYellowInWorldPosition = new Vector3(RightHandPosition.x, RightHandPosition.y, RightHandPosition.z);
            cubeYellow.transform.position = new Vector3(RightHandPosition.x, RightHandPosition.y, RightHandPosition.z);

            cubeRedInWorldPosition = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z);
            cubeRed.transform.position = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z);

            //Vector3 point = new Vector3(0.2f, 0.8f, 2.4f);
            float minimumDistance = GetMinimumDistanceToBoundingBox(RightHandPosition);

            strUpdate = "minimumDistance: " + minimumDistance;
            writerUpdate.WriteLine(strUpdate);
            writerUpdate.Close();


            if (grabbedItem != true)             //0.03
            {
                if (minimumDistance < 0.2f)             //0.03
                {
                    boxMaterial.color = Color.red;
                    GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

                    RightHandPositionGrab = RightHandPosition;
                    grabbedItem = true;

                    Debug.Log("minimumDistance: " + minimumDistance);
                    //Vector3 pointNew = new Vector3(1.2f, 0.8f, 2.4f);
                    //Vector3 displacementVector = pointNew - point;
                }
            }
            else             
            {
                if (((iterationGrab) % (2)) == 0)            
                {
                    Vector3 RightHandPositionNew = rightHandState.GetJointPose(HandJointID.IndexTip).position;
                    Vector3 displacementVector = RightHandPositionNew - RightHandPositionGrab;
                    RightHandPositionGrab = RightHandPositionNew;

                    // Update the bounding box positions
                    UpdateBoundingBox(displacementVector);
                    boxMaterial.color = Color.blue;
                    GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
                    AddLines(Color.blue);
                }

                iterationGrab += 1;
            }
        }

        if (currentRightGesture != HandGesture.Grab)
        {
            grabbedItem = false;
            iterationGrab = 0;
        }

        if (currentRightGesture == HandGesture.Victory)
        {
            //boxMaterial.color = Color.black;
            //GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

            writerUpdate = new StreamWriter(pathUpdate, true);
            strUpdate = "***************UpdateRotate**************************";
            writerUpdate.WriteLine(strUpdate);

            RightHandPosition = rightHandState.GetJointPose(HandJointID.IndexTip).position;
            LeftHandPosition = leftHandState.GetJointPose(HandJointID.IndexTip).position;
            strUpdate = "RightHandPosition: " + RightHandPosition;
            writerUpdate.WriteLine(strUpdate);
            strUpdate = "LeftHandPosition: " + LeftHandPosition;
            writerUpdate.WriteLine(strUpdate);


            //Vector3 point = new Vector3(0.5f, 0.5f, 3.0f);
            float minimumDistance = GetMinimumDistanceToBoundingBox(RightHandPosition);
            Debug.Log("minimumDistanceFloat: " + minimumDistance.ToString());
            strUpdate = "minimumDistanceFloat: " + minimumDistance.ToString();
            writerUpdate.WriteLine(strUpdate);

           

            if (rotatedItem != true)
            {
                if (minimumDistance < 0.2f)             //0.03
                {
                    boxMaterial.color = Color.red;
                    GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

                    RightHandPositionRotate = RightHandPosition;
                    rotatedItem = true;
                }
            }
            else
            {
                if (((iterationRotate) % (2)) == 0)
                {
                    Vector3 RightHandPositionNew = rightHandState.GetJointPose(HandJointID.IndexTip).position;
                    Vector3 displacementVector = RightHandPositionNew - RightHandPositionRotate;
                    
                    writerUpdate.WriteLine("displacementVector: " + displacementVector.ToString());
                    writerUpdate.WriteLine("x: " + displacementVector.x.ToString() + " y: " + displacementVector.y.ToString() + " z: " + displacementVector.z.ToString());

                    // Update the bounding box positions
                    RotateBoundingBox(displacementVector, RightHandPositionRotate);
                    RightHandPositionRotate = RightHandPositionNew;

                    boxMaterial.color = Color.green;
                    GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
                    AddLines(Color.green);
                }

                iterationRotate += 1;
            }
            writerUpdate.Close();

        }

        if (currentRightGesture != HandGesture.Victory)
        {

            rotatedItem = false;
            iterationRotate = 0;
        }

        /*
        if (grabbedItem == true)
        {
            if (currentRightGesture == HandGesture.Grab)
            {
                Vector3 RightHandPositionNew = rightHandState.GetJointPose(HandJointID.IndexTip).position;
                Vector3 displacementVector = RightHandPositionNew - RightHandPosition;

                // Update the bounding box positions
                UpdateBoundingBox(displacementVector);
                boxMaterial.color = Color.blue;
                GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
                grabbedItem = true;
                //strUpdate = "RightHandPositionNew: " + RightHandPositionNew;
                //writerUpdate.WriteLine(strUpdate);
            }
            if (currentRightGesture != HandGesture.Grab)
            {
                grabbedItem = false;
            }
        }
        */
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
        InitBoundingBox();
        grabbedItem = false;
        iterationGrab = 0;
        scaledItem = false;
        iterationScale = 0;
    }

    private void CreateBoundingBoxMesh()
    {
        try
        {
            // Create the vertices and indices for the bounding box
            boxVertices = new Vector3[8];
            nearestPointToIndices = new Vector3[12];
            distanceToIndices = new float[12];

            boxIndices = new int[]
            {
            0, 1, 1, 2, 2, 3, 3, 0, // Bottom edges
            4, 5, 5, 6, 6, 7, 7, 4, // Top edges
            0, 4, 1, 5, 2, 6, 3, 7  // Vertical edges
            };

            // Create the mesh and assign vertices and indices
            Mesh mesh = new Mesh();
            mesh.vertices = boxVertices;
            mesh.SetIndices(boxIndices, MeshTopology.Lines, 0);

            // Assign the mesh to the MeshFilter component
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed CreateBoundingBoxMesh :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return;
        }
    }

    private void InitBoundingBox()
    {
        try
        {
            minPoint = new Vector3(-0.5f, -0.5f, 3.5f);
            maxPoint = new Vector3(0.5f, 0.5f, 4.5f);

            // Update the bounding box vertices based on the min and max points
            boxVertices[0] = new Vector3(minPoint.x, minPoint.y, minPoint.z);
            boxVertices[1] = new Vector3(maxPoint.x, minPoint.y, minPoint.z);
            boxVertices[2] = new Vector3(maxPoint.x, minPoint.y, maxPoint.z);
            boxVertices[3] = new Vector3(minPoint.x, minPoint.y, maxPoint.z);
            boxVertices[4] = new Vector3(minPoint.x, maxPoint.y, minPoint.z);
            boxVertices[5] = new Vector3(maxPoint.x, maxPoint.y, minPoint.z);
            boxVertices[6] = new Vector3(maxPoint.x, maxPoint.y, maxPoint.z);
            boxVertices[7] = new Vector3(minPoint.x, maxPoint.y, maxPoint.z);

            // Update the mesh
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.vertices = boxVertices;
            mesh.RecalculateBounds();
            boxMaterial.color = Color.gray;
            GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
            AddLines(Color.gray);
            //mesh.RecalculateNormals();
        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed InitBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return;
        }
    }

    private void AddLines(Color boxColour)
    {
        try
        {
            int[] boxIndicesForLinesSet1 = new int[]
            {
            4, 0, 1, 2, 3, 7, 6, 5, 1
            };

            int[] boxIndicesLinesSet2 = new int[]
            {
            5, 4, 7, 3, 0, 1, 2, 6
            };


            LineRenderer lineSet1 = gameObjectLineSet1.GetComponent<LineRenderer>();
            lineSet1.material = new Material(Shader.Find("Sprites/Default"));

            lineSet1.positionCount = boxIndicesForLinesSet1.Length;
            lineSet1.startWidth = 0.005f;
            lineSet1.startColor = boxColour;

            for (int i = 0; i < boxIndicesForLinesSet1.Length; i++)
            {
                lineSet1.SetPosition(i, boxVertices[boxIndicesForLinesSet1[i]]);
            }
            lineSet1.endWidth = 0.005f;
            lineSet1.endColor = boxColour;
            lineSet1.useWorldSpace = true;

            LineRenderer lineSet2 = gameObjectLineSet2.GetComponent<LineRenderer>();
            lineSet2.material = new Material(Shader.Find("Sprites/Default"));

            lineSet2.positionCount = boxIndicesLinesSet2.Length;
            lineSet2.startWidth = 0.005f;
            lineSet2.startColor = boxColour;

            for (int i = 0; i < boxIndicesLinesSet2.Length; i++)
            {
                lineSet2.SetPosition(i, boxVertices[boxIndicesLinesSet2[i]]);
            }
            lineSet2.endWidth = 0.005f;
            lineSet2.endColor = boxColour;
            lineSet2.useWorldSpace = true;

            
            //LineRenderer lineBottom = gameObjectLineBottom.AddComponent<LineRenderer>();
            //lineBottom.startWidth = 0.005f;
            //lineBottom.startColor = boxMaterial.color;
            //lineBottom.SetPositions(boxVerticesForLinesBottom);
            //lineBottom.endWidth = 0.005f;
            //lineBottom.endColor = boxMaterial.color;
            //lineBottom.useWorldSpace = true;

            /*
            LineRenderer lineTop = gameObjectLineTop.AddComponent<LineRenderer>();
            lineTop.startWidth = 0.1f;
            lineTop.startColor = boxMaterial.color;
          
            lineTop.endWidth = 0.1f;
            lineTop.endColor = boxMaterial.color;
            lineTop.useWorldSpace = true;
            */

            /*
            LineRenderer lineEdge1 = gameObjectLineEdge1.AddComponent<LineRenderer>();
            lineEdge1.startWidth = 0.01f;
            lineEdge1.startColor = boxMaterial.color;
            lineEdge1.SetPositions(boxVerticesForEdge1);
            lineEdge1.endWidth = 0.01f;
            lineEdge1.endColor = boxMaterial.color;
            lineEdge1.useWorldSpace = true;

            LineRenderer lineEdge2 = gameObjectLineEdge2.AddComponent<LineRenderer>();
            lineEdge2.startWidth = 0.01f;
            lineEdge2.startColor = boxMaterial.color;
            lineEdge2.SetPositions(boxVerticesForEdge2);
            lineEdge2.endWidth = 0.01f;
            lineEdge2.endColor = boxMaterial.color;
            lineEdge2.useWorldSpace = true;

            LineRenderer lineEdge3 = gameObjectLineEdge3.AddComponent<LineRenderer>();
            lineEdge3.startWidth = 0.01f;
            lineEdge3.startColor = boxMaterial.color;
            lineEdge3.SetPositions(boxVerticesForEdge3);
            lineEdge3.endWidth = 0.01f;
            lineEdge3.endColor = boxMaterial.color;
            lineEdge3.useWorldSpace = true;

            LineRenderer lineEdge4 = gameObjectLineEdge4.AddComponent<LineRenderer>();
            lineEdge4.startWidth = 0.01f;
            lineEdge4.startColor = boxMaterial.color;
            lineEdge4.SetPositions(boxVerticesForEdge4);
            lineEdge4.endWidth = 0.01f;
            lineEdge4.endColor = boxMaterial.color;
            lineEdge4.useWorldSpace = true;
            */


        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed InitBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return;
        }

    }

    private void UpdateBoundingBox(Vector3 displacementVector)
    {
        try
        {
            for (int i = 0; i < boxVertices.Length; i++)
            {
                boxVertices[i] += displacementVector;
            }

            // Update the mesh
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.vertices = boxVertices;
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed UpdateBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return;
        }
    }

    private float GetMinimumDistanceToBoundingBox(Vector3 point)
    {
        try
        {
            writer = new StreamWriter(pathTest, true);
            str = "***************testProcessImage**************************";
            writer.WriteLine(str);
            for (int i = 0; i < boxIndices.Length; i += 2)
            {
                Vector3 lineStart = boxVertices[(boxIndices[i])];
                Vector3 lineEnd = boxVertices[(boxIndices[i + 1])];
                Vector3 nearestPoint = GetNearestPointOnLine(lineStart, lineEnd, point);

                str = "Nearest Point: " + nearestPoint;
                writer.WriteLine(str);
                Debug.Log("Nearest Point: " + nearestPoint);

                nearestPointToIndices[(i / 2)] = nearestPoint;
                float lineLengthToStart = (nearestPoint - lineStart).magnitude;
                float lineLengthToEnd = (nearestPoint - lineEnd).magnitude;
                float lineLengthBetweenStartAndEnd = (lineEnd - lineStart).magnitude;
                if ((lineLengthToStart + lineLengthToEnd) == lineLengthBetweenStartAndEnd)
                {
                    float distance = Vector3.Distance(point, nearestPoint);
                    distanceToIndices[(i / 2)] = distance;
                    str = "distance: " + distance.ToString();
                    writer.WriteLine(str);
                    Debug.Log("###########distance: " + distance.ToString());
                }
                else
                {
                    float distance = 20f;
                    distanceToIndices[(i / 2)] = distance;
                    str = "distance: " + distance.ToString();
                    str = "OutOfBoundIndex: " + (i / 2).ToString();
                    writer.WriteLine(str);
                    Debug.Log("###########distance: " + distance.ToString());
                    Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@OutOfBoundIndex: " + (i / 2).ToString());
                }

            }
            //Debug.Log("nearestPointToIndices: " + nearestPointToIndices.ToString());
            //Debug.Log("distanceToIndices: " + distanceToIndices.ToString());
            writer.Close();
            float minimumDistance = distanceToIndices.Min();
            //int minimumDistanceIndex = Array.IndexOf(distanceToIndices, minimumDistance);
            //float minimumDistanceIndexFloat = (float) minimumDistanceIndex;
            //float[] arrayMin = new float[] { minimumDistance, minimumDistanceIndex};
            return minimumDistance;
        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed GetMinimumDistanceToBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return 0f;
        }
        
    }

    private Vector3 GetNearestPointOnLine(Vector3 start, Vector3 end, Vector3 point)
    {
        try
        {
            Vector3 line = end - start;
            float lineLength = line.magnitude;
            Vector3 lineDirection = line.normalized;

            // Calculate the projection of the point onto the line
            Vector3 pointDirection = point - start;
            float dotProduct = Vector3.Dot(pointDirection, lineDirection);
            //dotProduct = Mathf.Clamp(dotProduct, 0f, lineLength);

            // Calculate the nearest point on the line
            Vector3 nearestPoint = start + lineDirection * dotProduct;
            return nearestPoint;
        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "GetNearestPointOnLine :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return new Vector3(0f, 0f, 0f);
        }
    }

    private void GetVerticesToUpdate(int minimumDistanceIndex)
    {
        try
        {
            writer = new StreamWriter(pathTest, true);
            str = "############################################GetVerticesToUpdate##############################################";
            writer.WriteLine(str);

            boxVerticesToUpdate = new Vector3[6];
            matricesToUpdateVertices = new Matrix4x4[6];

            //int minimumDistanceIndex = (int) minimumDistanceIndexFloat;
            Vector3 lineStart = boxVertices[(boxIndices[(minimumDistanceIndex*2)])];
            Vector3 lineEnd = boxVertices[(boxIndices[(minimumDistanceIndex * 2) + 1])];

            writer.WriteLine("lineStart: ");
            writer.WriteLine("x: " + lineStart.x.ToString() + " y: " + lineStart.y.ToString() + " z: " + lineStart.z.ToString());
            writer.WriteLine("lineEnd: ");
            writer.WriteLine("x: " + lineEnd.x.ToString() + " y: " + lineEnd.y.ToString() + " z: " + lineEnd.z.ToString());


            int n = 0;
            for (int i = 0; i < boxVertices.Length; i++)
            {
                Vector3 difLineStart = boxVertices[i] - lineStart;
                Vector3 difLineEnd = boxVertices[i] - lineEnd;

                writer.WriteLine("int i: " + i.ToString());
                writer.WriteLine("difLineStart: ");
                writer.WriteLine("x: " + difLineStart.x.ToString() + " y: " + difLineStart.y.ToString() + " z: " + difLineStart.z.ToString());
                writer.WriteLine("difLineEnd: ");
                writer.WriteLine("x: " + difLineEnd.x.ToString() + " y: " + difLineEnd.y.ToString() + " z: " + difLineEnd.z.ToString());

                if ((difLineStart.x == 0f || difLineStart.y == 0f || difLineStart.z == 0f) &&  (difLineEnd.x == 0f || difLineEnd.y == 0f || difLineEnd.z == 0f))
                {
                    boxVerticesToUpdate[n] = boxVertices[i];

                    writer.WriteLine("int n: " + n.ToString());
                    writer.WriteLine("boxVertices[i]: ");
                    writer.WriteLine("x: " + boxVertices[i].x.ToString() + " y: " + boxVertices[i].y.ToString() + " z: " + boxVertices[i].z.ToString());

                    //Vector3 diffrenceEnd = boxVertices[i] - lineEnd;
                    //Debug.Log("diffrenceEnd: " + diffrenceEnd.ToString());

                    //Vector3 diffrenceStart = boxVertices[i] - lineStart;
                    //Debug.Log("diffrenceStart: " + diffrenceStart.ToString());

                    Vector3 normalisedDiffrenceEnd = normaliseVectorComponents(difLineEnd);
                    Vector3 normalisedDiffrenceStart = normaliseVectorComponents(difLineStart);

                    writer.WriteLine("normalisedDiffrenceStart: ");
                    writer.WriteLine("x: " + normalisedDiffrenceStart.x.ToString() + " y: " + normalisedDiffrenceStart.y.ToString() + " z: " + normalisedDiffrenceStart.z.ToString());
                    writer.WriteLine("normalisedDiffrenceEnd: ");
                    writer.WriteLine("x: " + normalisedDiffrenceEnd.x.ToString() + " y: " + normalisedDiffrenceEnd.y.ToString() + " z: " + normalisedDiffrenceEnd.z.ToString());
                    
                    Vector3 crossedDiffrence = Vector3.Scale(normalisedDiffrenceEnd, normalisedDiffrenceStart);
                    Vector3 changeInTwoVertices = new Vector3(1f, 1f, 1f) - crossedDiffrence;

                    writer.WriteLine("crossedDiffrence: ");
                    writer.WriteLine("x: " + crossedDiffrence.x.ToString() + " y: " + crossedDiffrence.y.ToString() + " z: " + crossedDiffrence.z.ToString());
                    writer.WriteLine("changeInTwoVertices: ");
                    writer.WriteLine("x: " + changeInTwoVertices.x.ToString() + " y: " + changeInTwoVertices.y.ToString() + " z: " + changeInTwoVertices.z.ToString());


                    Vector3 difference = lineEnd - lineStart;
                    Vector3 normalisedDifference = normaliseVectorComponents(difference);
                    Vector3 changeInVerticesStartandEnd = new Vector3(1f, 1f, 1f) - normalisedDifference;

                    writer.WriteLine("difference: ");
                    writer.WriteLine("x: " + difference.x.ToString() + " y: " + difference.y.ToString() + " z: " + difference.z.ToString());
                    writer.WriteLine("normalisedDifference: ");
                    writer.WriteLine("x: " + normalisedDifference.x.ToString() + " y: " + normalisedDifference.y.ToString() + " z: " + normalisedDifference.z.ToString());
                    writer.WriteLine("changeInVerticesStartandEnd: ");
                    writer.WriteLine("x: " + changeInVerticesStartandEnd.x.ToString() + " y: " + changeInVerticesStartandEnd.y.ToString() + " z: " + changeInVerticesStartandEnd.z.ToString());

                    Vector3 changeInVertex = Vector3.Scale(changeInTwoVertices, changeInVerticesStartandEnd);

                    writer.WriteLine("changeInVertex: ");
                    writer.WriteLine("x: " + changeInVertex.x.ToString() + " y: " + changeInVertex.y.ToString() + " z: " + changeInVertex.z.ToString());

                    Matrix4x4 changeInVertexMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, changeInVertex);
                    matricesToUpdateVertices[n] = changeInVertexMatrix;

                    writer.WriteLine("changeInVertexMatrix: " + changeInVertexMatrix.ToString());

                    n += 1;
                }

            }

            writer.WriteLine("boxVerticesToUpdate: ");
            boxVerticesToUpdate.ToList().ForEach(i => writer.WriteLine(i.x.ToString() + ", " + i.y.ToString() + ", " + i.z.ToString()));
            writer.WriteLine("matricesToUpdateVertices: ");
            matricesToUpdateVertices.ToList().ForEach(i => writer.WriteLine(i.ToString()));

            writer.Close();

        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed UpdateBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return;
        }
    }

    private void ChangeBoundingBox(Vector3 displacementVector)
    {
        try
        {
            for (int i = 0; i < boxVertices.Length; i++)
            {
                for (int j = 0; j < boxVerticesToUpdate.Length; j++)
                {
                    if (boxVertices[i] == boxVerticesToUpdate[j])
                    {
                        boxVertices[i] += matricesToUpdateVertices[j].MultiplyPoint(displacementVector);
                    }
                }
            }

            // Update the mesh
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.vertices = boxVertices;
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed UpdateBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return;
        }
    }

    private Vector3 normaliseVectorComponents(Vector3 vector)
    {
        try
        {
            Vector3 vectorNormalized = new Vector3(0f, 0f, 0f);
            if (vector.x != 0f)
            {
                vectorNormalized.x = (vector.x / vector.x);
            }
            if (vector.y != 0f)
            {
                vectorNormalized.y = (vector.y / vector.y);
            }
            if (vector.z != 0f)
            {
                vectorNormalized.z = (vector.z / vector.z);
            }
            return vectorNormalized;

        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed UpdateBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return new Vector3(0f, 0f, 0f);
        }
    }


    private void RotateBoundingBox(Vector3 displacementVector, Vector3 RightHandPositionRotate)
    {
        try
        {
            writer = new StreamWriter(pathTest, true);
            str = "############################################RotateBoundingBox##############################################";
            writer.WriteLine(str);

            boxVerticesInOrder = new Vector3[8];
            Vector3 middleVector1 = new Vector3(0f, 0f, 0f);
            Vector3 middleVector2 = new Vector3(0f, 0f, 0f);
            int n1 = 0;
            int n2 = 4;

            Vector3 displacementVectorInXZPlane = new Vector3(displacementVector.x, 0f, displacementVector.z);

            str = displacementVectorInXZPlane.ToString();
            writer.WriteLine("displacementVectorInXZPlane: " + str);
            writer.WriteLine("x: " + displacementVectorInXZPlane.x.ToString() + " y: " + displacementVectorInXZPlane.y.ToString() + " z: " + displacementVectorInXZPlane.z.ToString());

            float rotationValue = Vector3.Magnitude(displacementVectorInXZPlane);
            str = rotationValue.ToString();
            writer.WriteLine("rotationValue: " + str);

            for (int i = 0; i < boxVertices.Length; i++)
            {
                if (boxVertices[0].y == boxVertices[i].y)
                {
                    boxVerticesInOrder[n1] = boxVertices[i];
                    middleVector1 += boxVertices[i];
                    n1 += 1;
                }
                else
                {
                    boxVerticesInOrder[n2] = boxVertices[i];
                    middleVector2 += boxVertices[i];
                    n2 += 1;
                }

            }
            writer.WriteLine("boxVertices: ");
            boxVertices.ToList().ForEach(i => writer.WriteLine(i.x.ToString() + ", " + i.y.ToString() + ", " + i.z.ToString()));
            writer.WriteLine("boxVerticesInOrder: ");
            boxVerticesInOrder.ToList().ForEach(i => writer.WriteLine(i.x.ToString() + ", " + i.y.ToString() + ", " + i.z.ToString()));

            middleVector1 = middleVector1 / 4;
            middleVector2 = middleVector2 / 4;

            writer.WriteLine("middleVector1: " + middleVector1.x.ToString() + ", " + middleVector1.y.ToString() + ", " + middleVector1.z.ToString());
            writer.WriteLine("middleVector2: " + middleVector2.x.ToString() + ", " + middleVector2.y.ToString() + ", " + middleVector2.z.ToString());

            Vector3 RightHandPositionRotateInXZPlane = new Vector3(RightHandPositionRotate.x, 0f, RightHandPositionRotate.z);
            Vector3 middleVector1InXZPlane = new Vector3(middleVector1.x, 0f, middleVector1.z);

            Vector3 RightHandPositionInRotationAxis = RightHandPositionRotateInXZPlane - middleVector1InXZPlane;
            Vector3 directionOfRotation = Vector3.Cross(RightHandPositionInRotationAxis, displacementVectorInXZPlane);
            Vector3 directionOfRotationNormalised = Vector3.Normalize(directionOfRotation);

            writer.WriteLine("directionOfRotation: " + directionOfRotation.x.ToString() + ", " + directionOfRotation.y.ToString() + ", " + directionOfRotation.z.ToString());
            writer.WriteLine("directionOfRotationNormalised: " + directionOfRotationNormalised.x.ToString() + ", " + directionOfRotationNormalised.y.ToString() + ", " + directionOfRotationNormalised.z.ToString());

            Vector3 finalRotation = 4* rotationValue * directionOfRotationNormalised;

            double[] rvec = new double[3] { 0d , finalRotation.y , 0d };
            double[,] rotMat = new double[3, 3] { { 0d, 0d, 0d }, { 0d, 0d, 0d }, { 0d, 0d, 0d } };

            rvec.ToList().ForEach(i => writer.WriteLine(i.ToString()));

            Cv2.Rodrigues(rvec, out rotMat);

            Matrix4x4 matrixRotation = new Matrix4x4();
            matrixRotation.SetRow(0, new Vector4((float)rotMat[0, 0], (float)rotMat[0, 1], (float)rotMat[0, 2], 0f));
            matrixRotation.SetRow(1, new Vector4((float)rotMat[1, 0], (float)rotMat[1, 1], (float)rotMat[1, 2], 0f));
            matrixRotation.SetRow(2, new Vector4((float)rotMat[2, 0], (float)rotMat[2, 1], (float)rotMat[2, 2], 0f));
            matrixRotation.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

            writer.WriteLine("matrixRotation: " );
            writer.WriteLine(matrixRotation.ToString());

            
            for (int i = 0; i < boxVerticesInOrder.Length; i++)
            {
                if (i < 4)
                {
                    for (int j = 0; j < boxVertices.Length; j++)
                    {
                        if (boxVerticesInOrder[i] == boxVertices[j])
                        {
                            writer.WriteLine("boxVertices[j]before: " + boxVertices[j].x.ToString() + ", " + boxVertices[j].y.ToString() + ", " + boxVertices[j].z.ToString());
                            Vector3 verticeCalculated = boxVertices[j] - middleVector1;
                            writer.WriteLine("verticeCalculated: " + verticeCalculated.x.ToString() + ", " + verticeCalculated.y.ToString() + ", " + verticeCalculated.z.ToString());
                            boxVertices[j] = middleVector1 + matrixRotation.MultiplyPoint(verticeCalculated);
                            writer.WriteLine("boxVertices[j]: " + boxVertices[j].x.ToString() + ", " + boxVertices[j].y.ToString() + ", " + boxVertices[j].z.ToString());
                        }
                    }
                    
                }
                else
                {
                    for (int j = 0; j < boxVertices.Length; j++)
                    {
                        if (boxVerticesInOrder[i] == boxVertices[j])
                        {
                            writer.WriteLine("boxVertices[j]before: " + boxVertices[j].x.ToString() + ", " + boxVertices[j].y.ToString() + ", " + boxVertices[j].z.ToString());
                            Vector3 verticeCalculated = boxVertices[j] - middleVector2;
                            writer.WriteLine("verticeCalculated: " + verticeCalculated.x.ToString() + ", " + verticeCalculated.y.ToString() + ", " + verticeCalculated.z.ToString());
                            boxVertices[j] = middleVector2 + matrixRotation.MultiplyPoint(verticeCalculated);
                            writer.WriteLine("boxVertices[j]: " + boxVertices[j].x.ToString() + ", " + boxVertices[j].y.ToString() + ", " + boxVertices[j].z.ToString());
                        }
                    }
                }
            }

            writer.WriteLine("boxVertices: ");
            boxVertices.ToList().ForEach(i => writer.WriteLine(i.x.ToString() + ", " + i.y.ToString() + ", " + i.z.ToString()));

            writer.Close();

            // Update the mesh
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.vertices = boxVertices;
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();
        }
        catch (Exception e)
        {
            writerErrorLog = new StreamWriter(pathErrorLog, true);
            strError = strError + "Failed UpdateBoundingBox :" + e.Message;
            writerErrorLog.WriteLine(strError);
            writerErrorLog.Close();
            return;
        }
    }




}
