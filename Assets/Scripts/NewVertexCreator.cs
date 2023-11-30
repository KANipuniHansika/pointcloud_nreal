using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace pointcloud_nreal
{
    class NewVertexCreator
    {
        private StreamWriter verticesWriter;
        List<Vector3> verticesData;
        Matrix4x4 matrix4x4;
        List<Vector2> imageDataIn;
        List<Vector4> imageDataOut;

        public List<Vector3> createVerticesData(List<Vector3> verticesData)
        {
            verticesWriter = new StreamWriter("./Assets/PLYFiles/verticesWriter.txt", true);   //**Windows path
            //verticesWriter = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter.txt");     //**Nral path

            int vertexCount = verticesData.Count();
            List<Vector3> verticesDataTemp = new List<Vector3>(vertexCount);
            //Debug.Log("Vertices  #################### " + vertexCount); 
            Vector4 column0 = new Vector4(1f, 0f, 0f, 0f);
            Vector4 column1 = new Vector4(0f, 1f, 0f, 0f);
            Vector4 column2 = new Vector4(0f, 0f, 1f, 0f);
            Vector4 column3 = new Vector4(-1f, 1f, 5f, 1f);
            matrix4x4 = new Matrix4x4(column0, column1, column2, column3); 

            //for (int n = 0; n < 5; n++)
            foreach (var x in verticesData)
            {
                Vector3 vertexTemp = new Vector3(0f, 0f, 0f);
                vertexTemp = multiplyVerticesWithMatrix4x4(matrix4x4, x);
                verticesDataTemp.Add(vertexTemp);
                //verticesWriter.WriteLine(point4f.ToString());
            }
            verticesData = verticesDataTemp;
            foreach (var x in verticesData)
            {
                //Debug.Log(x.ToString());
                verticesWriter.WriteLine(x.ToString());
            }
            verticesWriter.Flush();
            verticesWriter.Close();

            return verticesData;
        }

        public List<Vector4> createImageData(List<Vector2> imageDataIn)
        {
            List<Vector2> verticesDataTemp = new List<Vector2>();

            Vector4 imageDataOutTemp = new Vector4(0f, 0f, 0f, 0f);

            return imageDataOut;
        }

        private Vector3 multiplyVerticesWithMatrix4x4(Matrix4x4 matrix4x4, Vector3 vertex)
        {
            Vector3 vertexNew = matrix4x4.MultiplyPoint(vertex);
            return vertexNew;
        }
    }
}