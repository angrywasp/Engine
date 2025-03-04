// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
//
// Author: Kenneth James Pouncey

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public struct RenderTargetBinding
	{
        private readonly Texture renderTarget;
        private readonly int arraySlice;
        private DepthFormat depthFormat;
        private int mipLevel = 0;

		public Texture RenderTarget  => renderTarget;

        public int ArraySlice => arraySlice;

        internal DepthFormat DepthFormat => depthFormat;

        internal int MipLevel => mipLevel;

		public RenderTargetBinding(RenderTarget2D renderTarget)
		{
			if (renderTarget == null) 
				throw new ArgumentNullException("renderTarget");

			this.renderTarget = renderTarget;
            this.arraySlice = (int)CubeMapFace.PositiveX;
            this.depthFormat = renderTarget.DepthStencilFormat;
            this.mipLevel = 0;
		}

        public RenderTargetBinding(RenderTarget2D renderTarget, int mipLevel)
		{
			if (renderTarget == null) 
				throw new ArgumentNullException("renderTarget");

			this.renderTarget = renderTarget;
            this.arraySlice = (int)CubeMapFace.PositiveX;
            this.depthFormat = renderTarget.DepthStencilFormat;
            this.mipLevel = mipLevel;
		}

        public RenderTargetBinding(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (cubeMapFace < CubeMapFace.PositiveX || cubeMapFace > CubeMapFace.NegativeZ)
                throw new ArgumentOutOfRangeException("cubeMapFace");

            this.renderTarget = renderTarget;
            this.arraySlice = (int)(cubeMapFace - CubeMapFace.PositiveX);
            this.depthFormat = renderTarget.DepthStencilFormat;
        }

        public RenderTargetBinding(RenderTargetCube renderTarget, CubeMapFace cubeMapFace, int mipLevel)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (cubeMapFace < CubeMapFace.PositiveX || cubeMapFace > CubeMapFace.NegativeZ)
                throw new ArgumentOutOfRangeException("cubeMapFace");

            this.renderTarget = renderTarget;
            this.arraySlice = (int)(cubeMapFace - CubeMapFace.PositiveX);
            this.depthFormat = renderTarget.DepthStencilFormat;
            this.mipLevel = mipLevel;
        }

        public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }
	}
}
