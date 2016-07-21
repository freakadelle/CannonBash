using System;
using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Tutorial.Core.Assets;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{

    static class MapGenerator
    {
        public static Dictionary<float2, MapTile> tileIndicies;

        public static float2 minMaxHeight = new float2(999, 0);
        //Todo: Mapsize ist noch nicht allgmeeingültig um verschiedene Größen der Map zu generieren
        private static float mapSize;
        public static float2 mapUnits;
        private static int _mapTextureId, _skyTextureId;

        //MAP GENERATION SETTINGS
        public static float2 gridSize = new float2(100, 100);
        public const float tileSize = 15, jointSize = 3f;
        private static float2 nonPlayableAreaBounds;



        //RETURNS A PLANE MAP
        public static SceneNodeContainer instantiatePlaneMap()
        {
            tileIndicies = new Dictionary<float2, MapTile>();

            //INIT MAP
            SceneNodeContainer mapScene = new SceneNodeContainer();
            mapScene.Children = new List<SceneNodeContainer>();
            mapScene.Components = new List<SceneComponentContainer>();

            mapSize = (jointSize + tileSize) * (gridSize.x + gridSize.y) / 200.0f;
            mapUnits = new float2((jointSize + tileSize) * gridSize.x, (jointSize + tileSize) * gridSize.y);
            nonPlayableAreaBounds = new float2((int) (gridSize.x / 5.0f), (int) (gridSize.y / 5.0f));

            //ADD MAP COMPONENTS
            mapScene.Components.Add(generateTransform());
            mapScene.Components.Add(generateMaterial());
            mapScene.Components.Add(generatePlainMapMesh());

            mapScene.Children.Add(generateSky());

            return mapScene;
        }

        //MANIPULATES ALL MAPTILES IN A RANDOM VERTICAL DIRECTION
        public static void addHeightNoise(float minHeight, float maxHeight, bool _isRenderTime = false)
        {
            float2 minMaxNoise = new float2(minHeight, maxHeight);

            //Random MapHeight Generation
            foreach (KeyValuePair<float2, MapTile> entry in tileIndicies)
            {
                translateTile(entry.Value, new float3(0, (float)(minMaxNoise.x + (Constants.random.NextDouble() * minMaxNoise.y)), 0), _isRenderTime);
            }
        }

        //GENERATES A SINGLE HILL
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
                        translateTile(tempTile, new float3(0, height - ((heightLvl / _boundaries.x) * height), 0), _isRenderTime);
                    }
                }
            }
        }

        //GENERATE A NUMBER OF HILLS
        public static void generateTerrain(int _numberOfHills)
        {
            for (int i = 0; i < _numberOfHills; i++)
            {
                float2 randTileGrid = randomGrid();
                float randHillHeight = (float)(mapSize + Constants.random.NextDouble() * mapSize) * 2f;
                float randBound = (float)(mapSize + Constants.random.NextDouble() * mapSize) / (tileSize / 10.0f);

                createHillAt(randTileGrid, randHillHeight, new float2(randBound, randBound));
            }
        }

        //RASTERIZE THE MAP AND RETURNS A LIST OF ZENITS OF THE SPECIFIC RASTERS
        public static List<MapTile> gridMapReturnZenitTiles(int _grids)
        {
            List<MapTile> zenits = new List<MapTile>();

            float2 _gridSize;
            _gridSize.x = (int)((gridSize.x + 1 - (nonPlayableAreaBounds.x * 2)) / _grids);
            _gridSize.y = (int)((gridSize.y + 1 - (nonPlayableAreaBounds.y * 2)) / _grids);

            for (int x = 0; x < _grids; x++)
            {
                for (int y = 0; y < _grids; y++)
                {
                    float2 _start = new float2(nonPlayableAreaBounds.x + (_gridSize.x * x), nonPlayableAreaBounds.y + (_gridSize.y * y));
                    float2 _end = new float2(nonPlayableAreaBounds.x + (_gridSize.x * (x + 1)), nonPlayableAreaBounds.y + (_gridSize.y * (y + 1)));
                    zenits.Add(getHighestTileInArea(_start, _end));
                }
            }

            return zenits;
        }

        //TRANSLATES A TILE AND ADJUST ITS NEIGHBORS
        public static void translateTile(MapTile tile, float3 translation, bool _isRenderTime)
        {

            //TRANSLATE SINGLE MAP TILE
            foreach (int index in tile.verticesIndicies)
            {
                if (_isRenderTime)
                {
                    Renderer.translateVerticesOfMesh(SceneManager.rootNodes["mapRoot"].GetMesh(), index, translation);
                }
                else
                {
                    translateVertice(index, translation);
                }
            }

            //Todo: CenterPos of tile adjusts only height but not x and z axis
            tile.CenterPos = new float3(tile.CenterPos.x, SceneManager.rootNodes["mapRoot"].GetMesh().Vertices[tile.verticesIndicies[0]].y, tile.CenterPos.z);
            minMaxHeight = new float2(System.Math.Min(minMaxHeight.x, tile.CenterPos.y), System.Math.Max(minMaxHeight.y, tile.CenterPos.y));

            //ADJSUT CROSSJOINTS HEIGHT
            foreach (int index in tile.neighborJointsIndicies)
            {
                if (_isRenderTime)
                {
                    Renderer.translateVerticesOfMesh(SceneManager.rootNodes["mapRoot"].GetMesh(), index, translation * 0.25f);
                }
                else
                {
                    translateVertice(index, translation * 0.25f);
                }
            }
        }

        private static void translateVertice(int _vertInd, float3 translation)
        {
            float3 vertice = SceneManager.rootNodes["mapRoot"].GetMesh().Vertices[_vertInd];
            vertice = new float3(vertice.x + translation.x, vertice.y + translation.y, vertice.z + translation.z);
            SceneManager.rootNodes["mapRoot"].GetMesh().Vertices[_vertInd] = vertice;
        }

        private static float distanceBetweenGrids(float2 _grid1, float2 _grid2)
        {
            float a = System.Math.Abs(_grid1.x - _grid2.x);
            float b = System.Math.Abs(_grid1.y - _grid2.y);
            return (float) System.Math.Sqrt((a * a) + (b * b));
        }

        private static float2 randomGridInPlayableArea()
        {
            float2 randTileGrid = new float2(
                    Constants.random.Next((int) nonPlayableAreaBounds.x, (int)(gridSize.x + 1) - (int) nonPlayableAreaBounds.x),
                    Constants.random.Next((int) nonPlayableAreaBounds.y, (int)(gridSize.y + 1) - (int) nonPlayableAreaBounds.y)
            );
            
            return randTileGrid;
        }

        private static float2 randomGrid()
        {
            float2 randTileGrid = new float2(
                    Constants.random.Next(0, (int)(gridSize.x + 1)),
                    Constants.random.Next(0, (int)(gridSize.y + 1))
            );

            return randTileGrid;
        }

        private static MapTile getHighestTileInArea(float2 _start, float2 _end)
        {
            MapTile _zenitTile = new MapTile(new float2(-1, -1));
            _zenitTile.CenterPos = new float3(-1, -1, -1);

            float2 diff = new float2(System.Math.Abs(_start.x - _end.x), System.Math.Abs(_start.y - _end.y));

            for (int x = 0; x < diff.x; x++)
            {
                for (int y = 0; y < diff.y; y++)
                {
                    float2 _curTile = new float2(_start.x + x, _start.y + y);
                    float _curTileHeight = tileIndicies[_curTile].CenterPos.y;

                    if (_curTileHeight > _zenitTile.CenterPos.y)
                    {
                        _zenitTile = tileIndicies[_curTile];
                    }
                }
            }

            return _zenitTile;
        }

        public static void nextTexture()
        {
            _mapTextureId++;
            _skyTextureId++;

            if(_mapTextureId >= AssetsManager.TEXTURE_MAP_FILES.Count)
            {
                _mapTextureId = 0;
            }

            string texture = AssetsManager.textures[AssetsManager.TEXTURE_MAP_FILES[_mapTextureId]].path;
            SceneManager.rootNodes["mapRoot"].GetMaterial().Diffuse.Texture = texture;

            if (_skyTextureId >= AssetsManager.TEXTURE_SKY_FILES.Count)
            {
                _skyTextureId = 0;
            }

            texture = AssetsManager.textures[AssetsManager.TEXTURE_SKY_FILES[_skyTextureId]].path;
            SceneManager.rootNodes["mapRoot"].Children[0].GetMaterial().Diffuse.Texture = texture;
        }

        //----------------------------------------------------------------------------------------------------
        //COMPONENT GENERATION METHODS
        //----------------------------------------------------------------------------------------------------

        /*
        * Generates a transform copmonent
        */
        private static TransformComponent generateTransform()
        {
            TransformComponent transComp = new TransformComponent();
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;
            transComp.Translation = float3.Zero;

            return transComp;
        }

        /*
        * Generates a material copmonent
        */
        private static MaterialComponent generateMaterial()
        {
            MaterialComponent matComp = new MaterialComponent();

            matComp.Diffuse = new MatChannelContainer();
            matComp.Diffuse.Color = new float3(1f, 1f, 1f);
            matComp.Diffuse.Mix = 1f;
            matComp.Diffuse.Texture = AssetsManager.textures[AssetsManager.TEXTURE_MAP_FILES[_mapTextureId]].path;

            matComp.Emissive = new MatChannelContainer();
            matComp.Emissive.Color = new float3(1, 1, 0.5f);
            matComp.Emissive.Mix = 0.15f;

            matComp.Specular = new SpecularChannelContainer();
            matComp.Specular.Color = new float3(1,1,0.75f);
            matComp.Specular.Intensity = 0.15f;
            matComp.Specular.Shininess = 1f;
            matComp.Specular.Mix = 1f;

            return matComp;
        }

        /*
        * Generates a Meshcomponent. Plane Mesh
        */
        private static MeshComponent generatePlainMapMesh()
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
                tempNormalList.Add(new float3((float)Constants.random.NextDouble() * 0.2f, 1, (float)Constants.random.NextDouble() * 0.2f));
            }

            tempUVsList.Add(new float2(0f, 0f));
            tempUVsList.Add(new float2(1, 1));

            //Add generated components to Mesh
            meshComp.Vertices = tempVerticeList.ToArray();
            meshComp.Triangles = tempTriangleList.ToArray();
            meshComp.Normals = tempNormalList.ToArray();
            meshComp.UVs = tempUVsList.ToArray();

            return meshComp;
        }

        private static SceneNodeContainer generateSky()
        {
            SceneNodeContainer skyScene = AssetsManager.fusFiles["360Sky"];

            //SET SKY TEXTURE
            skyScene.GetMaterial().Diffuse.Texture = AssetsManager.textures[AssetsManager.TEXTURE_SKY_FILES[_skyTextureId]].path;

            //TRANSLATE SKY TO CENTRE OF MAP
            float2 centerTileGrid = new float2((int)((gridSize.x) / 2.0f), (int)((gridSize.y) / 2.0f));
            skyScene.GetTransform().Translation = new float3(tileIndicies[centerTileGrid].CenterPos);

            //SCALE SKY TO PROPER SIZE
            float skyScale = mapSize * 0.5f;
            skyScene.GetTransform().Scale = new float3(skyScale, skyScale, skyScale);

            return skyScene;
        }
    }
}