using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Tutorial.Core.Assets;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{
    class MapTile: SceneNodeContainer
    {
        public float2 index;
        private float3 centerPos;
        public Bunker mountedBunker;
        public List<MapTile> neighborMapTiles;
        public List<int> verticesIndicies;
        public List<int> neighborJointsIndicies;

        public MapTile(float2 _index)
        {
            mountedBunker = null;

            index = _index;

            Components = new List<SceneComponentContainer>();
            Children = new List<SceneNodeContainer>();

            neighborJointsIndicies = new List<int>();
            verticesIndicies = new List<int>();

            //pos = _pos;
            neighborMapTiles = new List<MapTile>();

            Name = "Tile_" + index.x + "-" + index.y;
        }

        public void addTransformComponent()
        {
            TransformComponent transComp = new TransformComponent();
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;
            transComp.Translation = float3.Zero;

            Components.Add(transComp);
        }

        public void addMaterialComponent()
        {
            MaterialComponent matComp = new MaterialComponent();

            matComp.Diffuse = new MatChannelContainer();
            matComp.Diffuse.Color = new float3(1, 0.5f, 0.5f);
            matComp.Diffuse.Mix = 1;

            matComp.Emissive = new MatChannelContainer();
            matComp.Emissive.Color = new float3(1, 0.5f, 0.5f);
            matComp.Emissive.Mix = 1;

            matComp.Specular = new SpecularChannelContainer();
            matComp.Specular.Color = float3.One;
            matComp.Specular.Intensity = 0.3f;
            matComp.Specular.Mix = 1;
            matComp.Specular.Shininess = 100;

            Components.Add(matComp);
        }

        public void addMeshComponent()
        {
            MeshComponent meshComp = new MeshComponent();
            meshComp.BoundingBox = new AABBf(float3.Zero, float3.Zero);

            meshComp.Vertices = new[]
            {
                new float3(0, 0, 0),
                new float3(1, 0, 0),
                new float3(0, 0, 1),
                new float3(1, 0, 1),
            };

            meshComp.Triangles = new ushort[]
            {
                0, 1, 2,
                2, 1, 3
            };

            meshComp.Normals = new float3[]
            {
                new float3(0, 1, 0),
                new float3(0, 1, 0),
                new float3(0, 1, 0),
                new float3(0, 1, 0),
            };

            Components.Add(meshComp);
        }

        //GETTER SETTER
        public float3 CenterPos
        {
            get
            {
                return centerPos;
                //float3 ul = MapGenerator.mapScene.GetMesh().Vertices[verticesIndicies[0]];

                //centerPos = new float3(ul.x + (MapGenerator.tileSize/2.0f), ul.y, ul.z + (MapGenerator.tileSize / 2.0f));
                //return centerPos;
            }
            set
            {
                centerPos = value;
                if (mountedBunker != null)
                {
                    mountedBunker.bunkerBase.Translation = centerPos;
                }
            }
        }
    }

}
