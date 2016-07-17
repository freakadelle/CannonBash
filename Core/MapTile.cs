using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Tutorial.Core.Assets;

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

        //GETTER SETTER
        public float3 CenterPos
        {
            get
            {
                return centerPos;
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
