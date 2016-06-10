using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using System.Diagnostics;

namespace Fusee.Tutorial.Core
{

    static class MapGenerator
    {
        public static Dictionary<string, MapTile> positionIndex;
        public static float2 mapSize = float2.One;
        public static float tileLength = 1f;
        public static float jointLength = 0.2f;
        public static SceneNodeContainer mapScene;

        public static SceneNodeContainer generate()
        {
            mapScene = new SceneNodeContainer();
            mapScene.Components = new List<SceneComponentContainer>();
            //mapScene.Children = new List<SceneNodeContainer>();

            TransformComponent transComp = new TransformComponent();
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;
            transComp.Translation = float3.Zero;

            positionIndex = new Dictionary<string, MapTile>();

            MeshComponent meshComp = new MeshComponent();
            meshComp.BoundingBox = new AABBf(float3.Zero, float3.Zero);
            meshComp.Normals = new float3[] {};
            meshComp.Triangles = new ushort[] {};
            meshComp.Vertices = new float3[] {};

            float3 verticePoint = float3.Zero;
            List<float3> tempVerticeList = new List<float3>();
            List<ushort> tempTriangleList = new List<ushort>();

            int tileCount = 0;

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    MapTile tile = new MapTile("Tile_" + x + "-" + y);

                    verticePoint = new float3((x * tileLength) + (x * jointLength), 0, (y * tileLength) + (y * jointLength));
                    tempVerticeList.Add(verticePoint);

                    verticePoint = new float3(((x + 1) * tileLength) + (x * jointLength), 0, (y * tileLength) + (y * jointLength));
                    tempVerticeList.Add(verticePoint);

                    verticePoint = new float3((x * tileLength) + (x * jointLength), 0, ((y + 1) * tileLength) + (y * jointLength));
                    tempVerticeList.Add(verticePoint);

                    verticePoint = new float3(((x + 1) * tileLength) + (x * jointLength), 0, ((y + 1) * tileLength) + (y * jointLength));
                    tempVerticeList.Add(verticePoint);

                    tempTriangleList.Add(Convert.ToUInt16((tileCount * 4)));
                    tempTriangleList.Add(Convert.ToUInt16(((tileCount * 4) + 1)));
                    tempTriangleList.Add(Convert.ToUInt16((tileCount * 4) + 2));
                    tempTriangleList.Add(Convert.ToUInt16(((tileCount * 4) + 1)));
                    tempTriangleList.Add(Convert.ToUInt16(((tileCount * 4) + 3)));
                    tempTriangleList.Add(Convert.ToUInt16(((tileCount * 4) + 2)));


                    //Debug.WriteLine((tileCount * 4) + ", " + ((tileCount * 4) + 1) + ", " + ((tileCount * 4) + 2));

                    tileCount++;
                    //meshComp.Vertices = meshComp.Vertices.Concat(verticePoint);

                }
            }

            meshComp.Vertices = tempVerticeList.ToArray();
            meshComp.Triangles = tempTriangleList.ToArray();

            

            meshComp.Normals = new float3[]
            {
                new float3(0, 1, 0),
                new float3(0, 1, 0),
                new float3(0, 1, 0),
                new float3(0, 1, 0),
            };

            mapScene.Components.Add(transComp);
            mapScene.Components.Add(meshComp);

            return mapScene;
        }


    }
}


































//SceneNodeContainer tileMap = new SceneNodeContainer();
//tileMap.Components = new List<SceneComponentContainer>();
//            tileMap.Children = new List<SceneNodeContainer>();
            
//            TransformComponent transComp = new TransformComponent();
//transComp.Rotation = float3.Zero;
//            transComp.Scale = float3.One;
//            transComp.Translation = float3.One;
//            positionIndex = new Dictionary<string, MapTile>();

//            MeshComponent meshComp = new MeshComponent();
//meshComp.BoundingBox = new AABBf(float3.Zero, float3.Zero);

//tileMap.Components.Add(transComp);
//            tileMap.Components.Add(meshComp);

//            for (int x = 0; x<mapSize.x; x++)
//            {
//                for (int y = 0; y<mapSize.y; y++)
//                {
//                    MapTile tile = new MapTile("tile." + x + "." + y, new float3(tileLength * x, 0, tileLength * y));
//tileMap.Children.Add(tile);
//                    positionIndex.Add(x + "," + y, tile);
//                }
//            }


//            for (int x = 0; x<mapSize.x; x++)
//            {
//                for (int y = 0; y<mapSize.y; y++)
//                {
//                    for (int ix = -1; ix <= 1; ix++)
//                    {
//                        for (int iy = -1; iy <= 1; iy++)
//                        {
//                            var neighbourX = x + ix;
//var neighbourY = y + iy;

//float3 currentTileUR = positionIndex[x + "," + y].GetTransform().Translation;
//float3 currentTileLR = new float3(currentTileUR.x, currentTileUR.y, currentTileUR.z - tileLength);
//float3 currentTileUL = new float3(currentTileUR.x - tileLength, currentTileUR.y, currentTileUR.z);
//float3 currentTileLL = new float3(currentTileUR.x - tileLength, currentTileUR.y, currentTileUR.z - tileLength);
//float mapJointScale = 0.2f;

//                            if (neighbourX > 1 && neighbourX<mapSize.x && neighbourY> 0 && neighbourY<mapSize.y && neighbourX != x && neighbourY != y)
//                            {
//                                positionIndex[x + "," + y].neighbours.Add(positionIndex[neighbourX + "," + neighbourY]);
//                                var neighbour = positionIndex[neighbourX + "," + neighbourY];

//MapTile mapJointX = new MapTile("MapJointX", new float3(currentTileUR.x - mapJointScale, currentTileUL.y, currentTileUL.z));
//mapJointX.GetTransform().Scale.x = 0.2f;
//                                tileMap.Children.Add(mapJointX);
//                            }

//                            if (neighbourX > 0 && neighbourX<mapSize.x && neighbourY> 1 && neighbourY<mapSize.y && neighbourX != x && neighbourY != y)
//                            {
//                                positionIndex[x + "," + y].neighbours.Add(positionIndex[neighbourX + "," + neighbourY]);
//                                var neighbour = positionIndex[neighbourX + "," + neighbourY];

//MapTile mapJointY = new MapTile("MapJointY", new float3(currentTileLR.x, currentTileLL.y, currentTileLL.z + 1));
//mapJointY.GetTransform().Scale.z = 0.2f;
//                                tileMap.Children.Add(mapJointY);
//                            }
//                        }
//                    }
//                }
//            }
//                    tileMap.Name = "TileMap";

//            return tileMap;