using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{
    class TexturedWall
    {
        public SceneNodeContainer wallScene;
        public TransformComponent transform;
        public string textureName;
        public string name;

        public TexturedWall(string _name, string _textureName)
        {
            textureName = _textureName;
            name = _name;
        }

        public SceneNodeContainer instantiateTexturedWall()
        {
            wallScene = new SceneNodeContainer();
            wallScene.Components = new List<SceneComponentContainer>();

            addTransformCoponent();
            addMaterialComponent();
            addMeshComponent();

            wallScene.Name = name;

            transform = wallScene.GetTransform();

            return wallScene;
        }

        public void addTransformCoponent()
        {
            TransformComponent transComp = new TransformComponent();
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;
            transComp.Translation = float3.Zero;

            wallScene.Components.Add(transComp);
        }

        public void addMaterialComponent()
        {
            MaterialComponent matComp = new MaterialComponent();

            matComp.Diffuse = new MatChannelContainer();
            matComp.Diffuse.Color = new float3(1f, 0f, 0f);
            matComp.Diffuse.Mix = 1f;
            matComp.Diffuse.Texture = textureName;

            matComp.Emissive = new MatChannelContainer();
            matComp.Emissive.Color = new float3(1, 1, 0.5f);
            matComp.Emissive.Mix = 0.15f;

            matComp.Specular = new SpecularChannelContainer();
            matComp.Specular.Color = new float3(1, 1, 0.75f);
            matComp.Specular.Intensity = 0.15f;
            matComp.Specular.Shininess = 1f;
            matComp.Specular.Mix = 1f;

            wallScene.Components.Add(matComp);
        }

        public void addMeshComponent()
        {
            MeshComponent meshComp = new MeshComponent();
            meshComp.BoundingBox = new AABBf(float3.Zero, float3.Zero);

            meshComp.Vertices = new[]
            {
                new float3(0, 0, 0), 
                new float3(1, 0, 0),
                new float3(0, 1, 0),
                new float3(1, 1, 0)
            };

            meshComp.Normals = new[]
            {
                new float3(0, 0, -1),
                new float3(0, 0, -1),
                new float3(0, 0, -1),
                new float3(0, 0, -1)
            };

            meshComp.Triangles = new ushort[]
            {
                0, 3, 2, 1, 3, 0
            };

            meshComp.UVs = new[]
            {
                new float2(0f, 0f),
                new float2(1f, 0.1f)
            };

            wallScene.Components.Add(meshComp);
        }
    }
}