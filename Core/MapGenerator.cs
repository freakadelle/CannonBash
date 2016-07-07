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
        public static Random random = new Random();

        public static Dictionary<float2, MapTile> tileIndicies;
        public static float2 mapSize = float2.One;
        public static float2 minMaxNoise = float2.One;
        public static float tileSize = 1f;
        public static float jointSize = 0.2f;
        public static SceneNodeContainer mapScene;

        public static SceneNodeContainer instantiatePlaneMap(int xTiles, int yTiles)
        {
            mapSize = new float2(xTiles, yTiles);

            mapScene = new SceneNodeContainer();
            mapScene.Components = new List<SceneComponentContainer>();

            tileIndicies = new Dictionary<float2, MapTile>();

            addTransformCoponent();
            addMaterialComponent();
            addMeshComponent();

            mapScene.Name = "TileMap";

            return mapScene;
        }

        public static void addHeightNoise(float minHeight, float maxHeight)
        {
            minMaxNoise = new float2(minHeight, maxHeight);
            
            //Random MapHeight Generation
            foreach (KeyValuePair<float2, MapTile> entry in MapGenerator.tileIndicies)
            {
                translateTile(entry.Value, new float3(0, (float)(minMaxNoise.x + (random.NextDouble() * minMaxNoise.y)), 0));
            }
        }

        public static void translateTile(MapTile tile, float3 translation)
        {
            foreach (int index in tile.verticesIndicies)
            {
                float3 vertice = mapScene.GetMesh().Vertices[index];
                vertice = new float3(vertice.x + translation.x, vertice.y + translation.y, vertice.z + translation.z);
                mapScene.GetMesh().Vertices[index] = vertice;
            }

            //Todo: CenterPos of tile adjusts only height but not x and z axis
            tile.CenterPos = new float3(tile.CenterPos.x, mapScene.GetMesh().Vertices[tile.verticesIndicies[0]].y, tile.CenterPos.z);

            foreach (int index in tile.neighborJointsIndicies)
            {
                float3 vertice = mapScene.GetMesh().Vertices[index];
                //vertice = new float3(vertice.x, vertice.y + translation.y / tile.neighborJointsIndicies.Count, vertice.z);
                vertice = new float3(vertice.x, vertice.y + translation.y / 4, vertice.z);
                mapScene.GetMesh().Vertices[index] = vertice;
            }
        }


        public static void createHillAt(float2 _gridPos, float height, float2 _inclinePercXZ, float2 _boundaries)
        {
            Debug.WriteLine("CREATE HILL AT: " + _gridPos.x + " " + _gridPos.y);
            for (int x = (int)-_boundaries.x; x < (int)_boundaries.x; x++)
            {
                for (int y = (int)-_boundaries.y; y < (int)_boundaries.y; y++)
                {
                    float2 tilePos = new float2(_gridPos.x + x, _gridPos.y + y);

                    if (tileIndicies.ContainsKey(tilePos))
                    {
                        MapTile tempTile = tileIndicies[tilePos];
                        int heightLvl = System.Math.Max(System.Math.Abs(x), System.Math.Abs(y));
                        translateTile(tempTile, new float3(0, height - ((heightLvl/_boundaries.x) * height), 0));
                    }
                }
            }
        }

        public static void addTransformCoponent()
        {
            TransformComponent transComp = new TransformComponent();
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;
            transComp.Translation = float3.Zero;

            mapScene.Components.Add(transComp);
        }

        public static void addMaterialComponent()
        {
            MaterialComponent matComp = new MaterialComponent();

            matComp.Diffuse = new MatChannelContainer();
            matComp.Diffuse.Color = new float3(0f, 0.5f, 0.1f);
            matComp.Diffuse.Mix = 1;

            matComp.Emissive = new MatChannelContainer();
            matComp.Emissive.Color = new float3(0f, 0.5f, 0.1f);
            matComp.Emissive.Mix = 1;

            matComp.Specular = new SpecularChannelContainer();
            matComp.Specular.Color = float3.One;
            matComp.Specular.Intensity = 0.001f;
            matComp.Specular.Mix = 1;
            matComp.Specular.Shininess = 0.1f;

            mapScene.Components.Add(matComp);
        }

        public static void addMeshComponent()
        {
            MeshComponent meshComp = new MeshComponent();
            meshComp.BoundingBox = new AABBf(float3.Zero, float3.Zero);
            meshComp.Normals = new float3[] { };
            meshComp.Triangles = new ushort[] { };
            meshComp.Vertices = new float3[] { };

            Crossjoint tempCrossJoint;
            float3 tempTileVert = float3.Zero;
            List<float3> tempVerticeList = new List<float3>();
            List<ushort> tempTriangleList = new List<ushort>();
            List<float3> tempNormalList = new List<float3>();
            MapTile tempTile = new MapTile(float2.Zero);

            //GENERATING BASE MAP TILES
            for (int xCrossJoint = 0; xCrossJoint < mapSize.x; xCrossJoint++)
            {
                for (int yCrossJoint = 0; yCrossJoint < mapSize.y; yCrossJoint++)
                {
                    float xPos;
                    float zPos;

                    //CREATE NEW CROSSJOINT
                    xPos = (tileSize + jointSize) * xCrossJoint;
                    zPos = (tileSize + jointSize) * yCrossJoint;
                    tempCrossJoint = new Crossjoint(tempVerticeList.Count, new float3(xPos, 0, zPos));

                    tempVerticeList.Add(tempCrossJoint.pos);

                    //GENERATE MAPTILES AROUND THE CROSSJOINT VERT
                    int tileCorner = 0;
                    for (int xTile = 0; xTile <= 1; xTile++)
                    {
                        for (int zTile = 0; zTile <= 1; zTile++)
                        {
                            float2 tileIndex = new float2(xCrossJoint + xTile, yCrossJoint + zTile);

                            //If tile already created, just add crossjoint to its neighbors. Else create new Tile
                            if (tileIndicies.ContainsKey(tileIndex))
                            {
                                tempTile = tileIndicies[tileIndex];
                                tempTile.neighborJointsIndicies.Add(tempCrossJoint.index);
                            }
                            else
                            {
                                tempTile = new MapTile(tileIndex);

                                tileIndicies.Add(tempTile.index, tempTile);

                                tempTile.neighborJointsIndicies.Add(tempCrossJoint.index);

                                //Calculate centered Position of Tile
                                xPos = (tempCrossJoint.pos.x - ((tileSize + jointSize) * 0.5f)) +
                                       (xTile * (tileSize + jointSize));
                                zPos = (tempCrossJoint.pos.z - ((tileSize + jointSize) * 0.5f)) +
                                       (zTile * (tileSize + jointSize));
                                tempTile.CenterPos = new float3(xPos, 0, zPos);

                                //GENERATE VERTICES FOR EVERY CORNER OF SINGLE MAPTILE
                                for (int zVert = 0; zVert <= 1; zVert++)
                                {
                                    for (int xVert = 0; xVert <= 1; xVert++)
                                    {
                                        xPos = (tempTile.CenterPos.x - (tileSize / 2.0f)) + (xVert * (tileSize));
                                        zPos = (tempTile.CenterPos.z - (tileSize / 2.0f)) + (zVert * (tileSize));
                                        tempTileVert = new float3(xPos, 0, zPos);

                                        tempVerticeList.Add(tempTileVert);
                                        tempTile.verticesIndicies.Add(tempVerticeList.Count - 1);
                                    }
                                }

                                ////GENERATE TRIANGLES OF MAP TILE
                                tempTriangleList.Add(Convert.ToUInt16(tempVerticeList.Count + 0 - 4));
                                tempTriangleList.Add(Convert.ToUInt16(tempVerticeList.Count + 1 - 4));
                                tempTriangleList.Add(Convert.ToUInt16(tempVerticeList.Count + 2 - 4));
                                tempTriangleList.Add(Convert.ToUInt16(tempVerticeList.Count + 1 - 4));
                                tempTriangleList.Add(Convert.ToUInt16(tempVerticeList.Count + 3 - 4));
                                tempTriangleList.Add(Convert.ToUInt16(tempVerticeList.Count + 2 - 4));

                                //HORIZONTAL TRIANGLES OF MAP-TILE-JOINTS
                                float2 prevTile = new float2(tempTile.index.x - 1, tempTile.index.y);
                                if (tileIndicies.ContainsKey(prevTile))
                                {
                                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[prevTile].verticesIndicies[3]));
                                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[prevTile].verticesIndicies[1]));
                                    tempTriangleList.Add(Convert.ToUInt16(tempTile.verticesIndicies[0]));
                                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[prevTile].verticesIndicies[3]));
                                    tempTriangleList.Add(Convert.ToUInt16(tempTile.verticesIndicies[0]));
                                    tempTriangleList.Add(Convert.ToUInt16(tempTile.verticesIndicies[2]));
                                }

                                ////Vertical TRIANGLES OF MAP-TILE-JOINTS
                                prevTile = new float2(tempTile.index.x, tempTile.index.y - 1);
                                if (tileIndicies.ContainsKey(prevTile))
                                {
                                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[prevTile].verticesIndicies[2]));
                                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[prevTile].verticesIndicies[3]));
                                    tempTriangleList.Add(Convert.ToUInt16(tempTile.verticesIndicies[0]));
                                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[prevTile].verticesIndicies[3]));
                                    tempTriangleList.Add(Convert.ToUInt16(tempTile.verticesIndicies[1]));
                                    tempTriangleList.Add(Convert.ToUInt16(tempTile.verticesIndicies[0]));

                                    
                                }
                            }
                        }
                        //ONE TILE CREATED
                    }
                    //ALL 4 TILES CRAETEd AROUND CROSSJOINT

                    //CREATE TRIANGLES FOR CROSSJOINTS
                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[tempTile.index].verticesIndicies[0]));
                    tempTriangleList.Add(Convert.ToUInt16(tempCrossJoint.index));
                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[new float2(tempTile.index.x, tempTile.index.y - 1)].verticesIndicies[2]));

                    tempTriangleList.Add(Convert.ToUInt16(tempCrossJoint.index));
                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[tempTile.index].verticesIndicies[0]));
                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[new float2(tempTile.index.x - 1, tempTile.index.y)].verticesIndicies[1]));

                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[new float2(tempTile.index.x - 1, tempTile.index.y - 1)].verticesIndicies[3]));
                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[new float2(tempTile.index.x, tempTile.index.y - 1)].verticesIndicies[2]));
                    tempTriangleList.Add(Convert.ToUInt16(tempCrossJoint.index));

                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[new float2(tempTile.index.x - 1, tempTile.index.y - 1)].verticesIndicies[3]));
                    tempTriangleList.Add(Convert.ToUInt16(tempCrossJoint.index));
                    tempTriangleList.Add(Convert.ToUInt16(tileIndicies[new float2(tempTile.index.x - 1, tempTile.index.y)].verticesIndicies[1]));
                }
                //SINGLE CROSSJOINT CREATED
            }
            //MAP FINNISHED

            for (int i = 0; i < tempVerticeList.Count; i++)
            {
                tempNormalList.Add(new float3(random.Next(-1, 2), random.Next(1, 2), random.Next(-1, 2)));
            }

            //Add generated components to Mesh
            meshComp.Vertices = tempVerticeList.ToArray();
            meshComp.Triangles = tempTriangleList.ToArray();
            meshComp.Normals = tempNormalList.ToArray();
            
            mapScene.Components.Add(meshComp);
        }
    }


}
































//ALtes Mesh Generation
//MapTile tile = new MapTile(new float2(xTile, yTile));

//                    //Generating 4 Vertices for every corner of a mapTile
//                    for (int yVert = 0; yVert <= 1; yVert++)
//                    {
//                        for (int xVert = 0; xVert <= 1; xVert++)
//                        {
//                            Debug.WriteLine("X: " + xVert + " <-> Y: " + yVert);
//                            float xPos = ((xTile + xVert) * tileSize) + (xTile * jointSize);
//float zPos = (yTile * tileSize) + ((yTile + yVert) * jointSize);
//verticePoint = new float3(xPos, 0, zPos);
//tempVerticeList.Add(verticePoint);
//                            tile.verticesIndicies.Add(tempVerticeList.Count - 1);
//                        }
//                    }

//                    Debug.WriteLine("TILECOUNT: " + tileCount* 4 + " <-> " + "INDICIES: " + tempVerticeList.Count);

//                    //BASE TRIANGLES OF MAP TILE
//                    tempTriangleList.Add(Convert.ToUInt16((tileCount* 4)));
//                    tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 1)));
//                    tempTriangleList.Add(Convert.ToUInt16((tileCount* 4) + 2));
//                    tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 1)));
//                    tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 3)));
//                    tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 2)));

//                    //TRIANGLES OF MAP-TILE-JOINTS
//                    if (xTile<mapSize.x && yTile< (mapSize.y - 1))
//                    {
//                        //HORIZONTALS
//                        tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 3)));
//                        tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 4)));
//                        tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 2)));
//                        tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 3)));
//                        tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 5)));
//                        tempTriangleList.Add(Convert.ToUInt16(((tileCount* 4) + 4)));


//                    }

//                    if (xTile< (mapSize.x - 1) && yTile<mapSize.y)
//                    {
//                        //VERTICALS
//                        //tempTriangleList.Add(Convert.ToUInt16((((tileCount) * 4) + 1)));
//                        //tempTriangleList.Add(Convert.ToUInt16((((tileCount + mapSize.x) * 4) + 2)));
//                        //tempTriangleList.Add(Convert.ToUInt16((((tileCount) * 4) + 3)));
//                        //tempTriangleList.Add(Convert.ToUInt16((((tileCount) * 4) + 1)));
//                        //tempTriangleList.Add(Convert.ToUInt16((((tileCount + mapSize.x) * 4) + 0)));
//                        //tempTriangleList.Add(Convert.ToUInt16((((tileCount + mapSize.x) * 4) + 2)));
//                    }

//                    tileIndicies.Add(xTile + "." + yTile, tile);

//                    tileCount++;