﻿//using System.Collections.Generic;
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
//        /*
//            Todo: GUI möglich? Anzahl Schüsse/Welcher Spieler an der Reihe/Welche Spieler noch am Leben/Lebenspunkte/Windstärke etc...
//            Todo: Bug beim treffen eines Tiles in unmittelbarer Nähe. Nächster Spieler feuert automatisch Kugel ab
//            Todo: Bug mit Textur beim Verändelrn der Mapgröße.
//            Todo: Bug mit Textur wenn Textur direkt aus dem Dictionary eingelesen wird.
//            Todo: Windstärke einbauen. Macht nur Sinn wenn grafisches Interface möglich
//            Todo: Game Over. Game Win
//            Todo: Menü für die Einstellung der Spieleranzahl und der Auswahl der Map
//            Todo: Bäume auf die Insel generieren
//            Todo: Mapgenerierung verbessern?!
//            Todo: Lavamap fertig machen
//        */

//        private Renderer _renderer;
//        private List<Bunker> _players;
//        private List<Projectile> _projectiles;
//        private List<Camera> _cams;
//        private int numberOfPlayers = 4, activePlayerId = 0, activeCamId = 1;
//        private int turnTime;
//        private bool turnEnded;

//        //INIT IS CALLED ON STARTUP
//        public override void Init()
//        {
//            //LOAD ALL GAME RELEVANT ASSETS
//            AssetsManager.loadGameAssets();

//            //INITIALIZE CAMERA
//            _cams = new List<Camera>();

//            //PLAYER CAMERA
//            Camera cam = new Camera();
//            cam.projection = float4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, Width / (float)Height, 0.01f, 20);
//            cam.pivot = new float3(1, -1.8f, -2.7f);
//            _cams.Add(cam);

//            //WORLD CAMERA
//            cam = new Camera();
//            cam.projection = float4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, Width / (float)Height, 0.01f, 20);
//            cam.Rotation = new float3((float)-PI * 0.5f, (float)PI * 0.5f, 0);
//            cam.up = new float3(0, 1, 0);
//            cam.target = new float3(50, 50, 50);
//            cam.eye = new float3(0, 10, 1500);
//            _cams.Add(cam);

//            //INITIALIZE NEW GAME
//            newGame();

//            cam.Translation = new float3(-MapGenerator.mapUnits.x / 2.0f, 0, -MapGenerator.mapUnits.y / 2.0f);

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
//            for (int i = 0; i < _projectiles.Count; i++)
//            {
//                //PROJECTILE MOVEMENT
//                _projectiles[i].update();

//                MapTile collTile = _projectiles[i].isCollided();

//                //DESTRY PROJECTILE IF OUT OF MAP
//                if (_projectiles[i].isOutOfMap())
//                {
//                    SceneManager.destroyNode(SceneManager.rootNodes["projectileRoot"], _projectiles[i].container.Name);
//                    _projectiles.RemoveAt(i);
//                    turnEnded = true;
//                }
//                else if (collTile != null)
//                {
//                    projectileHitTile(collTile, 8, 50);
//                    SceneManager.destroyNode(SceneManager.rootNodes["projectileRoot"], _projectiles[i].container.Name);
//                    _projectiles.RemoveAt(i);
//                    turnEnded = true;
//                }
//            }

//            if (turnTime < 0)
//            {
//                turnTime = Constants.TURN_TIME_MAX;
//                turnEnded = false;
//                nextPlayersTurn();
//            }

//            if (turnEnded)
//            {
//                turnTime--;
//            }

//            //WATER ANIMATION PROCESS INCREMENT
//            float newAlpha = (float)_renderer.shaderEffects["mapRoot"].GetEffectParam("alpha") + 1.8f;
//            _renderer.shaderEffects["mapRoot"].SetEffectParam("alpha", newAlpha);

//            //ADJUST VIEW FOR SPECIFIC CAMERA
//            Camera activeCamera = _cams[activeCamId];
//            float4x4 view;
//            if (activeCamId == 1)
//            {
//                view = (activeCamera.MtxCam * activeCamera.MtxRot * activeCamera.MtxOffset) * SceneManager.sceneScale;
//            }
//            else
//            {
//                view = (activeCamera.MtxPivot * activeCamera.MtxRot * activeCamera.MtxOffset) * SceneManager.sceneScale;
//            }

//            //UPDATE VIEW & PROJECTION
//            _renderer.View = view;
//            RC.ModelView = view;
//            RC.Projection = activeCamera.projection;

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
//            _projectiles = new List<Projectile>();
//            _players = new List<Bunker>();

//            //INIT AMOUNT OF PLAYERS AND BUNKERS
//            loadPlayers(numberOfPlayers);

//            //SLICE MAP INTO GRIDS DEPENDING ON AMOUNT OF PLAYER AND RETURN ZENIT TILE OF EACH GRID
//            int grids = Max((int)Ceiling(_players.Count / 2.0f), 2);
//            List<MapTile> areaZenits = MapGenerator.gridMapReturnZenitTiles(grids);

//            //MOUNT BUNKERS ON ZENIT TILES
//            foreach (var player in _players)
//            {
//                int randomZenit = Constants.random.Next(0, areaZenits.Count);
//                player.mountBunkerOnTile(areaZenits[randomZenit]);
//                areaZenits.RemoveAt(randomZenit);
//            }

//            //MOUNT CAMERA ON ACTIVE BUNKER
//            _cams[0].mountCameraOnBunker(_players[activePlayerId]);

//            turnTime = Constants.TURN_TIME_MAX;
//            turnEnded = false;
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
//                _players.Add(tempBunker);
//                SceneManager.rootNodes["bunkerRoot"].Children.Add(_players[i].scene);
//            }
//        }

//        private void nextPlayersTurn()
//        {
//            activePlayerId++;
//            if (activePlayerId >= _players.Count)
//            {
//                activePlayerId = 0;
//            }

//            _players[activePlayerId].ammo = 1;
//            _cams[0].mountCameraOnBunker(_players[activePlayerId]);
//        }

//        private void switchCam()
//        {
//            activeCamId++;
//            if (activeCamId >= _cams.Count)
//            {
//                activeCamId = 0;
//            }
//        }

//        private void projectileHitTile(MapTile _tile, float _radius, float _strength)
//        {
//            MapGenerator.createHillAt(_tile.index, -_strength, float2.One * _radius, true);
//        }

//        //HANDLE KEYBOARD & MOUSE INPUTS
//        private void handleInputControls()
//        {
//            ////CAMERA ROTATION
//            Bunker activePlayer = _players[activePlayerId];

//            if (Mouse.LeftButton && activePlayer.ammo > 0)
//            {
//                //SHOOT PROJECTILE AND ADD TO LIST AND SCENE
//                Projectile proj = activePlayer.shootProjectile();
//                _projectiles.Add(proj);
//                SceneManager.rootNodes["projectileRoot"].Children.Add(proj.container);
//                activePlayer.ammo--;
//            }
//            else if (Keyboard.IsKeyDown(KeyCodes.Space))
//            {
//                MapGenerator.nextTexture();
//            }
//            else if (Keyboard.IsKeyDown(KeyCodes.Enter))
//            {
//                newGame();
//                _renderer.randomShaderEffects();
//            }
//            else if (Keyboard.IsKeyDown(KeyCodes.V))
//            {
//                switchCam();
//            }

//            //MULTI CAMERA CONTROLS
//            Camera activeCam = _cams[activeCamId];

//            activeCam.pivot = new float3(activeCam.pivot.x + Keyboard.ADAxis, activeCam.pivot.y + Keyboard.UpDownAxis, activeCam.pivot.z + Keyboard.WSAxis);

//            if (activeCamId == 0)
//            {
//                //ACTIVE PLAYER AND PLAYER CAMERA CONTROL
//                activePlayer.rotatePlatform(Mouse.XVel);
//                activePlayer.liftCannon(Mouse.YVel);
//                activeCam.Rotation = new float3(-activePlayer.bunkerCannon.Rotation.z - (float)(PI * 1.5f), -activePlayer.bunkerPlatform.Rotation.y - (float)(PI * 1.5f), 0);
//            }
//            else if (activeCamId == 1)
//            {
//                activeCam.setCurDamp(DeltaTime);

//                // Zoom & Roll
//                activeCam.mouseWheelZoom(Mouse.WheelVel);

//                // UpDown / LeftRight rotation
//                if (Mouse.LeftButton)
//                    activeCam.rotate(Mouse.Velocity);
//                else
//                    activeCam.rotateDump();

//                activeCam.update();
//            }
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
//            _cams[activeCamId].projection = float4x4.CreatePerspectiveFieldOfView(_cams[activeCamId].FieldOfView, aspectRatio, 1, 20000);
//        }
//    }
//}