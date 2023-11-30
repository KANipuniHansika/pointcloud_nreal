using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace plyData
{
    public class KDTree
    {
        private KDNode rootNode;
        private KDNodeWithIndex rootNodeWithIndex;

        public void Build(PointCloud pointCloud)
        {
            List<Tuple<Vector3, Color32>> pointsData = pointCloud.pointsData;
            //List<Vector3> points = pointCloud.Points;
            rootNode = BuildTree(pointsData, 0);
        }

        public void BuildPointCloudWithIndex(PointCloudWithIndex pointCloudWithIndex)
        {
            List<Tuple<Vector3, int>> pointsDataWithIndex = pointCloudWithIndex.pointsDataWithIndex;
            //List<Vector3> points = pointCloud.Points;
            rootNodeWithIndex = BuildTreeWithIndex(pointsDataWithIndex, 0);
        }

        private KDNode BuildTree(List<Tuple<Vector3, Color32>> pointsData, int depth)
        {
            if (pointsData.Count == 0)
                return null;

            int axis = depth % 3;
            pointsData.Sort((a, b) => a.Item1[axis].CompareTo(b.Item1[axis]));
            //points.Sort((a, b) => a[axis].CompareTo(b[axis]));

            int median = pointsData.Count / 2;
            KDNode node = new KDNode(pointsData[median]);
            node.LeftChild = BuildTree(pointsData.GetRange(0, median), depth + 1);
            node.RightChild = BuildTree(pointsData.GetRange(median + 1, pointsData.Count - median - 1), depth + 1);

            return node;
        }


        private KDNodeWithIndex BuildTreeWithIndex(List<Tuple<Vector3, int>> pointsDataWithIndex, int depth)
        {
            if (pointsDataWithIndex.Count == 0)
                return null;

            int axis = depth % 3;
            pointsDataWithIndex.Sort((a, b) => a.Item1[axis].CompareTo(b.Item1[axis]));
            //points.Sort((a, b) => a[axis].CompareTo(b[axis]));

            int median = pointsDataWithIndex.Count / 2;
            KDNodeWithIndex nodeWithIndex = new KDNodeWithIndex(pointsDataWithIndex[median]);
            nodeWithIndex.LeftChildWithIndex = BuildTreeWithIndex(pointsDataWithIndex.GetRange(0, median), depth + 1);
            nodeWithIndex.RightChildWithIndex = BuildTreeWithIndex(pointsDataWithIndex.GetRange(median + 1, pointsDataWithIndex.Count - median - 1), depth + 1);

            return nodeWithIndex;
        }

        public List<Vector3> Search(Vector3 queryPoint, float radius)
        {
            List<Vector3> result = new List<Vector3>();
            SearchRecursively(rootNode, queryPoint, radius, 0, result);
            return result;
        }

        private void SearchRecursively(KDNode node, Vector3 queryPoint, float radius, int depth, List<Vector3> result)
        {
            if (node == null)
                return;

            int axis = depth % 3;
            float distance = Mathf.Abs(queryPoint[axis] - node.Point[axis]);

            if (distance <= radius)
            {
                float sqrDistance = (node.Point - queryPoint).sqrMagnitude;
                if (sqrDistance <= radius * radius)
                    result.Add(node.Point);
            }

            if (queryPoint[axis] < node.Point[axis])
                SearchRecursively(node.LeftChild, queryPoint, radius, depth + 1, result);
            else
                SearchRecursively(node.RightChild, queryPoint, radius, depth + 1, result);

            if (distance <= radius)
            {
                if (queryPoint[axis] < node.Point[axis])
                    SearchRecursively(node.RightChild, queryPoint, radius, depth + 1, result);
                else
                    SearchRecursively(node.LeftChild, queryPoint, radius, depth + 1, result);
            }
        }

        
        public void SearchCuboidArea(Vector3 minCorner, Vector3 maxCorner, out List<Vector3> verticesDataAferSearch, out List<Color32> colorsDataAferSearch)
        {
            //List<Tuple<Vector3, Color32>> pointsDataAfterSearch = new List<Tuple<Vector3, Color32>>();
            verticesDataAferSearch = new List<Vector3>();
            colorsDataAferSearch = new List<Color32>();
            SearchCuboidArea(rootNode, minCorner, maxCorner, 0, verticesDataAferSearch, colorsDataAferSearch);
        }

        private void SearchCuboidArea(KDNode node, Vector3 minCorner, Vector3 maxCorner, int depth, List<Vector3> verticesDataAferSearch, List<Color32> colorsDataAferSearch)
        {
            //verticesDataAferSearch = null;
            //colorsDataAferSearch = null;

            if (node == null)
            {
                verticesDataAferSearch = null;
                colorsDataAferSearch = null;
                return;
            }
                

            Vector3 nodePoint = node.Point;
            Color32 nodeColor = node.Color;
            Color32 nodeColorNew = new Color32(0, 255, 0, 255);

            if (IsPointInCuboidArea(nodePoint, minCorner, maxCorner))
            {
                verticesDataAferSearch.Add(nodePoint);
                colorsDataAferSearch.Add(nodeColorNew);
            }   
            else
            {
                verticesDataAferSearch.Add(nodePoint);
                colorsDataAferSearch.Add(nodeColor);
            }

            int axis = depth % 3;

            if (nodePoint[axis] > minCorner[axis])
            {
                SearchCuboidArea(node.LeftChild, minCorner, maxCorner, 0, verticesDataAferSearch, colorsDataAferSearch);
            }
            if (nodePoint[axis] < maxCorner[axis])
            {
                SearchCuboidArea(node.RightChild, minCorner, maxCorner, 0, verticesDataAferSearch, colorsDataAferSearch);
            }
                
        }

        public void SearchCuboidAreaWithIndex(Vector3 minCorner, Vector3 maxCorner, out List<Vector3> verticesDataAferSearch, out List<int> indexDataAferSearch)
        {
            //List<Tuple<Vector3, Color32>> pointsDataAfterSearch = new List<Tuple<Vector3, Color32>>();
            verticesDataAferSearch = new List<Vector3>();
            indexDataAferSearch = new List<int>();
            SearchCuboidAreaWithIndex(rootNodeWithIndex, minCorner, maxCorner, 0, verticesDataAferSearch, indexDataAferSearch);
        }

        private void SearchCuboidAreaWithIndex(KDNodeWithIndex nodeWithIndex, Vector3 minCorner, Vector3 maxCorner, int depth, List<Vector3> verticesDataAferSearch, List<int> indexDataAferSearch)
        {
            //verticesDataAferSearch = null;
            //colorsDataAferSearch = null;

            if (nodeWithIndex == null)
            {
                verticesDataAferSearch = null;
                indexDataAferSearch = null;
                return;
            }


            Vector3 nodePoint = nodeWithIndex.Point;
            int nodeIndex = nodeWithIndex.Index;

            if (IsPointInCuboidArea(nodePoint, minCorner, maxCorner))
            {
                verticesDataAferSearch.Add(nodePoint);
                indexDataAferSearch.Add(nodeIndex);
            }

            int axis = depth % 3;

            if (axis == 0)
            {
                if (minCorner.x < nodePoint.x)
                {
                    SearchCuboidAreaWithIndex(nodeWithIndex.LeftChildWithIndex, minCorner, maxCorner, depth + 1, verticesDataAferSearch, indexDataAferSearch);
                }
                if (maxCorner.x > nodePoint.x)
                {
                    SearchCuboidAreaWithIndex(nodeWithIndex.RightChildWithIndex, minCorner, maxCorner, depth + 1, verticesDataAferSearch, indexDataAferSearch);
                }
            }
            else if (axis == 1)
            {
                if (minCorner.y < nodePoint.y)
                {
                    SearchCuboidAreaWithIndex(nodeWithIndex.LeftChildWithIndex, minCorner, maxCorner, depth + 1, verticesDataAferSearch, indexDataAferSearch);
                }
                if (maxCorner.y > nodePoint.y)
                {
                    SearchCuboidAreaWithIndex(nodeWithIndex.RightChildWithIndex, minCorner, maxCorner, depth + 1, verticesDataAferSearch, indexDataAferSearch);
                }
            }
            else
            {
                if (minCorner.z < nodePoint.z)
                {
                    SearchCuboidAreaWithIndex(nodeWithIndex.LeftChildWithIndex, minCorner, maxCorner, depth + 1, verticesDataAferSearch, indexDataAferSearch);
                }
                if (maxCorner.z > nodePoint.z)
                {
                    SearchCuboidAreaWithIndex(nodeWithIndex.RightChildWithIndex, minCorner, maxCorner, depth + 1, verticesDataAferSearch, indexDataAferSearch);
                }
            }

            /*
            if (nodePoint[axis] > minCorner[axis])
            {
                SearchCuboidAreaWithIndex(nodeWithIndex.LeftChildWithIndex, minCorner, maxCorner, 0, verticesDataAferSearch, indexDataAferSearch);
            }
            if (nodePoint[axis] < maxCorner[axis])
            {
                SearchCuboidAreaWithIndex(nodeWithIndex.RightChildWithIndex, minCorner, maxCorner, 0, verticesDataAferSearch, indexDataAferSearch);
            }
            */

        }

        private bool IsPointInCuboidArea(Vector3 point, Vector3 minCorner, Vector3 maxCorner)
        {
            return point.x >= minCorner.x && point.x <= maxCorner.x &&
                   point.y >= minCorner.y && point.y <= maxCorner.y &&
                   point.z >= minCorner.z && point.z <= maxCorner.z;
        }
    }

    public class KDNode
    {
        public Vector3 Point;
        public Color32 Color;
        public KDNode LeftChild;
        public KDNode RightChild;

        public KDNode(Tuple<Vector3, Color32> pointsData)
        {
            Point = pointsData.Item1;
            Color = pointsData.Item2;
            LeftChild = null;
            RightChild = null;
        }
    }

    public class KDNodeWithIndex
    {
        public Vector3 Point;
        public int Index;
        public KDNodeWithIndex LeftChildWithIndex;
        public KDNodeWithIndex RightChildWithIndex;

        public KDNodeWithIndex(Tuple<Vector3, int> pointsDataWithIndex)
        {
            Point = pointsDataWithIndex.Item1;
            Index = pointsDataWithIndex.Item2;
            LeftChildWithIndex = null;
            RightChildWithIndex = null;
        }
    }

    public class PointCloud
    {
        public List<Tuple<Vector3, Color32>> pointsData;

        //var numbers = new List<Tuple<int, string>>

        public PointCloud()
        {
            pointsData = new List<Tuple<Vector3, Color32>>();
        }

        public void AddPoint(Vector3 point, Color32 color)
        {
            pointsData.Add(new Tuple<Vector3, Color32>(point, color));
        }
    }

    public class PointCloudWithIndex
    {
        public List<Tuple<Vector3, int>> pointsDataWithIndex;

        public PointCloudWithIndex()
        {
            pointsDataWithIndex = new List<Tuple<Vector3, int>>();
        }

        public void AddPoint(Vector3 point, int index)
        {
            pointsDataWithIndex.Add(new Tuple<Vector3, int>(point, index));
        }
    }
}