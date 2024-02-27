﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SharpGLTF.Schema2
{
    /// <summary>
    /// Represents a material sub-channel, which usually contains a texture.<br/>
    /// Use <see cref="Material.Channels"/> and <see cref="Material.FindChannel(string)"/> to access it.
    /// </summary>
    /// <remarks>
    /// This structure is not part of the gltf schema,
    /// but wraps several components of the material
    /// to have an homogeneous and easy to use API.
    /// </remarks>
    [System.Diagnostics.DebuggerDisplay("Channel {_Key}")]
    public readonly struct MaterialChannel
    {
        #region lifecycle

        internal MaterialChannel(Material m, String key, Func<Boolean, TextureInfo> texInfo, params IMaterialParameter[] parameters)
        {
            Guard.NotNull(m, nameof(m));
            Guard.NotNullOrEmpty(key, nameof(key));

            Guard.NotNull(texInfo, nameof(texInfo));
            Guard.NotNull(parameters, nameof(parameters));

            _Key = key;
            _Material = m;

            _TextureInfo = texInfo;
            _Parameters = parameters;
        }

        #endregion

        #region data

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly Material _Material;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly String _Key;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly Func<Boolean, TextureInfo> _TextureInfo;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        private readonly IReadOnlyList<IMaterialParameter> _Parameters;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (_Material == null) return 0;

            return _Material.GetHashCode() ^ _Key.GetHashCode(StringComparison.InvariantCulture);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) { return obj is MaterialChannel other && Equals(other); }

        public bool Equals(MaterialChannel other)
        {
            if (this._Material == null && other._Material == null) return true;
            if (this._Material != other._Material) return false;
            if (this._Key != other._Key) return false;
            return true;
        }

        #endregion

        #region properties

        public Material LogicalParent => _Material;

        public String Key => _Key;

        public Boolean HasDefaultContent => _CheckHasDefaultContent();

        /// <summary>
        /// Gets or sets the <see cref="Vector4"/> parameter of this channel.
        /// The meaning of the <see cref="Vector4.X"/>, <see cref="Vector4.Y"/>. <see cref="Vector4.Z"/> and <see cref="Vector4.W"/>
        /// depend on the type of channel.
        /// </summary>
        [Obsolete("Use Parameters[]")]
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public Vector4 Parameter
        {
            get => _MaterialParameter<float>.Combine(_Parameters);
            set => _MaterialParameter<float>.Apply(_Parameters, value);
        }

        public IReadOnlyList<IMaterialParameter> Parameters => _Parameters;

        /// <summary>
        /// Gets the <see cref="Texture"/> instance used by this Material, or null.
        /// </summary>
        public Texture Texture => _GetTexture();

        /// <summary>
        /// Gets the index of texture's TEXCOORD_[index] attribute used for texture coordinate mapping.
        /// </summary>
        public int TextureCoordinate => _TextureInfo?.Invoke(false)?.TextureCoordinate ?? 0;

        public TextureTransform TextureTransform => _TextureInfo?.Invoke(false)?.Transform;

        public TextureSampler TextureSampler => Texture?.Sampler;

        public Vector4 Color
        {
            get
            {
                var p4 = _Parameters.OfType<_MaterialParameter<Vector4>>().SingleOrDefault();
                if (p4.Name == "RGBA") return p4.Value;

                var p3 = _Parameters.OfType<_MaterialParameter<Vector3>>().SingleOrDefault();
                if (p3.Name == "RGB") return new Vector4(p3.Value, 1);

                throw new InvalidOperationException("RGB or RGBA not found.");
            }
            set
            {
                var p4 = _Parameters.OfType<_MaterialParameter<Vector4>>().SingleOrDefault();
                if (p4.Name == "RGBA") { p4.Value = value; return; }

                var p3 = _Parameters.OfType<_MaterialParameter<Vector3>>().SingleOrDefault();
                if (p3.Name == "RGB") { p3.Value = new Vector3(value.X, value.Y, value.Z); return; }

                throw new InvalidOperationException("RGB or RGBA not found.");
            }
        }

        #endregion

        #region API

        public float GetFactor(string key)
        {
            return _Parameters
                .OfType<_MaterialParameter<float>>()
                .Single(item => item.Name == key)
                .Value;
        }

        public void SetFactor(string key, float value)
        {
            _Parameters
                .OfType<_MaterialParameter<float>>()
                .Single(item => item.Name == key)
                .Value = value;
        }

        private Texture _GetTexture()
        {
            var texInfo = _TextureInfo?.Invoke(false);
            if (texInfo == null) return null;

            return _Material.LogicalParent.LogicalTextures[texInfo._LogicalTextureIndex];
        }

        public Texture SetTexture(
            int texCoord,
            Image primaryImg,
            Image fallbackImg = null,
            TextureWrapMode ws = TextureWrapMode.REPEAT,
            TextureWrapMode wt = TextureWrapMode.REPEAT,
            TextureMipMapFilter min = TextureMipMapFilter.DEFAULT,
            TextureInterpolationFilter mag = TextureInterpolationFilter.DEFAULT)
        {
            if (primaryImg == null) return null; // in theory, we should completely remove the TextureInfo

            Guard.NotNull(_Material, nameof(_Material));

            if (_TextureInfo == null) throw new InvalidOperationException();

            var sampler = _Material.LogicalParent.UseTextureSampler(ws, wt, min, mag);
            var texture = _Material.LogicalParent.UseTexture(primaryImg, fallbackImg, sampler);

            SetTexture(texCoord, texture);

            return texture;
        }

        public void SetTexture(int texSet, Texture tex)
        {
            Guard.NotNull(tex, nameof(tex));
            Guard.MustShareLogicalParent(_Material, tex, nameof(tex));

            if (_TextureInfo == null) throw new InvalidOperationException();

            var texInfo = _TextureInfo(true);

            texInfo.TextureCoordinate = texSet;
            texInfo._LogicalTextureIndex = tex.LogicalIndex;
        }

        public void SetTransform(Vector2 offset, Vector2 scale, float rotation = 0, int? texCoordOverride = null)
        {
            if (_TextureInfo == null) throw new InvalidOperationException();

            var texInfo = _TextureInfo(true);

            texInfo.SetTransform(offset, scale, rotation, texCoordOverride);
        }

        private bool _CheckHasDefaultContent()
        {
            if (this.Texture != null) return false;
            if (!this._Parameters.All(item => item.IsDefault)) return false;
            return true;
        }

        #endregion
    }
}
