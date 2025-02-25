using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.Serialization;

namespace GUI.Types.Renderer
{
    class RenderMaterial
    {
        public Shader Shader => shader;
        public Material Material { get; }
        public IKeyValueCollection VsInputSignature { get; }
        public Dictionary<string, RenderTexture> Textures { get; } = new();
        public bool IsBlended { get; }
        public bool IsToolsMaterial { get; }

        private readonly Shader shader;
        private readonly bool isAdditiveBlend;
        private readonly bool isRenderBackfaces;
        private readonly bool isOverlay;

        public RenderMaterial(Material material, IKeyValueCollection insg, ShaderLoader shaderLoader)
        {
            Material = material;
            VsInputSignature = insg;
            shader = shaderLoader.LoadShader(material.ShaderName, material.GetShaderArguments());

            IsToolsMaterial = material.IntAttributes.ContainsKey("tools.toolsmaterial");
            IsBlended = (material.IntParams.ContainsKey("F_TRANSLUCENT") && material.IntParams["F_TRANSLUCENT"] == 1)
                || material.IntAttributes.ContainsKey("mapbuilder.water")
                || material.IntParams.ContainsKey("F_BLEND_MODE") && material.IntParams["F_BLEND_MODE"] > 0
                || material.ShaderName == "vr_glass.vfx"
                || material.ShaderName == "vr_glass_markable.vfx"
                || material.ShaderName == "csgo_glass.vfx"
                || material.ShaderName == "csgo_effects.vfx"
                || material.ShaderName == "tools_sprite.vfx";
            isAdditiveBlend = material.IntParams.ContainsKey("F_ADDITIVE_BLEND") && material.IntParams["F_ADDITIVE_BLEND"] == 1
                || material.IntParams.ContainsKey("F_BLEND_MODE") && material.IntParams["F_BLEND_MODE"] == 4;
            isRenderBackfaces = material.IntParams.ContainsKey("F_RENDER_BACKFACES") && material.IntParams["F_RENDER_BACKFACES"] == 1;
            isOverlay = (material.IntParams.ContainsKey("F_OVERLAY") && material.IntParams["F_OVERLAY"] == 1)
                || material.IntParams.ContainsKey("F_DEPTH_BIAS") && material.IntParams["F_DEPTH_BIAS"] == 1
                || material.ShaderName.EndsWith("static_overlay.vfx", System.StringComparison.Ordinal);
        }

        public void Render(Shader shader = default, WorldLightingInfo lightingInfo = default)
        {
            //Start at 1, texture unit 0 is reserved for the animation texture
            var textureUnit = 1;

            shader ??= this.shader;

            IEnumerable<KeyValuePair<string, RenderTexture>> textures = Textures;

            if (lightingInfo != default)
            {
                textures = Textures.Concat(lightingInfo.Lightmaps);

                shader.SetUniform2("g_vLightmapUvScale", lightingInfo.LightmapUvScale);
            }

            foreach (var (name, texture) in textures)
            {
                if (shader.SetTexture(textureUnit, name, texture.Handle, texture.Target))
                {
                    textureUnit++;
                }
            }

            foreach (var param in Material.FloatParams)
            {
                shader.SetUniform1(param.Key, param.Value);
            }

            foreach (var param in Material.VectorParams)
            {
                shader.SetUniform4(param.Key, param.Value);
            }

            if (IsBlended)
            {
                GL.DepthMask(false);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, isAdditiveBlend ? BlendingFactor.One : BlendingFactor.OneMinusSrcAlpha);
            }

            if (isOverlay)
            {
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(-0.05f, -64);
            }

            if (isRenderBackfaces)
            {
                GL.Disable(EnableCap.CullFace);
            }
        }

        public void PostRender()
        {
            if (IsBlended)
            {
                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);
            }

            if (isOverlay)
            {
                GL.Disable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(0, 0);
            }

            if (isRenderBackfaces)
            {
                GL.Enable(EnableCap.CullFace);
            }
        }
    }
}
