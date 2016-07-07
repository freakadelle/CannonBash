using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Tutorial.Core.Assets;
using Fusee.Xene;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Tutorial.Core
{

    /*
        Seltsam: Material des generierten SceneNodeContainer wird deaktiviert, wenn man ein fus. Objekt mit Cinema4D rein lädt
        Seltsam: Culling total verhauen bei c4D Fus Objekt

        Frage: Normalen defininieren von autogenerierter Landschaft. Normalen-Interpolationfür Rundungen
        Frage: Wie wird diese Kamera richtig bedient?!
        Frage: Mehrere Objekte mit unterschiedlichen Shadern. Materialien dem Shader zuspielen
    */

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        // angle variables
        private static float _angleHorz = M.PiOver6 * 2.0f, _angleVert = -M.PiOver6 * 0.5f,
                             _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit, _zoomVel, _zoom;
        private static float2 _offset;
        private static float2 _offsetInit;

        private const float RotationSpeed = 7;
        private const float Damping = 0.8f;

        private SceneContainer _scene;
        private float4x4 _sceneScale;
        private float4x4 _projection;
        private bool _twoTouchRepeated;

        public static string[] playerNames = { "Bunker_white", "Bunker_pink", "Bunker_yellow", "Bunker_green", "Bunker_blue", "Bunker_red" };
        public static int numberOfPlayers = 6;
        private List<Bunker> players;
        private bool _keys;

        private Renderer _renderer;

        Random random = new Random();

        // Init is called on startup. 
        public override void Init()
        {
            //Crate empty Scene
            _scene = new SceneContainer();
            _scene.Children = new List<SceneNodeContainer>();
            _scene.Header = new SceneHeader();

            //Map Generator Settings
            MapGenerator.tileSize = 10;
            MapGenerator.jointSize = 10f;

            //Generate Map
            SceneNodeContainer map = MapGenerator.instantiatePlaneMap(15, 35);

            //Init Amount of player bunkers
            loadPlayers(numberOfPlayers);

            MapGenerator.addHeightNoise(0, 10);

            for (int i = 0; i < numberOfPlayers; i++)
            {
                
                float2 randTileGrid = new float2(random.Next(0, (int) MapGenerator.mapSize.x), random.Next(0, (int)MapGenerator.mapSize.y));
                float2 randHillBound = new float2(random.Next(3, 6), random.Next(3, 6));
                float randBound = random.Next(5, 9);
                MapGenerator.createHillAt(randTileGrid, (float) (10 + random.NextDouble() * 75), new float2(10, 10), new float2(randBound, randBound));

                //Add player on Hill
                mountBunkerOnTile(MapGenerator.tileIndicies[randTileGrid], players[i]);

                _scene.Children.Add(players[i].scene.Children[0]);
            }

            //Add Map to scene
            _scene.Children.Add(map);

            

            ////Random Position for each bunker and add to scene
            //foreach (var player in players)
            //{
            //    float2 randTileIndex;
            //    MapTile matchingTile;

            //    do
            //    {
            //        randTileIndex = new float2(random.Next(0, (int)MapGenerator.mapSize.x), random.Next(0, (int)MapGenerator.mapSize.y));
            //        matchingTile = MapGenerator.tileIndicies[randTileIndex];
            //    } while (!mountBunkerOnTile(matchingTile, player));

            //    _scene.Children.Add(player.scene.Children[0]);
            //}
            
            _sceneScale = float4x4.CreateScale(1);
            
            //Instantiate Renderer and Backbuffer
            _renderer = new Renderer(RC);
            RC.ClearColor = new float4(0.1f, 0.1f, 0.1f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            var curDamp = (float)System.Math.Exp(-Damping * DeltaTime);

            // Zoom & Roll
            if (Touch.TwoPoint)
            {
                if (!_twoTouchRepeated)
                {
                    _twoTouchRepeated = true;
                    _angleRollInit = Touch.TwoPointAngle - _angleRoll;
                    _offsetInit = Touch.TwoPointMidPoint - _offset;
                }
                _zoomVel = Touch.TwoPointDistanceVel * -0.01f;
                _angleRoll = Touch.TwoPointAngle - _angleRollInit;
                _offset = Touch.TwoPointMidPoint - _offsetInit;
            }
            else
            {
                _twoTouchRepeated = false;
                _zoomVel = Mouse.WheelVel * -0.5f;
                _angleRoll *= curDamp * 0.8f;
                _offset *= curDamp * 0.8f;
            }

            // UpDown / LeftRight rotation
            if (Mouse.LeftButton)
            {
                _keys = false;
                _angleVelHorz = -RotationSpeed * Mouse.XVel * 0.000002f;
                _angleVelVert = -RotationSpeed * Mouse.YVel * 0.000002f;
            }
            else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint)
            {
                _keys = false;
                float2 touchVel;
                touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                _angleVelHorz = -RotationSpeed * touchVel.x * 0.000002f;
                _angleVelVert = -RotationSpeed * touchVel.y * 0.000002f;
            }
            else
            {
                if (_keys)
                {
                    _angleVelHorz = -RotationSpeed * Keyboard.LeftRightAxis * 0.002f;
                    _angleVelVert = -RotationSpeed * Keyboard.UpDownAxis * 0.002f;
                }
                else
                {
                    _angleVelHorz *= curDamp;
                    _angleVelVert *= curDamp;
                }
            }

            // SCRATCH:
            float camYaw = 0;

            camYaw = NormRot(camYaw);

            _zoom += _zoomVel;
            //Limit zoom
            if (_zoom < 80)
                _zoom = 80;
            if (_zoom > 2000)
                _zoom = 2000;

            _angleHorz += _angleVelHorz;
            // Wrap-around to keep _angleHorz between -PI and + PI
            _angleHorz = M.MinAngle(_angleHorz);

            _angleVert += _angleVelVert;
            // Limit pitch to the range between [-PI/2, + PI/2]
            _angleVert = M.Clamp(_angleVert, -M.PiOver2, M.PiOver2);

            // Wrap-around to keep _angleRoll between -PI and + PI
            _angleRoll = M.MinAngle(_angleRoll);


            // Create the camera matrix and set it as the current ModelView transformation
            var mtxRot = float4x4.CreateRotationZ(_angleRoll) * float4x4.CreateRotationX(_angleVert) * float4x4.CreateRotationY(_angleHorz);
            var mtxCam = float4x4.LookAt(0, 0, -_zoom, 50, 50, 50, 0, 1, 0);
            _renderer.View = mtxCam * mtxRot * _sceneScale;
            RC.ModelView = mtxCam * mtxRot * _sceneScale;

            //var mtxOffset = float4x4.CreateTranslation(2 * _offset.x / Width, (-2 * _offset.y / Height), 0);
            var mtxOffset = float4x4.CreateTranslation(0,0,0);
            RC.Projection = mtxOffset * _projection;

            if (Input.Keyboard.IsKeyDown(KeyCodes.Space))
            {
                //Todo: MeshComponent verändert sich nicht
                Debug.WriteLine("NOISE!!");
                MapGenerator.addHeightNoise(-10, 10);
                Debug.WriteLine(_scene.Children[0].GetMesh().Vertices[0]);
            }

            _renderer.Traverse(_scene.Children);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
            Present();

        }

        private bool mountBunkerOnTile(MapTile tile, Bunker bunker)
        {
            if (tile.mountedBunker == null)
            {
                bunker.bunkerBase.Translation = tile.CenterPos;
                tile.mountedBunker = bunker;
                return true;
            }

            return false;
        }

        public static float NormRot(float rot)
        {
            while (rot > M.Pi)
                rot -= M.TwoPi;
            while (rot < -M.Pi)
                rot += M.TwoPi;
            return rot;
        }

        public void loadPlayers(int numberOfPlayers)
        {
            players = new List<Bunker>();
            Bunker tempBunker;
            float tempScale = (1.0f / 600.0f) * MapGenerator.tileSize;

            for (int i = 0; i < numberOfPlayers; i++)
            {
                tempBunker = new Bunker(playerNames[i]);
                tempBunker.load(playerNames[i] + ".fus");
                tempBunker.bunkerBase.Scale = new float3(tempScale, tempScale, tempScale);
                players.Add(tempBunker);
            }
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            _projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
        }

    }
}