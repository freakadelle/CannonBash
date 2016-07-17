using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Tutorial.Core.Assets;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{
    class Projectile
    {

        private int id;
        private float weight;
        public float3 velocity;
        //public float2 tilePos;
        public SceneNodeContainer container;
        public TransformComponent transform;

        //public Projectile(int _id, float3 _pos, float2 _tilePos)
        public Projectile(int _id, float3 _pos)
        {
            id = _id;
            weight = 1;
            velocity = float3.One;
            //tilePos = _tilePos;

            //GET A CLONE OF A PROJECTILE
            container = AssetsManager.projectile.cloneContainer(_id);

            transform = container.GetTransform();
            transform.Translation = _pos;
            transform.Scale = float3.One * Constants.PROJECTILE_SCALE;
        }

        public void update()
        {
            transform.Translation = new float3(transform.Translation.x + velocity.x, transform.Translation.y + velocity.y, transform.Translation.z + velocity.z);
            velocity = new float3(velocity.x, velocity.y + (Constants.GRAVITY * weight), velocity.z);
        }

        public MapTile isCollided()
        {
            float rawGridX = transform.Translation.x/(MapGenerator.jointSize + MapGenerator.tileSize);
            float rawGridY = transform.Translation.z/(MapGenerator.jointSize + MapGenerator.tileSize);

            float2 tileGridFloor = new float2((int) System.Math.Floor(rawGridX), (int) System.Math.Floor(rawGridY));
            float2 tileGridCeil = new float2((int) System.Math.Floor(rawGridX), (int) System.Math.Floor(rawGridY));

            if (MapGenerator.tileIndicies.ContainsKey(tileGridFloor) && transform.Translation.y <= MapGenerator.tileIndicies[tileGridFloor].CenterPos.y)
            {
                return MapGenerator.tileIndicies[tileGridFloor];
            } else if (MapGenerator.tileIndicies.ContainsKey(tileGridCeil) && transform.Translation.y <= MapGenerator.tileIndicies[tileGridCeil].CenterPos.y)
            {
                return MapGenerator.tileIndicies[tileGridCeil];
            }

            return null;
        }

        public bool isOutOfMap()
        {
            if (transform.Translation.y < 0)
            {
                return true;
            }
            return false;
        }
    }
}
