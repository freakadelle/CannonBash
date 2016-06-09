using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{

    static class MapGenerator
    {

        public static float2 mapSize = float2.One;

        public static SceneNodeContainer generate()
        {
            SceneNodeContainer tileMap = new SceneNodeContainer();
            tileMap.Components = new List<SceneComponentContainer>();
            tileMap.Children = new List<SceneNodeContainer>();
            
            TransformComponent transComp = new TransformComponent();
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;
            transComp.Translation = float3.One;

            MeshComponent meshComp = new MeshComponent();
            meshComp.BoundingBox = new AABBf(float3.Zero, float3.Zero);

            tileMap.Components.Add(transComp);
            tileMap.Components.Add(meshComp);

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    tileMap.Children.Add(new MapTile("tile." + x + "." + y, new float3(1.2f * x, 0, 1.2f * y)));
                }
            }

            tileMap.Name = "TileMap";

            return tileMap;
        }
    }
}
