using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core.Assets
{
    class ContainerClone
    {
        public SceneNodeContainer container;

        public ContainerClone(string _fusfile)
        {
            container = new SceneNodeContainer();
            container = AssetsManager.fusFiles[_fusfile];
        }

        public SceneNodeContainer cloneContainer(int _id)
        {
            ContainerClone origClone = (ContainerClone)MemberwiseClone();
            SceneNodeContainer clone = new SceneNodeContainer();

            clone.Components = new List<SceneComponentContainer>();

            TransformComponent transformComp = new TransformComponent();
            transformComp.Translation = float3.Zero;
            transformComp.Rotation = float3.Zero;
            transformComp.Scale = float3.One;

            clone.Components.Add(transformComp);
            clone.Components.Add(origClone.container.GetMaterial());
            clone.Components.Add(origClone.container.GetMesh());

            clone.Name = "projectile";

            AssetsManager.renameNodesRecursively(clone, "", "_" + _id);

            return clone;
        }

    }
}


//WITH ATTRIBUTE
/*
    class ContainerClone
    {
        public SceneNodeContainer container;

        public ContainerClone(string _fusfile)
        {
            container = new SceneNodeContainer();
            container = AssetsManager.fusFiles[_fusfile];
        }

        public SceneNodeContainer cloneContainer(int _id)
        {
            ContainerClone clone = (ContainerClone)MemberwiseClone();
            clone.container.Name = "projectile";
            SceneNodeContainer clonedContainer = clone.container;

            AssetsManager.renameNodesRecursively(clonedContainer, "", "_" + _id);

            return clonedContainer;
        }

    }
*/


//DERIVED SCENENODECONTAINER
/*
    class ContainerClone: SceneNodeContainer
    {
        public ContainerClone(string _fusfile)
        {
            Components = new List<SceneComponentContainer>();
            Children = new List<SceneNodeContainer>();

            Components.Add(new TransformComponent());
            Components.Add(new MaterialComponent());

            this.GetTransform().Translation = float3.Zero;
            this.GetTransform().Scale = float3.One;

            Name = "projectile_root";

            Children.Add(AssetsManager.fusFiles[_fusfile]);
        }

        public SceneNodeContainer cloneContainer(int _id)
        {
            SceneNodeContainer clone = (SceneNodeContainer)MemberwiseClone();
            clone.Name = "projectile_root";

            clone.Components = new List<SceneComponentContainer>();
            clone.Components.Add(new TransformComponent());

            clone.GetTransform().Translation = float3.Zero;
            clone.GetTransform().Rotation = float3.Zero;
            clone.GetTransform().Scale = float3.One;

            AssetsManager.renameNodesRecursively(clone, "", "_" + _id);

            return clone;
        }

    }
    */
