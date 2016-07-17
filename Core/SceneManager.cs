using System.Collections.Generic;
using System.Linq;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{
    static class SceneManager
    {

        public static SceneContainer scene;
        public static Dictionary<string, SceneNodeContainer> rootNodes;

        public static float4x4 sceneScale = float4x4.CreateScale(1);

        public static void createEmpty()
        {
            scene = new SceneContainer();
            scene.Children = new List<SceneNodeContainer>();
            scene.Header = new SceneHeader();

            rootNodes = new Dictionary<string, SceneNodeContainer>();
        }

        public static void addRootNode(string _name, SceneNodeContainer _node)
        {
            _node.Name = _name;
            rootNodes.Add(_name, _node);
            scene.Children.Add(rootNodes[_name]);
        }

        //DESTROYS AN OBJECT WHETHER IT IS IN SCENECONTAINER OR IN SCENENODECONTAINER
        public static void destroyNode<T>(T _node, string _name)
        {
            SceneNodeContainer node = (SceneNodeContainer) (object) _node;
            SceneNodeContainer destrObj = node.Children.FindNodes(c => c.Name == _name).First();
            node.Children.RemoveAt(node.Children.IndexOf(destrObj));
        }

        public static SceneNodeContainer createEmptySceneNode(string _name = "", bool _hasChildren = true)
        {
            SceneNodeContainer sceneNode = new SceneNodeContainer();
            sceneNode.Components = new List<SceneComponentContainer>();

            TransformComponent transComp = new TransformComponent();
            transComp.Translation = float3.Zero;
            transComp.Rotation = float3.Zero;
            transComp.Scale = float3.One;

            sceneNode.Components.Add(transComp);
            sceneNode.Name = _name;

            if (_hasChildren)
            {
                sceneNode.Children = new List<SceneNodeContainer>();
            }

            return sceneNode;
        }
    }
}
