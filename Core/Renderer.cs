using System.Collections.Generic;
using System.Diagnostics;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{
    class Renderer : SceneVisitor
    {
        // ReSharper disable once InconsistentNaming
        public RenderContext RC;
        public float4x4 View;
        private static Dictionary<MeshComponent, Mesh> _meshes = new Dictionary<MeshComponent, Mesh>();
        private readonly CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();
        private readonly Dictionary<string, ITexture> _textures;
        private readonly Dictionary<string, ShaderEffect> _shaderEffects;
        private ITexture _textureValue;
        private readonly ShaderEffect _shaderEffect;

        #region LookupMesh

        public static void translateVerticesOfMesh(MeshComponent _comp, int _vertInd, float3 _trans)
        {
            Mesh mesh;
            _meshes.TryGetValue(_comp, out mesh);

            mesh = new Mesh
            {
                Vertices = _comp.Vertices,
                Normals = _comp.Normals,
                UVs = _comp.UVs,
                Triangles = _comp.Triangles
            };

            //Translate Vertice
            float3 tempVert = mesh.Vertices[_vertInd];
            tempVert = new float3(tempVert.x + _trans.x, tempVert.y + _trans.y, tempVert.z + _trans.z);
            mesh.Vertices[_vertInd] = tempVert;

            _meshes[_comp] = mesh;
        }

        private Mesh LookupMesh(MeshComponent mc)
        {
            Mesh mesh;
            if (_meshes.TryGetValue(mc, out mesh)) return mesh;

            mesh = new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                UVs = mc.UVs,
                Triangles = mc.Triangles
            };
            _meshes[mc] = mesh;

            return mesh;
        }
        #endregion

        #region Renderer
        public Renderer(RenderContext rc)
        {
            RC = rc;

            // Initialize the shader(s)
            _shaderEffects = new Dictionary<string, ShaderEffect>();

            _textures = new Dictionary<string, ITexture>();

            var leaves = AssetStorage.Get<ImageData>("Leaves.jpg");
            _textureValue = RC.CreateTexture(leaves);
            _textures.Add("Leaves.jpg", _textureValue);

            var sky = AssetStorage.Get<ImageData>("sky.png");
            _textureValue = RC.CreateTexture(leaves);
            _textures.Add("sky.png", _textureValue);

            var vertexShader = AssetStorage.Get<string>("VertexShader.vert");
            var pixelShader = AssetStorage.Get<string>("PixelShader.frag");
            var vertexShaderMountains = AssetStorage.Get<string>("VertexShader_mountain.vert");
            var pixelShaderMountains = AssetStorage.Get<string>("PixelShader_mountains.frag");
            var vertexShaderTexture = AssetStorage.Get<string>("VertexShader_texture.vert");
            var pixelShaderTexture = AssetStorage.Get<string>("PixelShader_texture.frag");


            _shaderEffect = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration
                    {
                        VS = vertexShader,
                        PS = pixelShader,
                        StateSet = new RenderStateSet
                        {
                            ZEnable = true,
                            CullMode = Cull.Counterclockwise
                        }
                    }
                },
                new[]
                {
                    new EffectParameterDeclaration {Name = "albedo", Value = float3.One},
                    new EffectParameterDeclaration {Name = "shininess", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "specfactor", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "speccolor", Value = float3.Zero},
                    new EffectParameterDeclaration {Name = "ambientcolor", Value = float3.Zero},
                });

            var shaderEffectMountains = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration
                    {
                        VS = vertexShaderMountains,
                        PS = pixelShaderMountains,
                        StateSet = new RenderStateSet
                        {
                            // Fix from E-Mail
                            ZEnable = true,
                            CullMode = Cull.Counterclockwise
                        }
                    }
                },
                new[]
                {
                    new EffectParameterDeclaration {Name = "albedo", Value = float3.One},
                    new EffectParameterDeclaration {Name = "shininess", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "specfactor", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "speccolor", Value = float3.Zero},
                    new EffectParameterDeclaration {Name = "ambientcolor", Value = float3.One},
                    new EffectParameterDeclaration {Name = "ambientMix", Value = 1.0f},
                    new EffectParameterDeclaration {Name = "texmix", Value = 0.0f},
                    new EffectParameterDeclaration {Name = "texture", Value = _textureValue},
                    new EffectParameterDeclaration {Name = "minMaxHeight", Value = MapGenerator.minMaxHeight}
                });

            var shaderEffectTexture = new ShaderEffect(
            new[]
            {
                    new EffectPassDeclaration
                    {
                        VS = vertexShaderTexture,
                        PS = pixelShaderTexture,
                        StateSet = new RenderStateSet
                        {
                            // Fix from E-Mail
                            ZEnable = true,
                            CullMode = Cull.Counterclockwise
                        }
                    }
            },
            new[]
            {
                    new EffectParameterDeclaration {Name = "albedo", Value = float3.One},
                    new EffectParameterDeclaration {Name = "texmix", Value = 0.0f},
                    new EffectParameterDeclaration {Name = "texture", Value = _textureValue},
            });

            // Add ShaderEffect
            _shaderEffects.Add("TileMap", shaderEffectMountains);
            _shaderEffects.Add("Sky", shaderEffectTexture);
            _shaderEffects.Add("wuerfel", shaderEffectMountains);

            _shaderEffect.AttachToContext(RC);
            shaderEffectMountains.AttachToContext(RC);
            shaderEffectTexture.AttachToContext(RC);
        }

        protected override void InitState()
        {
            _model.Clear();
            _model.Tos = float4x4.Identity;
        }

        protected override void PushState()
        {
            _model.Push();
        }

        protected override void PopState()
        {
            _model.Pop();
            RC.ModelView = View * _model.Tos;
        }
        #endregion

        #region Visitors
        [VisitMethod]
        public void OnMesh(MeshComponent mesh)
        {
            ShaderEffect currentShaderEffect;
            if (_shaderEffects.TryGetValue(CurrentNode.Name, out currentShaderEffect))
                currentShaderEffect.RenderMesh(LookupMesh(mesh));
            else
                _shaderEffect.RenderMesh(LookupMesh(mesh));
            //RC.Render(LookupMesh(mesh));
        }


        [VisitMethod]
        public void OnMaterial(MaterialComponent material)
        {
            // Prepare your Renderer to handle more than one ShaderEffect - e.g. based on object names. 
            ShaderEffect currentShaderEffect;
            RenderMaterial(material,
                _shaderEffects.TryGetValue(CurrentNode.Name, out currentShaderEffect) ? currentShaderEffect : _shaderEffect);
        }

        [VisitMethod]
        public void OnTransform(TransformComponent xform)
        {
            _model.Tos *= xform.Matrix();
            RC.ModelView = View * _model.Tos;
        }

        private void RenderMaterial(MaterialComponent material, ShaderEffect shaderEffect)
        {
            if (material.HasDiffuse)
            {
                shaderEffect.SetEffectParam("albedo", material.Diffuse.Color);

                if (material.Diffuse.Texture != null)
                {
                    var textureKey = material.Diffuse.Texture;
                    // Check if texture is in dictionary, else create and add it
                    if (!_textures.TryGetValue(textureKey, out _textureValue))
                    {
                        var imageData = AssetStorage.Get<ImageData>(material.Diffuse.Texture);
                        var texture = RC.CreateTexture(imageData);
                        _textures.Add(textureKey, texture);
                    }

                    _textures.TryGetValue(textureKey, out _textureValue);
                    // Set texture
                    shaderEffect.SetEffectParam("texture", _textureValue);
                    shaderEffect.SetEffectParam("texmix", material.Diffuse.Mix);
                }
                else
                {
                    shaderEffect.SetEffectParam("texmix", 0f);
                }
            }
            else
            {
                shaderEffect.SetEffectParam("albedo", float3.Zero);
            }
            if (material.HasSpecular)
            {
                shaderEffect.SetEffectParam("shininess", material.Specular.Shininess);
                shaderEffect.SetEffectParam("specfactor", material.Specular.Intensity);
                shaderEffect.SetEffectParam("speccolor", material.Specular.Color);
            }
            else
            {
                shaderEffect.SetEffectParam("shininess", 0);
                shaderEffect.SetEffectParam("specfactor", 0);
                shaderEffect.SetEffectParam("speccolor", float3.Zero);
            }
            if (material.HasEmissive)
            {
                shaderEffect.SetEffectParam("ambientcolor", material.Emissive.Color);
                shaderEffect.SetEffectParam("ambientMix", material.Emissive.Mix);
            }
            else
            {
                shaderEffect.SetEffectParam("ambientcolor", float3.Zero);
            }
        }
    }
    #endregion

}
