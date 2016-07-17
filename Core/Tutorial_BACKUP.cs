//using System.Collections.Generic;
//using System.Linq;
//using Fusee.Engine.Common;
//using Fusee.Engine.Core;
//using Fusee.Math.Core;
//using Fusee.Tutorial.Core.Assets;
//using static System.Math;
//using static Fusee.Engine.Core.Input;
//using static Fusee.Engine.Core.Time;

//namespace Fusee.Tutorial.Core
//{

//    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
//    public class Tutorial : RenderCanvas
//    {
//        private bool _twoTouchRep;

//        private Renderer _renderer;
//        private Camera cam;
//        private List<Bunker> players;
//        private List<Projectile> projectiles;
//        private int numberOfPlayers = 6, activePlayerId = 0;

//        //INIT IS CALLED ON STARTUP
//        public override void Init()
//        {
//            //LOAD ALL GAME RELEVANT ASSETS
//            AssetsManager.loadGameAssets();

//            //INITIALIZE CAMERA
//            cam = new Camera();
//            cam.projection = float4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, Width / (float)Height, 0.01f, 20);

//            //INITIALIZE NEW GAME
//            newGame();

//            //INSTANTIATE RENDERER AND BACKBUFFER
//            _renderer = new Renderer(RC);
//            RC.ClearColor = new float4(0.8f, 0.8f, 1f, 1);
//        }

//        //RenderAFrame is called once a frame
//        public override void RenderAFrame()
//        {
//            //CLEAR THE BACKBUFFER
//            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

//            //HANDLE INPUT CONTROLS
//            handleInputControls();

//            //UPDATE ALL EXISTING PROJECTILES
//            for (int i = 0; i < projectiles.Count; i++)
//            {
//                //PROJECTILE MOVEMENT
//                projectiles[i].update();

//                MapTile collTile = projectiles[i].isCollided();

//                //DESTRY PROJECTILE IF OUT OF MAP
//                if (projectiles[i].isOutOfMap())
//                {
//                    SceneManager.destroyNode(SceneManager.rootNodes["projectileRoot"], projectiles[i].container.Name);
//                    projectiles.RemoveAt(i);
//                } else if (collTile != null)
//                {
//                    projectileHitTile(collTile, 8, 50);
//                    SceneManager.destroyNode(SceneManager.rootNodes["projectileRoot"], projectiles[i].container.Name);
//                    projectiles.RemoveAt(i);
//                }
//            }

//            //WATER ANIMATION PROCESS INCREMENT
//            float newAlpha = (float) _renderer.shaderEffects["mapRoot"].GetEffectParam("alpha") + 1.8f;
//            _renderer.shaderEffects["mapRoot"].SetEffectParam("alpha", newAlpha);

//            //UPDATE CAMERA MOVEMENTS
//            cam.update();

//            //UPDATE VIEW & PROJECTION
//            float4x4 view = (cam.MtxPivot * cam.MtxRot * cam.MtxOffset) * SceneManager.sceneScale;
//            _renderer.View = view;
//            RC.ModelView = view;
//            RC.Projection = cam.projection;

//            //RENDER AND PRESENT
//            _renderer.Traverse(SceneManager.scene.Children);
//            Present();
//        }

//        //INITIALIZE A NEW GAME
//        private void newGame()
//        {
//            //CREATE EMPTY SCENE AND ROOT NODES
//            SceneManager.createEmpty();
//            SceneManager.addRootNode("mapRoot", MapGenerator.instantiatePlaneMap());
//            SceneManager.addRootNode("bunkerRoot", SceneManager.createEmptySceneNode());
//            SceneManager.addRootNode("projectileRoot", SceneManager.createEmptySceneNode());

//            //TERRAIN GENERATION - HILLS
//            MapGenerator.generateTerrain(12 * numberOfPlayers);

//            //INITIALIZE LISTS
//            projectiles = new List<Projectile>();
//            players = new List<Bunker>();

//            //INIT AMOUNT OF PLAYERS AND BUNKERS
//            loadPlayers(numberOfPlayers);

//            //SLICE MAP INTO GRIDS DEPENDING ON AMOUNT OF PLAYER AND RETURN ZENIT TILE OF EACH GRID
//            int grids = Max((int)Ceiling(players.Count / 2.0f), 2);
//            List<MapTile> areaZenits = MapGenerator.gridMapReturnZenitTiles(grids);

//            //MOUNT BUNKERS ON ZENIT TILES
//            foreach (var player in players)
//            {
//                int randomZenit = Constants.random.Next(0, areaZenits.Count);
//                player.mountBunkerOnTile(areaZenits[randomZenit]);
//                areaZenits.RemoveAt(randomZenit);
//            }

//            //MOUNT CAMERA ON ACTIVE BUNKER
//            cam.mountCameraOnBunker(players[activePlayerId]);
//        }

//        //LOAD SPECIFIED NUMBER OF PLAYERS
//        public void loadPlayers(int numberOfPlayers)
//        {
//            numberOfPlayers = Max(1, numberOfPlayers);
//            numberOfPlayers = Min(AssetsManager.FUS_BUNKER_FILES.Count(), numberOfPlayers);

//            for (int i = 0; i < numberOfPlayers; i++)
//            {
//                Bunker tempBunker = new Bunker(AssetsManager.FUS_BUNKER_FILES[i]);

//                //ADD BUNKER TO LIST AND SCENE
//                players.Add(tempBunker);
//                SceneManager.rootNodes["bunkerRoot"].Children.Add(players[i].scene);
//            }
//        }

//        private void nextPlayersTurn()
//        {
//            activePlayerId++;
//            if (activePlayerId >= players.Count)
//            {
//                activePlayerId = 0;
//            }

//            cam.mountCameraOnBunker(players[activePlayerId]);
//        }

//        private void projectileHitTile(MapTile _tile, float _radius, float _strength)
//        {
//            MapGenerator.createHillAt(_tile.index, -_strength, float2.One * _radius, true);
//        }

//        //HANDLE KEYBOARD & MOUSE INPUTS
//        private void handleInputControls()
//        {
//            //CAMERA ZOOM
//            //cam.CurDamp = (float)Exp(-cam.Damping * DeltaTime);
//            //if (Touch.TwoPoint)
//            //    _twoTouchRep = cam.touchZoom(Touch.TwoPointAngle, Touch.TwoPointMidPoint, Touch.TwoPointDistanceVel, _twoTouchRep);
//            //else
//            //    _twoTouchRep = cam.mouseWheelZoom(Mouse.WheelVel);

//            //CAMERA ROTATION
//            float2 rotVel = float2.Zero;
//            if (Mouse.LeftButton)
//                cam.rotate(Mouse.Velocity);
//            else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint)
//                cam.rotate(Touch.GetVelocity(TouchPoints.Touchpoint_0));
//            else if (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
//                cam.rotate(new float2(Keyboard.LeftRightAxis, Keyboard.UpDownAxis));
//            else
//                cam.rotateDump();

//            //CAMERA MOVEMENT
//            if (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0)
//            {
//                //float2 moveVel = new float2(Keyboard.ADAxis, Keyboard.WSAxis) * 10f;
//                //cam.Translation = new float3(cam.Translation.x - moveVel.x, cam.Translation.y, cam.Translation.z - moveVel.y);
//            }

//            Bunker activePlayer = players[activePlayerId];
//            if (Keyboard.IsKeyDown(KeyCodes.Space))
//            {
//                MapGenerator.nextTexture();
//            } else if (Keyboard.IsKeyDown(KeyCodes.Enter))
//            {
//                newGame();
//                _renderer.randomShaderEffects();
//            } else if (Keyboard.IsKeyDown(KeyCodes.End))
//            {
//                //SHOOT PROJECTILE AND ADD TO LIST AND SCENE
//                Projectile proj = activePlayer.shootProjectile();
//                projectiles.Add(proj);
//                SceneManager.rootNodes["projectileRoot"].Children.Add(proj.container);
//            } else if (Keyboard.IsKeyDown(KeyCodes.Delete))
//            {
//                nextPlayersTurn();
//            }

//            activePlayer.rotatePlatform(Mouse.XVel);
//            activePlayer.liftCannon(Mouse.YVel);

//            //float2 lookTargetDir = Constants.angleToVector(activePlayer.bunkerPlatform.Rotation.y);
//            //cam.target = new float3(lookTargetDir.x, 0, lookTargetDir.y);
//            cam.Rotation = new float3(0, -activePlayer.bunkerPlatform.Rotation.y - (float)(PI * 1.5f), 0);
//        }

//        //IS CALLED ON WINDOW RESIZE
//        public override void Resize()
//        {
//            // Set the new rendering area to the entire new windows size
//            RC.Viewport(0, 0, Width, Height);

//            // Create a new projection matrix generating undistorted images on the new aspect ratio.
//            var aspectRatio = Width / (float)Height;

//            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
//            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
//            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
//            cam.projection = float4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, aspectRatio, 1, 20000);
//        }
//    }
//}