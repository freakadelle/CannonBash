#define GUI_SIMPLE
using System.Collections.Generic;
using System.Linq;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Assets;
using static System.Math;
using static Fusee.Engine.Core.Input;
using Fusee.Base.Core;
#if GUI_SIMPLE
using Fusee.Engine.Core.GUI;
#endif

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        /*
            Todo: GUI möglich? Anzahl Schüsse/Welcher Spieler an der Reihe/Welche Spieler noch am Leben/Lebenspunkte/Windstärke etc...
            Todo: Bug beim treffen eines Tiles in unmittelbarer Nähe. Nächster Spieler feuert automatisch Kugel ab
            Todo: Bug mit Textur beim Verändelrn der Mapgröße.
            Todo: Bug mit Textur wenn Textur direkt aus dem Dictionary eingelesen wird.
            Todo: Windstärke einbauen. Macht nur Sinn wenn grafisches Interface möglich
            Todo: Game Over. Game Win
            Todo: Menü für die Einstellung der Spieleranzahl und der Auswahl der Map
            Todo: Bäume auf die Insel generieren
            Todo: Mapgenerierung verbessern?!
            Todo: Lavamap fertig machen
        */

        private bool _twoTouchRep;

        private Renderer _renderer;
        private Camera cam;
        private List<Bunker> players;
        private List<Projectile> projectiles;
        private int numberOfPlayers = 6, activePlayerId = 0;
        private int turnTime;
        private bool turnEnded;

        #if GUI_SIMPLE
        private GUIHandler _guiHandler;
        private Font _guiFont;
        private FontMap _guiFontMap;
        private GUIText _guiTextPlayer;
        private GUIText _guiTextHealth;
        private GUIText _guiTextShotPower;

        private GUIImage _guiCrossHair;
        #endif
        //INIT IS CALLED ON STARTUP
        public override void Init()
        {
            //LOAD ALL GAME RELEVANT ASSETS
            AssetsManager.loadGameAssets();

            //INITIALIZE CAMERA
            cam = new Camera();
            cam.projection = float4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, Width / (float)Height, 0.01f, 20);

            //INITIALIZE NEW GAME
            newGame();

            //INSTANTIATE RENDERER AND BACKBUFFER
            _renderer = new Renderer(RC);
            RC.ClearColor = new float4(0.8f, 0.8f, 1f, 1);

            // INSTANTIATE GUI
            Width = 1600;
            Height = 900;
            
            #if GUI_SIMPLE
            _guiHandler = new GUIHandler();
            _guiHandler.AttachToContext(RC);

            _guiCrossHair = AssetsManager.guiImages["crosshairTexture"];
            System.Diagnostics.Debug.WriteLine("_guiCrossHair: " + _guiCrossHair);
            _guiCrossHair.PosX = 200;
            _guiCrossHair.PosY = 500;

            _guiFont = AssetsManager.fonts["Army"];
            _guiFont.UseKerning = true;
            _guiFontMap = new FontMap(_guiFont, 20);

            _guiTextHealth = new GUIText("100", _guiFontMap, 30, 30);
            _guiTextHealth.TextColor = new float4(1, 1, 1, 1);

            _guiTextPlayer = new GUIText("Player X", _guiFontMap, 30, 60);
            _guiTextPlayer.TextColor = new float4(1, 1, 1, 1);

            _guiTextShotPower = new GUIText("0", _guiFontMap, 30, 90);
            _guiTextShotPower.TextColor = new float4(1, 1, 1, 1);

            _guiHandler.Add(_guiTextHealth);
            _guiHandler.Add(_guiTextPlayer);
            _guiHandler.Add(_guiTextShotPower);
            #endif
        }

        //RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            //CLEAR THE BACKBUFFER
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            //HANDLE INPUT CONTROLS
            handleInputControls();

            //UPDATE ALL EXISTING PROJECTILES
            for (int i = 0; i < projectiles.Count; i++)
            {
                //PROJECTILE MOVEMENT
                projectiles[i].update();

                MapTile collTile = projectiles[i].isCollided();

                //DESTRY PROJECTILE IF OUT OF MAP
                if (projectiles[i].isOutOfMap())
                {
                    SceneManager.destroyNode(SceneManager.rootNodes["projectileRoot"], projectiles[i].container.Name);
                    projectiles.RemoveAt(i);
                    turnEnded = true;
                } else if (collTile != null)
                {
                    projectileHitTile(collTile, 8, 50);
                    SceneManager.destroyNode(SceneManager.rootNodes["projectileRoot"], projectiles[i].container.Name);
                    projectiles.RemoveAt(i);
                    turnEnded = true;
                }
            }

            if (turnTime < 0)
            {
                turnTime = Constants.TURN_TIME_MAX;
                turnEnded = false;
                nextPlayersTurn();
            }

            if (turnEnded)
            {
                turnTime--;
            }

            //WATER ANIMATION PROCESS INCREMENT
            float newAlpha = (float) _renderer.shaderEffects["mapRoot"].GetEffectParam("alpha") + 1.8f;
            _renderer.shaderEffects["mapRoot"].SetEffectParam("alpha", newAlpha);

            //UPDATE CAMERA MOVEMENTS
            cam.update();

            //UPDATE VIEW & PROJECTION
            float4x4 view = (cam.MtxPivot * cam.MtxRot * cam.MtxOffset) * SceneManager.sceneScale;
            _renderer.View = view;
            RC.ModelView = view;
            RC.Projection = cam.projection;

            //RENDER AND PRESENT
            _renderer.Traverse(SceneManager.scene.Children);
            Present();
        }

        //INITIALIZE A NEW GAME
        private void newGame()
        {
            //CREATE EMPTY SCENE AND ROOT NODES
            SceneManager.createEmpty();
            SceneManager.addRootNode("mapRoot", MapGenerator.instantiatePlaneMap());
            SceneManager.addRootNode("bunkerRoot", SceneManager.createEmptySceneNode());
            SceneManager.addRootNode("projectileRoot", SceneManager.createEmptySceneNode());
            SceneManager.addRootNode("guiRoot", SceneManager.createEmptySceneNode());

            //TERRAIN GENERATION - HILLS
            MapGenerator.generateTerrain(12 * numberOfPlayers);

            //INITIALIZE LISTS
            projectiles = new List<Projectile>();
            players = new List<Bunker>();

            //INIT AMOUNT OF PLAYERS AND BUNKERS
            loadPlayers(numberOfPlayers);

            //SLICE MAP INTO GRIDS DEPENDING ON AMOUNT OF PLAYER AND RETURN ZENIT TILE OF EACH GRID
            int grids = Max((int)Ceiling(players.Count / 2.0f), 2);
            List<MapTile> areaZenits = MapGenerator.gridMapReturnZenitTiles(grids);

            //MOUNT BUNKERS ON ZENIT TILES
            foreach (var player in players)
            {
                int randomZenit = Constants.random.Next(0, areaZenits.Count);
                player.mountBunkerOnTile(areaZenits[randomZenit]);
                areaZenits.RemoveAt(randomZenit);
            }

            //MOUNT CAMERA ON ACTIVE BUNKER
            cam.mountCameraOnBunker(players[activePlayerId]);

            turnTime = Constants.TURN_TIME_MAX;
            turnEnded = false;
        }

        //LOAD SPECIFIED NUMBER OF PLAYERS
        public void loadPlayers(int numberOfPlayers)
        {
            numberOfPlayers = Max(1, numberOfPlayers);
            numberOfPlayers = Min(AssetsManager.FUS_BUNKER_FILES.Count(), numberOfPlayers);

            for (int i = 0; i < numberOfPlayers; i++)
            {
                Bunker tempBunker = new Bunker(AssetsManager.FUS_BUNKER_FILES[i]);

                //ADD BUNKER TO LIST AND SCENE
                players.Add(tempBunker);
                SceneManager.rootNodes["bunkerRoot"].Children.Add(players[i].scene);
            }
        }

        private void nextPlayersTurn()
        {
            activePlayerId++;
            if (activePlayerId >= players.Count)
            {
                activePlayerId = 0;
            }

            players[activePlayerId].ammo = 1;
            cam.mountCameraOnBunker(players[activePlayerId]);
        }

        private void projectileHitTile(MapTile _tile, float _radius, float _strength)
        {
            MapGenerator.createHillAt(_tile.index, -_strength, float2.One * _radius, true);
        }

        //HANDLE KEYBOARD & MOUSE INPUTS
        private void handleInputControls()
        {
            //CAMERA ROTATION
            Bunker activePlayer = players[activePlayerId];

            if (Mouse.LeftButton && activePlayer.ammo > 0)
            {
                //SHOOT PROJECTILE AND ADD TO LIST AND SCENE
                Projectile proj = activePlayer.shootProjectile();
                projectiles.Add(proj);
                SceneManager.rootNodes["projectileRoot"].Children.Add(proj.container);
                activePlayer.ammo--;
            } else if (Keyboard.IsKeyDown(KeyCodes.Space))
            {
                MapGenerator.nextTexture();
            } else if (Keyboard.IsKeyDown(KeyCodes.Enter))
            {
                newGame();
                _renderer.randomShaderEffects();
            }

            activePlayer.rotatePlatform(Mouse.XVel);
            activePlayer.liftCannon(Mouse.YVel);
            cam.Rotation = new float3(-activePlayer.bunkerCannon.Rotation.z - (float)(PI * 1.5f), -activePlayer.bunkerPlatform.Rotation.y - (float)(PI * 1.5f), 0);
        }

        //IS CALLED ON WINDOW RESIZE
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            cam.projection = float4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, aspectRatio, 1, 20000);
        }
    }
}