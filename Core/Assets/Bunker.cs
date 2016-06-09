using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Base.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core.Assets
{

    class Bunker
    {

        public static SceneContainer scene;

        public TransformComponent bunkerBase;
        public TransformComponent bunkerPlatform;
        public TransformComponent bunkerCannon;

        public Bunker()
        {
            bunkerBase = scene.Children.FindNodes(c => c.Name == "Bunker").First()?.GetTransform();
            bunkerPlatform = scene.Children.FindNodes(c => c.Name == "Turn").First()?.GetTransform();
            bunkerCannon = scene.Children.FindNodes(c => c.Name == "CannonRohr").First()?.GetTransform();
        }

        public static void load()
        {
            scene = AssetStorage.Get<SceneContainer>("Bunker_v5.fus");
        }

    }
}
