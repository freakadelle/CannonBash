using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{

    static class MapGenerator
    {
        public static Random random = new Random();
        public static Dictionary<float2, MapTile> tileIndicies;
        public static SceneNodeContainer mapScene;
        public static SceneNodeContainer skyScene;

        public static float2 minMaxHeight = new float2(999, 0);
        public static float mapSize;
        public static List<string> textureMapFiles = new List<string>() { "mountainsTexture_0.png", "mountainsTexture_3.png", "mountainsTexture_4.png" };
        public static List<string> textureSkyFiles = new List<string>() { "sky_8.png", "sky_6.png", "sky_8.png" };

        //MAP GENERATION SETTINGS
        //Base Map Settings
        public static float2 gridSize = new float2(100, 100);
        public static float tileSize = 15f;
        public static float jointSize = 10f;
        public static int nonPlayableAreaBounds = 25;
        public static int activeTextureId = 1;

        public static SceneNodeContainer instantiatePlaneMap()
        {
            //INIT MAP
            mapScene = new SceneNodeContainer();
            mapScene.Children = new List<SceneNodeContainer>();
            mapScene.Components = new List<SceneComponentContainer>();
            tileIndicies = new Dictionary<float2, MapTile>();

            //ADD MAP COMPONENTS
            addTransformCoponent();
            addMaterialComponent();
            addMeshComponent();

            addSky();

            mapSize = (jointSize + tileSize) * (gridSize.x * gridSize.y) / 10000.0f;
            mapScene.Name = "TileMap";

            return mapScene;
        }

        /*
        * Translates a tile and adjusts the Crossjoint neighbors and the bunker on this tile
        */
        public static void translateTile(MapTile tile, float3 translation, bool _isRenderTime)
        {

            //TRANSLATE SINGLE MAP TILE
            foreach (int index in tile.verticesIndicies)
            {
                if (_isRenderTime)
                {
                    Renderer.translateVerticesOfMesh(mapScene.GetMesh(), index, translation);
                }
                else
                {
                    translateVertice(index, translation);
                }
            }

            //Todo: CenterPos of tile adjusts only height but not x and z axis
            tile.CenterPos = new float3(tile.CenterPos.x, mapScene.GetMesh().Vertices[tile.verticesIndicies[0]].y, tile.CenterPos.z);
            minMaxHeight = new float2(System.Math.Min(minMaxHeight.x, tile.CenterPos.y), System.Math.Max(minMaxHeight.y, tile.CenterPos.y));

            //ADJSUT CROSSJOINTS HEIGHT
            foreach (int index in tile.neighborJointsIndicies)
            {
                if (_isRenderTime)
                {
                    Renderer.translateVerticesOfMesh(mapScene.GetMesh(), index, translation * 0.25f);
                }
                else
                {
                    translateVertice(index, translation * 0.25f);
                }
            }
        }

        /*
        * Manipulates all Maptiles in vertical direction with a random value
        */
        public static void addHeightNoise(float minHeight, float maxHeight, bool _isRenderTime = false)
        {
            float2 minMaxNoise = new float2(minHeight, maxHeight);

            //Random MapHeight Generation
            foreach (KeyValuePair<float2, MapTile> entry in MapGenerator.tileIndicies)
            {
                translateTile(entry.Value, new float3(0, (float)(minMaxNoise.x + (random.NextDouble() * minMaxNoise.y)), 0), _isRenderTime);
            }
        }

        /*
        * Moves all single MapTiles in vertical Direction which are around the specified _gridPos.
        * Creates a mountain
        */
        public static void createHillAt(float2 _gridPos, float height, float2 _boundaries, bool _isRenderTime = false)
        {
            for (int x = (int)-_boundaries.x; x < (int)_boundaries.x; x++)
            {
                for (int y = (int)-_boundaries.y; y < (int)_boundaries.y; y++)
                {
                    float2 tilePos = new float2(_gridPos.x + x, _gridPos.y + y);

                    if (tileIndicies.ContainsKey(tilePos))
                    {
                        MapTile tempTile = tileIndicies[tilePos];
                        int heightLvl = System.Math.Max(System.Math.Abs(x), System.Math.Abs(y));
                        translateTile(tempTile, new float3(0, height - ((heightLvl/_boundaries.x) * height), 0), _isRenderTime);
                    }
                }
            }
        }

        private static void translateVertice(int _vertInd, float3 translation)
        {
            float3 vertice = mapScene.GetMesh().Vertices[_vertInd];
            vertice = new float3(vertice.x + translation.x, vertice.y + translation.y, vertice.z + translation.z);
            mapScene.GetMesh().Vertices[_vertInd] = vertice;
        }

        /*
        * Returns the distance between two grids
        */
        public static float distanceBetweenGrids(float2 _grid1, float2 _grid2)
        {
            float a = System.Math.Abs(_grid1.x - _grid2.x);
            float b = System.Math.Abs(_grid1.y - _grid2.y);
            return (float) System.Math.Sqrt((a * a) + (b * b));
        }

        /*
        * Returns a random grid within the playable area
        */
        public static float2 randomGridInPlayableArea()
        {
            float2 randTileGrid = new float2(
                    random.Next(nonPlayableAreaBounds, (int)(gridSize.x + 1) - nonPlayableAreaBounds),
                    random.Next(nonPlayableAreaBounds, (int)(gridSize.y + 1) - nonPlayableAreaBounds)
            );
            
            return randTileGrid;
        }

        /*
        * Returns a random grid within the map
        */
        public static float2 randomGrid()
        {
            float2 randTileGrid = new float2(
                    random.Next(0, (int)(gridSize.x + 1)),
                    random.Next(0, (int)(gridSize.y + 1))
            );

            return randTileGrid;
        }

        public static void nextTexture()
        {
            activeTextureId++;

            if(activeTextureId >= textureMapFiles.Count)
            {
                activeTextureId = 0;
            }

            mapScene.GetMaterial().Diffuse.Texture = "Textures/Landscape/" + textureMapFiles[activeTextureId];
            mapScene.Children[0].GetMaterial().Diffuse.Texture = "Textures/Sky/" + textureSkyFiles[activeTextureId];
        }

        //----------------------------------------------------------------------------------------------------
        //COMPONENT GENERATION METHODS
        //----------------------------------------------------------------------------------------------------

        /*
        * Generates a transform copmonent
        */
        public static void addTransformCoponent()
        {
            TransformComponent transComp = new TransformComponent();
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;
            transComp.Translation = float3.Zero;

            mapScene.Components.Add(transComp);
        }

        /*
        * Generates a material copmonent
        */
        public static void addMaterialComponent()
        {
            MaterialComponent matComp = new MaterialComponent();

            matComp.Diffuse = new MatChannelContainer();
            matComp.Diffuse.Color = new float3(1f, 1f, 1f);
            matComp.Diffuse.Mix = 1f;
            matComp.Diffuse.Texture = "Textures/Landscape/" + textureMapFiles[activeTextureId];
            //matComp.Diffuse.Texture = "Leaves.jpg";

            matComp.Emissive = new MatChannelContainer();
            matComp.Emissive.Color = new float3(1, 1, 0.5f);
            matComp.Emissive.Mix = 0.15f;

            matComp.Specular = new SpecularChannelContainer();
            matComp.Specular.Color = new float3(1,1,0.75f);
            matComp.Specular.Intensity = 0.15f;
            matComp.Specular.Shininess = 1f;
            matComp.Specular.Mix = 1f;

            mapScene.Components.Add(matComp);
        }

        /*
        * Generates a Meshcomponent. Plane Mesh
        */
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
            List<float2> tempUVsList = new List<float2>();
            MapTile tempTile = new MapTile(float2.Zero);

            //GENERATING BASE MAP TILES
            for (int xCrossJoint = 0; xCrossJoint < gridSize.x; xCrossJoint++)
            {
                for (int yCrossJoint = 0; yCrossJoint < gridSize.y; yCrossJoint++)
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

            //Add some random normals
            for (int i = 0; i < tempVerticeList.Count; i++)
            {
                //tempNormalList.Add(new float3(random.Next(-1, 2), random.Next(1, 2), random.Next(-1, 2)));
                tempNormalList.Add(new float3((float)random.NextDouble() * 0.5f, 1, (float)random.NextDouble()*0.5f));
            }

            tempUVsList.Add(new float2(0f, 0f));
            tempUVsList.Add(new float2(1, 1));

            //Add generated components to Mesh
            meshComp.Vertices = tempVerticeList.ToArray();
            meshComp.Triangles = tempTriangleList.ToArray();
            meshComp.Normals = tempNormalList.ToArray();
            meshComp.UVs = tempUVsList.ToArray();
            
            mapScene.Components.Add(meshComp);
        }

        private static void addSky()
        {
            //Add sky to MapScene
            mapScene.Children.Add(skyScene);

            //Transform sky to centre of map
            TransformComponent skyTransform = mapScene.Children.FindNodes(c => c.Name == "Sky").First()?.GetTransform();
            float2 centerTileGrid = new float2((int)((gridSize.x - 2) / 2.0f), (int)((gridSize.y - 2) / 2.0f));
            skyTransform.Translation = new float3(tileIndicies[centerTileGrid].CenterPos);

            //Scale Sky to proper size
            float skyScale = System.Math.Max(gridSize.x, gridSize.y) / 8.0f;
            skyTransform.Scale = new float3(skyScale, skyScale, skyScale);

            mapScene.Children[0].GetMaterial().Diffuse.Texture = "Textures/Sky/" + textureSkyFiles[activeTextureId];
        }

        public static void loadMapAssets()
        {
            skyScene = AssetStorage.Get<SceneContainer>("360Sky.fus").Children[0];
        }

    }
}