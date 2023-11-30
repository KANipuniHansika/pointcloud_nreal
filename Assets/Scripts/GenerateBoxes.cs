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

public class GenerateBoxes : MonoBehaviour
{
    private Color boxColor;
    private Material boxMaterial;
    //private MeshFilter meshFilter;
    

    private Vector3 minPoint;
    private Vector3 maxPoint;

    private Vector3[] boxVertices;
    private int[] boxIndices;

    private Vector3[] nearestPointToIndices;
    private float[] distanceToIndices;

    String strError = "";
    String str = "";
    String strUpdate = "";
    string pathTest;
    string pathErrorLog;
    string pathUpdate;

    StreamWriter writer;
    StreamWriter writerErrorLog;
    StreamWriter writerUpdate;

    // Start is called before the first frame update
    void Start()
    {
        //pathTest = "/storage/emulated/0/Nreal/testProcessImage.txt"; //Nreal
        //pathTest = "./Assets/PLYFiles/testProcessImage.txt";

        //pathErrorLog = "/storage/emulated/0/Nreal/errorLog.txt"; //Nreal
        pathErrorLog = "./Assets/PLYFiles/errorLog.txt";

        //pathUpdate = "/storage/emulated/0/Nreal/Update.txt";  //Nreal
        //pathUpdate = "./Assets/PLYFiles/Update.txt";

        // Create the bounding box mesh
        //CreateBoundingBoxMesh();

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //meshFilter = GetComponent<MeshFilter>();

        // Create and apply the box material
        boxMaterial = new Material(Shader.Find("Standard"));
        boxMaterial.color = boxColor;
        GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

        minPoint = new Vector3(-0.5f, -1f, 0f);
        maxPoint = new Vector3(0f, -0.5f, 0.5f);
        boxColor = Color.gray;
        InitBoundingBox(minPoint, maxPoint, boxColor);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //CreateBoundingBoxMesh();
            minPoint = new Vector3(0f, 0f, 1f);
            maxPoint = new Vector3(0.5f, 0.5f, 1.5f);
            boxColor = Color.red;
            InitBoundingBox(minPoint, maxPoint, boxColor);
        }
    }

    private void InitBoundingBox(Vector3 minPoint, Vector3 maxPoint, Color boxColor)
    {
        try
        {
            boxVertices = new Vector3[8];

            // Update the bounding box vertices based on the min and max points
            boxVertices[0] = new Vector3(minPoint.x, minPoint.y, minPoint.z);
            boxVertices[1] = new Vector3(maxPoint.x, minPoint.y, minPoint.z);
            boxVertices[2] = new Vector3(maxPoint.x, minPoint.y, maxPoint.z);
            boxVertices[3] = new Vector3(minPoint.x, minPoint.y, maxPoint.z);
            boxVertices[4] = new Vector3(minPoint.x, maxPoint.y, minPoint.z);
            boxVertices[5] = new Vector3(maxPoint.x, maxPoint.y, minPoint.z);
            boxVertices[6] = new Vector3(maxPoint.x, maxPoint.y, maxPoint.z);
            boxVertices[7] = new Vector3(minPoint.x, maxPoint.y, maxPoint.z);

            boxIndices = new int[]
            {
            0, 1, 1, 2, 2, 3, 3, 0, // Bottom edges
            4, 5, 5, 6, 6, 7, 7, 4, // Top edges
            0, 4, 1, 5, 2, 6, 3, 7  // Vertical edges
            };

            // Update the mesh
            //Mesh mesh = new Mesh();
            //Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.vertices = boxVertices;
            mesh.SetIndices(boxIndices, MeshTopology.Lines, 0);
            //mesh.RecalculateBounds();

            
            //meshFilter.sharedMesh = mesh;
            boxMaterial.color = boxColor;
            GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
            //AddLines(Color.gray);
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
}
