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

        public string name;

        public SceneContainer scene;

        public TransformComponent bunkerBase;
        public TransformComponent bunkerPlatform;
        public TransformComponent bunkerCannon;

        public Bunker(string _name)
        {
            name = _name;
            loadAsset(name + ".fus");
        }

        private void loadAsset(string fileName)
        {
            scene = AssetStorage.Get<SceneContainer>(fileName);

            renameRecursively(scene.Children);

            bunkerBase = scene.Children.FindNodes(c => c.Name == name + "Base").First()?.GetTransform();
            bunkerPlatform = scene.Children.FindNodes(c => c.Name == name + "Turn").First()?.GetTransform();
            bunkerCannon = scene.Children.FindNodes(c => c.Name == name + "CannonRohr").First()?.GetTransform();
        }

        public void renameRecursively(List<SceneNodeContainer> elem)
        {
            foreach (var child in elem)
            {
                child.Name = name + child.Name;

                if (child.Children != null && child.Children.Count > 0)
                {
                    renameRecursively(child.Children);
                }
            }
        }
    }
}
