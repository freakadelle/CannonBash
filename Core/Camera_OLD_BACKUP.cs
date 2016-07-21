//using Fusee.Math.Core;
//using Fusee.Serialization;
//using Fusee.Tutorial.Core.Assets;

//namespace Fusee.Tutorial.Core
//{
//    class Camera: TransformComponent
//    {
//        // angle variables
//        private float _angleHorz = M.PiOver6 * 2.0f, _angleVert = -M.PiOver6 * 0.5f, _curDamp,
//                             _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit, _zoomVel, _fieldOfView;
//        private float2 _offset, _offsetInit, _zoomLimits;

//        public float4x4 projection;
//        private readonly float RotationSpeed = 7;
//        public readonly float Damping = 0.8f;

//        public float3 eye, target, up, pivot;

//        public Camera()
//        {
//            Translation = new float3(0,0,0);
//            Rotation = new float3(0,0,0);
//            Scale = new float3(0,0,0);
//            up = new float3(0, 1, 0);
//            target = new float3(50, 50, 50);
//            pivot = new float3(1,-1.8f,-2.7f);
//            _fieldOfView = M.PiOver4;
//            _zoomLimits = new float2(1, 100000);
//        }

//        public bool touchZoom(float _twoPointAngle, float2 _twoPointMidPoint, float _twoPointDistanceVel, bool _twoTouchRepeated)
//        {
//            if (!_twoTouchRepeated)
//            {
//                _angleRollInit = _twoPointAngle - _angleRoll;
//                _offsetInit = _twoPointMidPoint - _offset;
//            }
//            _zoomVel = _twoPointDistanceVel * -0.01f;
//            _angleRoll = _twoPointAngle - _angleRollInit;
//            _offset = _twoPointMidPoint - _offsetInit;

//            return true;
//        }

//        public bool mouseWheelZoom(float wheelVel)
//        {
//            _zoomVel = wheelVel * -2.0f;
//            _angleRoll *= _curDamp * 0.8f;
//            _offset *= _curDamp * 0.8f;

//            return false;
//        }

//        public void rotate(float2 vel)
//        {
//            _angleVelHorz = -RotationSpeed * vel.x * 0.000002f;
//            _angleVelVert = -RotationSpeed * vel.y * 0.000002f;
//        }

//        public void rotateDump()
//        {
//            _angleVelHorz *= _curDamp;
//            _angleVelVert *= _curDamp;
//        }

//        public void update()
//        {
//            Zoom += _zoomVel;

//            _angleHorz += _angleVelHorz;
//            // Wrap-around to keep _angleHorz between -PI and + PI
//            _angleHorz = M.MinAngle(_angleHorz);

//            _angleVert += _angleVelVert;
//            // Limit pitch to the range between [-PI/2, + PI/2]
//            _angleVert = M.Clamp(_angleVert, -M.PiOver2, M.PiOver2);

//            // Wrap-around to keep _angleRoll between -PI and + PI
//            _angleRoll = M.MinAngle(_angleRoll);
//        }

//        public void mountCameraOnBunker(Bunker _bunker)
//        {
//            //Todo: Warum muss die Translation negativ genommen werden?
//            Translation = -_bunker.bunkerBase.Translation - (_bunker.bunkerPlatform.Translation * Constants.BUNKER_SCALE * 1.1f);
//        }

//        //GETTER SETTER
//        //-------------------------------------------------

//        public float CurDamp
//        {
//            get { return _curDamp; }
//            set
//            {
//                _curDamp = value;
//            }
//        }


//        public float Zoom
//        {
//            get
//            {
//                return eye.z;
//            }
//            set
//            {
//                eye.z = value;
//                eye.z = System.Math.Max(_zoomLimits.x, eye.z);
//                eye.z = System.Math.Min(_zoomLimits.y, eye.z);
//            }
//        }

//        public float4x4 MtxRot
//        {
//            get
//            {
//                return float4x4.CreateRotationZ(Rotation.z) * float4x4.CreateRotationX(Rotation.x) * float4x4.CreateRotationY(Rotation.y);
//            }
//        }

//        public float4x4 MtxPivot
//        {
//            get
//            {
//                return float4x4.CreateTranslation(pivot.x, pivot.y, pivot.z);
//            }
//        }

//        public float4x4 MtxCam
//        {
//            get
//            {
//                return float4x4.LookAt(-eye, target, up);
//            }
//        }

//        public float4x4 MtxOffset
//        {
//            get
//            {
//                return float4x4.CreateTranslation(Translation.x, Translation.y, Translation.z);
//            }
//        }

//        public float FieldOfView
//        {
//            get { return _fieldOfView; }
//            set { _fieldOfView = value; }
//        }
//    }
//}
