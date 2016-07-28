using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Tutorial.Core.Assets;

namespace Fusee.Tutorial.Core
{
    class Camera : TransformComponent
    {
        public float4x4 projection;
        public float _angleVelHorz, _angleVelVert, _angleRollInit, _fieldOfView, _curDamp;
        private readonly float2 _zoomLimits;
        public readonly float RotationSpeed = 7, Damping = 0.8f;
        public float3 eye, target, up, pivot;

        public Camera()
        {
            Translation = float3.Zero;
            Rotation = float3.Zero;
            Scale = float3.One;

            up = float3.One;
            target = float3.One;
            eye = float3.One;
            pivot = float3.Zero;

            _fieldOfView = M.PiOver4;
            _zoomLimits = new float2(1, 100000);
        }

        public bool mouseWheelZoom(float wheelVel)
        {
            Zoom += wheelVel * -2.0f;
            return false;
        }

        public void rotate(float2 vel)
        {
            _angleVelHorz = -RotationSpeed * vel.x * 0.000002f;
            _angleVelVert = -RotationSpeed * vel.y * 0.000002f;
        }

        public void rotateDump()
        {
            _angleVelHorz *= _curDamp;
            _angleVelVert *= _curDamp;
        }

        public void update()
        {
            Rotation.y += _angleVelHorz;
            // Wrap-around to keep _angleHorz between -PI and + PI
            Rotation.y = M.MinAngle(Rotation.y);

            Rotation.x += _angleVelVert;
            // Limit pitch to the range between [-PI/2, + PI/2]
            Rotation.x = M.Clamp(Rotation.x, -M.PiOver2, M.PiOver2);

            // Wrap-around to keep _angleRoll between -PI and + PI
            Rotation.z = M.MinAngle(Rotation.z);
        }

        public void mountCameraOnBunker(Bunker _bunker)
        {
            Translation = -_bunker.bunkerBase.Translation - (_bunker.bunkerPlatform.Translation * Constants.BUNKER_SCALE * 1.1f);
        }

        public void setCurDamp(float _deltaTime)
        {
            _curDamp = (float)System.Math.Exp(-Damping * _deltaTime);
        }

        //GETTER SETTER
        //-------------------------------------------------

        private float Zoom
        {
            get { return eye.z; }
            set
            {
                eye.z = value;
                eye.z = System.Math.Max(_zoomLimits.x, eye.z);
                eye.z = System.Math.Min(_zoomLimits.y, eye.z);
            }
        }

        public float4x4 MtxRot
        {
            get
            {
                return float4x4.CreateRotationZ(Rotation.z) * float4x4.CreateRotationX(Rotation.x) * float4x4.CreateRotationY(Rotation.y);
            }
        }

        public float4x4 MtxPivot
        {
            get
            {
                return float4x4.CreateTranslation(pivot.x, pivot.y, pivot.z);
            }
        }

        public float4x4 MtxCam
        {
            get
            {
                return float4x4.LookAt(-eye, target, up);
            }
        }

        public float4x4 MtxOffset
        {
            get
            {
                return float4x4.CreateTranslation(Translation.x, Translation.y, Translation.z);
            }
        }

        public float FieldOfView
        {
            get { return _fieldOfView; }
            set { _fieldOfView = value; }
        }

    }
}
