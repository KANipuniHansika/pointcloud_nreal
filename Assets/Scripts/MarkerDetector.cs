using NRKernal;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using OpenCvSharp.Aruco;
using System.Collections.Generic;
using System.Linq;
using plyData;
using UnityEngine.Rendering;

namespace pointcloud_nreal
{
    public class MarkerDetector : MonoBehaviour
    {
        [SerializeField] GameObject cubeRed;
        [SerializeField] GameObject cubeGreen;
        [SerializeField] GameObject cubeBlue;
        [SerializeField] GameObject cubeYellow;
        [SerializeField] GameObject cubePurple;
        [SerializeField] GameObject pointCloud;

        public RawImage CaptureImage;
        /// <summary> Number of frames. </summary>
        public Text FrameCount;
        /// <summary> Gets or sets the RGB camera texture. </summary>
        /// <value> The RGB camera texture. </value>
        private NRRGBCamTexture RGBCamTexture { get; set; }


        private Text MarkerCount;
        private Text TextOut;
        private Texture2D output;

        private List<Matrix4x4> markerTransforms;
        List<int> result;

        Matrix4x4 matrixA;
        Matrix4x4 matrixX;
        Matrix4x4 matrixB;
        Matrix4x4 matrixCalc;
        Matrix4x4 matrixFinal;
        Matrix4x4 matrixATemp;

        Vector3 cubeRedInWorldPosition = new Vector3(0f, 0f, 0f);
        Vector3 cubeGreenInWorldPosition = new Vector3(0f, 0f, 0f);
        Vector3 cubeBlueInWorldPosition = new Vector3(0f, 0f, 0f);
        Vector3 cubeYellowInWorldPosition = new Vector3(0f, 0f, 0f);
        Vector3 cubePurpleInWorldPosition = new Vector3(0f, 0f, 0f);


        private StreamWriter verticesWriter;
        private StreamWriter verticesWriter1;
        private StreamWriter verticesWriter2;


        // Start is called before the first frame update
        void Start()
        {
            RGBCamTexture = new NRRGBCamTexture();
            CaptureImage.texture = RGBCamTexture.GetTexture();
            RGBCamTexture.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (RGBCamTexture == null)
            {
                NRDebugger.Error("RGBCamTexture is null!");
                String str = "RGBCamTexture is null!";
                string path = "/storage/emulated/0/Nreal/testexception.txt";
                StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(str);
                writer.Close();
                return;
            }
            FrameCount.text = RGBCamTexture.FrameCount.ToString();

            if (NRInput.GetButtonDown(ControllerButton.HOME))
            {
                if (RGBCamTexture == null)
                {
                    NRDebugger.Error("RGBCamTexture is null!");
                    String str = "RGBCamTexture is null!";
                    string path = "/storage/emulated/0/Nreal/testexception.txt";
                    StreamWriter writer = new StreamWriter(path, true);
                    writer.WriteLine(str);
                    writer.Close();
                    return;
                }
                ProcessTexture(RGBCamTexture);
                
            }
        }

        private void ProcessTexture(NRRGBCamTexture input)
        {
            try
            {
                Texture2D targetTexture = input.GetTexture();
                byte[] rawData = targetTexture.GetRawTextureData();
                string s = System.Text.Encoding.ASCII.GetString(rawData);
                string path = "/storage/emulated/0/Nreal/rawData.txt";
                StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(s);
                writer.Close();
                //File.WriteAllBytes("/storage/emulated/0/Pictures/rawData.txt", rawData);

                OpenCvSharp.Unity.TextureConversionParams targetTextureparams = new OpenCvSharp.Unity.TextureConversionParams() { FlipVertically = false, FlipHorizontally = true, RotationAngle = 0 };

                Mat img = OpenCvSharp.Unity.TextureToMat(targetTexture, targetTextureparams);
                ProcessImage(img, targetTexture);
            }
            catch (Exception e)
            {
                NRDebugger.Error("Process Texture faild!");
                String str = e.ToString();
                string path = "/storage/emulated/0/Nreal/testexception.txt";
                StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(str);
                writer.Close();
            }
        }

        private void ProcessImage(Mat img, Texture2D texture)
        {
            try
            {
                // Variables to hold results
                Point2f[][] corners;
                int[] ids;
                Point2f[][] rejectedPoints;
                string path = "/storage/emulated/0/Nreal/testProcessImage.txt";
                StreamWriter writer = new StreamWriter(path, true);

                markerTransforms = new List<Matrix4x4>();
                result = new List<int>();

                NativeResolution resolutionRGB = NRFrame.GetDeviceResolution(NativeDevice.RGB_CAMERA);
                NativeMat3f RGBcameraMatrix = NRFrame.GetRGBCameraIntrinsicMatrix();
                NRDistortionParams RGBcameraDistortion = NRFrame.GetDeviceDistortion(NativeDevice.RGB_CAMERA);
                bool eyeMatrix;
                float znear = 0;
                float zfar = 0;
                EyeProjectMatrixData eyeProjectMatrix = NRFrame.GetEyeProjectMatrix(out eyeMatrix, znear, zfar);
                Matrix4x4 LEyeMatrix = eyeProjectMatrix.LEyeMatrix;
                Matrix4x4 REyeMatrix = eyeProjectMatrix.REyeMatrix;
                Matrix4x4 RGBEyeMatrix = eyeProjectMatrix.RGBEyeMatrix;
                var eyeProjectMatrixString = eyeProjectMatrix.ToString();
                Pose headpose = NRFrame.HeadPose;
                Pose RGBcamPose = NRFrame.GetDevicePoseFromHead(NativeDevice.RGB_CAMERA);
                Pose leftDisplayPose = NRFrame.GetDevicePoseFromHead(NativeDevice.LEFT_DISPLAY);
                Pose RightDisplayPose = NRFrame.GetDevicePoseFromHead(NativeDevice.RIGHT_DISPLAY);
                Matrix4x4 leftDisplayPose_To_Head = Matrix4x4.TRS(leftDisplayPose.position, leftDisplayPose.rotation, Vector3.one);
                Matrix4x4 RightDisplayPose_To_Head = Matrix4x4.TRS(RightDisplayPose.position, RightDisplayPose.rotation, Vector3.one);
                Matrix4x4 Camera_To_Head = Matrix4x4.TRS(RGBcamPose.position, RGBcamPose.rotation, Vector3.one);
                Matrix4x4 Head_To_World = Matrix4x4.TRS(headpose.position, headpose.rotation, Vector3.one);
                Matrix4x4 Camera_To_World = Head_To_World * Camera_To_Head;

                //Vector3 pInHead = Head_T_cam.MultiplyPoint(new Vector3(1, 0, 0));

                string str = "\n" + "******All Data********" + "\n";
                str = str + "Texture Width: " + texture.width + "Texture Height: " + texture.height + "\n";
                str = str + "Resolution: " + resolutionRGB + "\n";
                str = str + "RGBcameraMatrix: " + RGBcameraMatrix + "\n";
                str = str + "RGBcameraDistortion: " + RGBcameraDistortion + "\n";
                str = str + "LEyeMatrix: " + LEyeMatrix + "\n";
                str = str + "REyeMatrix: " + REyeMatrix + "\n";
                str = str + "RGBEyeMatrix: " + RGBEyeMatrix + "\n";
                str = str + "eyeProjectMatrixString: " + eyeProjectMatrixString + "\n";
                str = str + "headpose: " + headpose + "\n";
                str = str + "RGBcamPose: " + RGBcamPose + "\n";
                str = str + "leftDisplayPose: " + leftDisplayPose + "\n";
                str = str + "RightDisplayPose: " + RightDisplayPose + "\n";
                str = str + "leftDisplayPose_To_Head: " + leftDisplayPose_To_Head + "\n";
                str = str + "RightDisplayPose_To_Head: " + RightDisplayPose_To_Head + "\n";
                str = str + "Camera_To_Head: " + Camera_To_Head + "\n";
                str = str + "Head_To_World: " + Head_To_World + "\n";
                str = str + "Camera_To_World: " + Camera_To_World + "\n";
                writer.WriteLine(str);

                Mat gray = new Mat();
                Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
                DetectorParameters detectorParameters = DetectorParameters.Create();
                var dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);

                CvAruco.DetectMarkers(gray, dictionary, out corners, out ids, detectorParameters, out rejectedPoints);
                str = "detectmarkers - Markers detected";
                writer.WriteLine(str);

                string resultOut = $"[{string.Join(",", ids)}]";
                str = "write the ids detected" + "\n" + "M: " + resultOut;
                writer.WriteLine(str);

                //Mat detectedMarkers = img;

                str = "write the corners detected: " + corners.Length.ToString() + "\n";
                writer.WriteLine(str);

                CvAruco.DrawDetectedMarkers(gray, corners, ids, Scalar.Crimson);
                output = OpenCvSharp.Unity.MatToTexture(gray, output);
                byte[] bytesPNG = output.EncodeToPNG();
                File.WriteAllBytes("/storage/emulated/0/Nreal/detectedMarkers.png", bytesPNG);

                float xid5=0, yid5=0, xid23=0, yid23=0, xid41=0, yid41=0, xid59=0, yid59=0;

                for (int i = 0; i < ids.Length; i++)
                {
                    float totalX = 0;
                    float totalY = 0;
                    for (int j = 0; j < (corners[i].Length); j++)
                    {
                        totalX = totalX + corners[i][j].X;
                        totalY = totalY + corners[i][j].Y;
                        string corneridsx = corners[i][j].X.ToString();
                        string corneridsy = corners[i][j].Y.ToString();
                        writer.WriteLine("corner:" + i.ToString() + "cornerId:" + ids[i].ToString() + " [i]:" + i.ToString() + " [j]:" + j.ToString() + " X: " + corneridsx + " Y: " + corneridsy + "\n");
                    }
                    float xValue = 1280f - (totalX / 4);
                    float yValue = totalY / 4;
                    writer.WriteLine("cornerId:" + ids[i].ToString() + " length:" + corners[i].Length.ToString() + " totalX:" + totalX.ToString() + " totalY:" + totalY.ToString() + " xValue:" + xValue.ToString() + " yValue:" + yValue.ToString() + "\n");
                    switch (ids[i])
                    {
                        case 5:
                            xid5 = xValue;
                            yid5 = yValue;
                            break;
                        case 23:
                            xid23 = xValue;
                            yid23 = yValue;
                            break;
                        case 41:
                            xid41 = xValue;
                            yid41 = yValue;
                            break;
                        case 59:
                            xid59 = xValue;
                            yid59 = yValue;
                            break;
                        default:
                            break;
                    }    
                }

                double[,] matrix3x3 = new double[3, 3] {
                {1146.431d, 0d, 653.5949d},
                {0d, 1146.744d, 378.364d},
                {0d, 0d, 1d},
                };

                //4.3cm
                float markerSizeInMeters = 0.30f;

                Point3f[] markerPoints = new Point3f[] {
                new Point3f(-markerSizeInMeters / 2f,  markerSizeInMeters / 2f, 0f),
                new Point3f( markerSizeInMeters / 2f,  markerSizeInMeters / 2f, 0f),
                new Point3f( markerSizeInMeters / 2f, -markerSizeInMeters / 2f, 0f),
                new Point3f(-markerSizeInMeters / 2f, -markerSizeInMeters / 2f, 0f)
                };

                Point2f[] markerPointsImage = new Point2f[] {
                new Point2f(xid5, yid5),
                new Point2f(xid23, yid23),
                new Point2f(xid41, yid41),
                new Point2f(xid59, yid59),
                };

                double[] distCoeffs = new double[4] { 0.1318923d, -0.5029326d, 0.00262032d, 0.001157547d };
                
                double[] rvec = new double[3] { 0d, 0d, 0d };
                double[] tvec = new double[3] { 0d, 0d, 0d };
                double[,] rotMat = new double[3, 3] { { 0d, 0d, 0d }, { 0d, 0d, 0d }, { 0d, 0d, 0d } };

                Cv2.SolvePnP(markerPoints, markerPointsImage, matrix3x3, distCoeffs, out rvec, out tvec, false, SolvePnPFlags.P3P);
                Cv2.Rodrigues(rvec, out rotMat);
                Matrix4x4 matrixTranslation = new Matrix4x4();
                matrixTranslation.SetRow(0, new Vector4((float)rotMat[0, 0], (float)rotMat[0, 1], (float)rotMat[0, 2], (float)tvec[0]));
                matrixTranslation.SetRow(1, new Vector4((float)rotMat[1, 0], (float)rotMat[1, 1], (float)rotMat[1, 2], (float)tvec[1]));
                matrixTranslation.SetRow(2, new Vector4((float)rotMat[2, 0], (float)rotMat[2, 1], (float)rotMat[2, 2], (float)tvec[2]));
                matrixTranslation.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
                //Matrix4x4 matrix = PositionObject(matrixTranslation);
                Matrix4x4 worldObjectIncameraMatrix = Camera_To_World * matrixTranslation;
                //Matrix4x4 world_Matrix = Camera_To_World * matrix;

                str = "\n" + "******Data2DTo3D********" + "\n";
                str = str + "markerPoints: " + String.Join(" ", markerPoints.ToString()) + "\n";
                str = str + "markerPointsImage: " + String.Join(" ", markerPointsImage.ToString()) + "\n";
                str = str + "distCoeffs: " + String.Join(" ", distCoeffs) + "\n";
                str = str + "rvec: " + String.Join(" ", rvec) + "\n";
                str = str + "tvec: " + String.Join(" ", tvec) + "\n";
                str = str + "rotMat: " + rotMat + "\n";
                str = str + "matrixTranslation: " + matrixTranslation + "\n";
                //str = str + "matrix: " + matrix + "\n";
                str = str + "worldObjectIncameraMatrix: " + worldObjectIncameraMatrix + "\n";
                //str = str + "world_Matrix: " + world_Matrix + "\n";
                writer.WriteLine(str);

                for (int i = 0; i < ids.Length; i++)
                {
                    Cv2.SolvePnP(markerPoints, corners[i], matrix3x3, distCoeffs, out rvec, out tvec, false, SolvePnPFlags.Iterative);
                    CvAruco.DrawAxis(gray, matrix3x3, distCoeffs, rvec, tvec, 0.06f);
                    output = OpenCvSharp.Unity.MatToTexture(gray, output);
                    byte[] bytesPng = output.EncodeToPNG();
                    File.WriteAllBytes("/storage/emulated/0/Nreal/detectedAxises.png", bytesPng);
                }

                try
                {
                    cubeRedInWorldPosition = worldObjectIncameraMatrix.MultiplyPoint(new Vector3(-0.15f, 0.15f, 0f));
                    cubeRed.transform.position = new Vector3(cubeRedInWorldPosition.x, cubeRedInWorldPosition.y, cubeRedInWorldPosition.z);
                    str = "\n" + "******id 5********" + "\n";
                    str = str + "cubeRedInWorldPosition: " + cubeRedInWorldPosition + "\n";
                    str = str + "localPositionRed: " + cubeRedInWorldPosition.x.ToString() + "," + cubeRedInWorldPosition.y.ToString() + "," + cubeRedInWorldPosition.z.ToString() + "\n";

                    cubeBlueInWorldPosition = worldObjectIncameraMatrix.MultiplyPoint(new Vector3(0.15f, 0.15f, 0f));
                    cubeBlue.transform.position = new Vector3(cubeBlueInWorldPosition.x, cubeBlueInWorldPosition.y, cubeBlueInWorldPosition.z);
                    str = str + "\n" + "******id 23********" + "\n";
                    str = str + "cubeBlueInWorldPosition: " + cubeBlueInWorldPosition + "\n";
                    str = str + "localPositionBlue: " + cubeBlueInWorldPosition.x.ToString() + "," + cubeBlueInWorldPosition.y.ToString() + "," + cubeBlueInWorldPosition.z.ToString() + "\n";

                    cubeGreenInWorldPosition = worldObjectIncameraMatrix.MultiplyPoint(new Vector3(0.15f, -0.15f, 0f));
                    cubeGreen.transform.position = new Vector3(cubeGreenInWorldPosition.x, cubeGreenInWorldPosition.y, cubeGreenInWorldPosition.z);
                    str = str + "\n" + "******id 41********" + "\n";
                    str = str + "cubeGreenInWorldPosition: " + cubeGreenInWorldPosition + "\n";
                    str = str + "localPositionGreen: " + cubeGreenInWorldPosition.x.ToString() + "," + cubeGreenInWorldPosition.y.ToString() + "," + cubeGreenInWorldPosition.z.ToString() + "\n";

                    cubeYellowInWorldPosition = worldObjectIncameraMatrix.MultiplyPoint(new Vector3(-0.15f, -0.15f, 0f));
                    cubeYellow.transform.position = new Vector3(cubeYellowInWorldPosition.x, cubeYellowInWorldPosition.y, cubeYellowInWorldPosition.z);
                    str = str + "\n" + "******id 59********" + "\n";
                    str = str + "cubeYellowInWorldPosition: " + cubeYellowInWorldPosition + "\n";
                    str = str + "localPositionYellow: " + cubeYellowInWorldPosition.x.ToString() + "," + cubeYellowInWorldPosition.y.ToString() + "," + cubeYellowInWorldPosition.z.ToString() + "\n";

                    cubePurpleInWorldPosition = worldObjectIncameraMatrix.MultiplyPoint(new Vector3(0.15f, 0.15f, 0.15f));
                    cubePurple.transform.position = new Vector3(cubePurpleInWorldPosition.x, cubePurpleInWorldPosition.y, cubePurpleInWorldPosition.z);
                    str = str + "\n" + "******id 0********" + "\n";
                    str = str + "cubePurpleInWorldPosition: " + cubePurpleInWorldPosition + "\n";
                    str = str + "localPositionPurple: " + cubePurpleInWorldPosition.x.ToString() + "," + cubePurpleInWorldPosition.y.ToString() + "," + cubePurpleInWorldPosition.z.ToString() + "\n";


                    writer.WriteLine(str);
                }
                catch (Exception e)
                {
                    NRDebugger.Error("Process Image faild!");
                    String str1 = e.ToString();
                    string path1 = "/storage/emulated/0/Nreal/testexception1.txt";
                    StreamWriter writer1 = new StreamWriter(path1, true);
                    writer.WriteLine(str1);
                    writer.Close();
                }

                /*for (int i = 0; i < ids.Length; i++)
                {

                    if (ids[i] == 23)
                    {
                        try
                        {
                            Matrix4x4 cubeBlueInWorldMatrix = Camera_To_World * matrix;
                            cubeBlueInWorldPosition = Camera_To_World.MultiplyPoint(new Vector3(matrix.m03, matrix.m13, matrix.m23));

                            cubeBlue.transform.position = cubeBlueInWorldPosition;
                            Vector3 forward = new Vector3(cubeBlueInWorldMatrix.m02, cubeBlueInWorldMatrix.m12, cubeBlueInWorldMatrix.m22);
                            Vector3 upwards = new Vector3(cubeBlueInWorldMatrix.m01, cubeBlueInWorldMatrix.m11, cubeBlueInWorldMatrix.m21);
                            cubeBlue.transform.rotation = Quaternion.LookRotation(forward, upwards);

                            str = "\n" + "******id 23********" + "\n";
                            str = str + "cubeBlueInWorldMatrix: " + cubeBlueInWorldMatrix + "\n";
                            str = str + "localPosition: " + cubeBlueInWorldPosition.x.ToString() + "," + cubeBlueInWorldPosition.y.ToString() + "," + cubeBlueInWorldPosition.z.ToString() + "\n";
                            str = str + "localRotation: " + Quaternion.LookRotation(forward, upwards) + "\n";
                            writer.WriteLine(str);

                        }
                        catch (Exception e)
                        {
                            NRDebugger.Error("Process Image faild!");
                            String str1 = e.ToString();
                            string path1 = "/storage/emulated/0/Nreal/testexception1.txt";
                            StreamWriter writer1 = new StreamWriter(path1, true);
                            writer.WriteLine(str1);
                            writer.Close();
                        }
                    }

                }*/

                matrixFinal = new Matrix4x4();
                matrixFinal.SetRow(0, new Vector4(0f, 0f, 0f, 0f));
                matrixFinal.SetRow(1, new Vector4(0f, 0f, 0f, 0f));
                matrixFinal.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
                matrixFinal.SetRow(3, new Vector4(0f, 0f, 0f, 0f));

                matrixATemp = new Matrix4x4();
                matrixATemp.SetRow(0, new Vector4(0f, 0f, 0f, 0f));
                matrixATemp.SetRow(1, new Vector4(0f, 0f, 0f, 0f));
                matrixATemp.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
                matrixATemp.SetRow(3, new Vector4(0f, 0f, 0f, 0f));

                matrixCalc = new Matrix4x4();
                matrixCalc.SetRow(0, new Vector4(0f, 0f, 0f, 0f));
                matrixCalc.SetRow(1, new Vector4(0f, 0f, 0f, 0f));
                matrixCalc.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
                matrixCalc.SetRow(3, new Vector4(0f, 0f, 0f, 0f));

                matrixA = new Matrix4x4();
                matrixA.SetRow(0, new Vector4(0f, 0f, 0f, 0f));
                matrixA.SetRow(1, new Vector4(0f, 0f, 0f, 0f));
                matrixA.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
                matrixA.SetRow(3, new Vector4(0f, 0f, 0f, 0f));

                matrixX = new Matrix4x4();   
                matrixX.SetRow(0, new Vector4(-0.016407f, 0.469691f, 0.678333f, 0.474119f));
                matrixX.SetRow(1, new Vector4(-0.477083f, -0.568129f, -0.579286f, -0.584403f));
                matrixX.SetRow(2, new Vector4(0.749661f, 0.248618f, 0.443195f, 0.678341f));
                matrixX.SetRow(3, new Vector4(1f, 1f, 1f, 1f));

                matrixB = new Matrix4x4();
                matrixB.SetRow(0, new Vector4(-0.55f, 0.15f, 0.15f, -0.15f)); //Positive
                matrixB.SetRow(1, new Vector4(0.15f, 0.15f, -0.15f, -0.15f)); //Positive
                matrixB.SetRow(2, new Vector4(-0.15f, 0f, 0f, 0f)); //negative
                matrixB.SetRow(3, new Vector4(1f, 1f, 1f, 1f));

                //matrixB = new Matrix4x4();
                //matrixB.SetRow(0, new Vector4(-0.55f, 0.15f, 0.15f, -0.15f));
                //matrixB.SetRow(1, new Vector4(0.30f, 0.30f, 0f, 0f));
                //matrixB.SetRow(2, new Vector4(-0.10f, 0f, 0f, 0f));
                //matrixB.SetRow(3, new Vector4(1f, 1f, 1f, 1f));

                Matrix4x4 matrixXInverse = Matrix4x4.Inverse(matrixX);

                matrixA = matrixB * matrixXInverse;

                //matrixCalc = new Matrix4x4();
                //matrixCalc.SetRow(0, new Vector4(1f, 0f, 0f, matrixA.m03));
                //matrixCalc.SetRow(1, new Vector4(0f, 1f, 0f, matrixA.m13));
                ///matrixCalc.SetRow(2, new Vector4(0f, 0f, 1f, matrixA.m23));
                //matrixCalc.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

                matrixCalc = new Matrix4x4();
                matrixCalc.SetRow(0, new Vector4(matrixA.m00, matrixA.m01, matrixA.m02, matrixA.m03));
                matrixCalc.SetRow(1, new Vector4(matrixA.m10, matrixA.m11, matrixA.m12, matrixA.m13));
                matrixCalc.SetRow(2, new Vector4(matrixA.m20, matrixA.m21, matrixA.m22, matrixA.m23));
                matrixCalc.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

                matrixATemp = worldObjectIncameraMatrix * matrixCalc;

                matrixFinal = new Matrix4x4();
                matrixFinal.SetRow(0, new Vector4(matrixATemp.m00, matrixATemp.m01, matrixATemp.m02, matrixATemp.m03));
                matrixFinal.SetRow(1, new Vector4(matrixATemp.m10, matrixATemp.m11, matrixATemp.m12, matrixATemp.m13));
                matrixFinal.SetRow(2, new Vector4(matrixATemp.m20, matrixATemp.m21, matrixATemp.m22, matrixATemp.m23));
                matrixFinal.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

                str = "\n" + "******MatrixCalculated********" + "\n";
                str = str + "matrixX: " + matrixX + "\n";
                str = str + "matrixXInverse: " + matrixXInverse + "\n";
                str = str + "matrixB: " + matrixB + "\n";
                str = str + "matrixA: " + matrixA + "\n";
                str = str + "matrixCalc: " + matrixCalc + "\n";
                str = str + "matrixATemp: " + matrixATemp + "\n";
                str = str + "matrixFinal: " + matrixFinal + "\n";
                writer.WriteLine(str);

                //Point pt1 = new Point(0.0, 20.0);
                //Point pt2 = new Point(20.0, 20.0);
                //Point pt3 = new Point(20.0, 0.0);
                //Point pt4 = new Point(0.0, 0.0);

                //Point pt5 = new Point(0.0, 40.0);
                //Point pt6 = new Point(20.0, 40.0);
                ///Point pt7 = new Point(10.0, 20.0);
                //Point pt8 = new Point(0.0, 20.0);
                //corners = new Point2f[2][];
                //corners[0] = new Point2f[] { pt1, pt2, pt3, pt4 };
                //corners[1] = new Point2f[] { pt5, pt6, pt7, pt8 };

                Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Table.ply");     //**Nral path
                //Mesh mesh = ImportAsMesh("/storage/emulated/0/Nreal/Meeting_room_4010_LiDAR_binary.ply");
                var meshfilter = pointCloud.GetComponent<MeshFilter>();
                meshfilter.mesh = mesh;

                testVertices();

                writer.Close();
            }
            catch (Exception e)
            {
                NRDebugger.Error("Process Image faild!");
                String str = e.ToString();
                string path = "/storage/emulated/0/Nreal/testexception1.txt";
                StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(str);
                writer.Close();
            }
        }

        public Matrix4x4 TransfromMatrixForIndex(int markerIndex)
        {
            return markerTransforms[markerIndex];
        }

        private Matrix4x4 PositionObject(Matrix4x4 transformMatrix)
        {
            //Matrix4x4 matrixY = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
            //Matrix4x4 matrixZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            //Matrix4x4 matrix = matrixY * transformMatrix * matrixZ;
              
            Matrix4x4 Camera_To_Point = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
            Matrix4x4 matrix = Camera_To_Point * transformMatrix;

            //gameObject.transform.localPosition = MatrixHelper.GetPosition(matrix);
            //gameObject.transform.localRotation = MatrixHelper.GetQuaternion(matrix);
            //gameObject.transform.localScale = MatrixHelper.GetScale(matrix);
            return matrix;
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

                //var verticesFromData = new VerticesFromData();
                var newVertices = createVerticesData(body.vertices);
                mesh.SetVertices(newVertices);
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
            //verticesWriter = new StreamWriter("./Assets/Ply/verticesWriter.txt", true);   //**Windows path
            //verticesWriter1 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter3.txt");     //**Nral path
            //verticesWriter2 = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter4.txt");

            int vertexCount = verticesData.Count();
            List<Vector3> verticesDataTemp = new List<Vector3>(vertexCount);
            //Debug.Log("Vertices  #################### " + vertexCount);

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
                vertexTemp = matrixFinal.MultiplyPoint(var);
                verticesDataTemp.Add(vertexTemp);
                //verticesWriter.WriteLine(point4f.ToString());
            }
            //verticesWriter1.WriteLine(str1);
            //verticesWriter1.Flush();
            //verticesWriter1.Close();

            verticesData = verticesDataTemp;
            //foreach (var var in verticesData)
            //{
                //str2 = str2 + "\n" + "**************" + "\n";
                //str2 = str2 + "Vertex: " + var.ToString() + "\n";
                //str2 = str2 + "localPosition: " + var.x.ToString() + "," + var.y.ToString() + "," + var.z.ToString() + "\n";
                //Debug.Log(x.ToString());
                //verticesWriter.WriteLine(x.ToString());
            //}

            //verticesWriter2.WriteLine(str2);
            //verticesWriter2.Flush();
            //verticesWriter2.Close();

            return verticesData;
        }

        public void createTestVerticesData1(List<Vector3> verticesData)
        {
            //verticesWriter = new StreamWriter("./Assets/Ply/verticesWriter.txt", true);   //**Windows path
            verticesWriter = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter1.txt");     //**Nral path

            int vertexCount = verticesData.Count();
            List<Vector3> verticesDataTemp = new List<Vector3>(vertexCount);
            //Debug.Log("Vertices  #################### " + vertexCount);

            //for (int n = 0; n < 5; n++)
            foreach (var x in verticesData)
            {
                Vector3 vertexTemp = new Vector3(0f, 0f, 0f);
                vertexTemp = matrixCalc.MultiplyPoint(x);
                verticesDataTemp.Add(vertexTemp);
                //verticesWriter.WriteLine(point4f.ToString());
            }
            verticesData = verticesDataTemp;
            String str = "";
            foreach (var var in verticesData)
            {
                str = str + "\n" + "**************" + "\n";
                str = str + "Vertex: " + var.ToString() + "\n";
                str = str + "localPosition: " + var.x.ToString() + "," + var.y.ToString() + "," + var.z.ToString() + "\n";
                //Debug.Log(x.ToString());
                
            }
            verticesWriter.WriteLine(str);
            verticesWriter.Flush();
            verticesWriter.Close();

            //return verticesData;
        }

        public void createTestVerticesData2(List<Vector3> verticesData)
        {
            //verticesWriter = new StreamWriter("./Assets/Ply/verticesWriter.txt", true);   //**Windows path
            verticesWriter = new StreamWriter("/storage/emulated/0/Nreal/verticesWriter2.txt");     //**Nral path

            int vertexCount = verticesData.Count();
            List<Vector3> verticesDataTemp = new List<Vector3>(vertexCount);
            //Debug.Log("Vertices  #################### " + vertexCount);

            //for (int n = 0; n < 5; n++)
            foreach (var x in verticesData)
            {
                Vector3 vertexTemp = new Vector3(0f, 0f, 0f);
                vertexTemp = matrixFinal.MultiplyPoint(x);
                verticesDataTemp.Add(vertexTemp);
                //verticesWriter.WriteLine(point4f.ToString());
            }
            verticesData = verticesDataTemp;
            String str = "";
            foreach (var var in verticesData)
            {
                str = str + "\n" + "**************" + "\n";
                str = str + "Vertex: " + var.ToString() + "\n";
                str = str + "localPosition: " + var.x.ToString() + "," + var.y.ToString() + "," + var.z.ToString() + "\n";
                //Debug.Log(x.ToString());

            }
            verticesWriter.WriteLine(str);
            verticesWriter.Flush();
            verticesWriter.Close();

            //return verticesData;
        }
        public void testVertices()
        {
            try
            {
                List<Vector3> TestVerticesData = new List<Vector3> {
                new Vector3(-0.016407f, -0.477083f, 0.749661f),
                new Vector3(0.469691f, -0.568129f, 0.248618f),
                new Vector3(0.678333f, -0.579286f, 0.443195f),
                new Vector3(0.474119f, -0.584403f, 0.678341f),
                new Vector3(0.258802f, -0.572500f, 0.470895f),
                };
                //var verticesFromData = new VerticesFromData();
                createTestVerticesData1(TestVerticesData);
                createTestVerticesData2(TestVerticesData);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing: " + e.Message);
            }
        }

    }
}