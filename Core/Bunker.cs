using System.Linq;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core.Assets
{

    class Bunker: SceneContainer
    {

        public string name;
        public MapTile tile;

        public int ammo;

        public SceneNodeContainer scene;

        public TransformComponent bunkerBase;
        public TransformComponent bunkerPlatform;
        public TransformComponent bunkerCannon;

        public float shootForce;
        private float _rotateSpeed;

        public Bunker(string _name)
        {
            name = _name;

            scene = AssetsManager.fusFiles[name];

            bunkerBase = scene.FindNodes(c => c.Name == "Base_" + name).First()?.GetTransform();
            bunkerPlatform = scene.FindNodes(c => c.Name == "Turn_" + name).First()?.GetTransform();
            bunkerCannon = scene.FindNodes(c => c.Name == "CannonRohr_" + name).First()?.GetTransform();

            bunkerBase.Scale = float3.One * Constants.BUNKER_SCALE;

            shootForce = 10;
            ammo = 1;
            _rotateSpeed = 0.0001f;
        }

        public void rotatePlatform(float _amount)
        {
            bunkerPlatform.Rotation = new float3(bunkerPlatform.Rotation.x, bunkerPlatform.Rotation.y + (_amount * _rotateSpeed), bunkerPlatform.Rotation.z);
        }

        public void liftCannon(float _amount)
        {
            float liftHeight = bunkerCannon.Rotation.z + (_amount*_rotateSpeed);
            liftHeight = System.Math.Max(liftHeight, 0.8f);
            liftHeight = System.Math.Min(liftHeight, 2.0f);
            bunkerCannon.Rotation = new float3(bunkerCannon.Rotation.x, bunkerCannon.Rotation.y, liftHeight);
        }

        public Projectile shootProjectile()
        {
            float3 projPos = bunkerBase.Translation + (bunkerPlatform.Translation*Constants.BUNKER_SCALE*1.1f) + new float3(0, 0, 0);
            Projectile proj = new Projectile(Constants.projectile_Count, projPos);
            Constants.projectile_Count++;

            float2 shootDirection = Constants.angleToVector(bunkerPlatform.Rotation.y + (float)System.Math.PI);
            float2 shootHeight = Constants.angleToVector(bunkerCannon.Rotation.z + (float)System.Math.PI);

            proj.velocity = new float3(shootDirection.x, -shootHeight.x, shootDirection.y);
            proj.velocity = float3.Normalize(proj.velocity) * shootForce;

            return proj;
        }

        public void mountBunkerOnTile(MapTile _tile)
        {
            bunkerBase.Translation = _tile.CenterPos;
            tile = _tile;
            tile.mountedBunker = this;
        }
    }
}
