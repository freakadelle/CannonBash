using Fusee.Base.Common;

namespace Fusee.Tutorial.Core
{
    class TextureImage
    {
        public ImageData src;
        public string name;
        public string path;

        public TextureImage(ImageData _src, string _name, string _path)
        {
            src = _src;
            name = _name;
            path = _path;
        }
    }
}
