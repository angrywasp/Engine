﻿using System;
using System.Collections.Generic;
using System.Text;

using SharpGLTF.SchemaReflection;

namespace SharpGLTF
{
    class SpecularGlossinessExtension : SchemaProcessor
    {
        private static string SchemaUri => Constants.ArchivedExtensionPath("KHR_materials_pbrSpecularGlossiness", "glTF.KHR_materials_pbrSpecularGlossiness.schema.json");
        public override IEnumerable<(string, SchemaType.Context)> Process()
        {
            var ctx = SchemaProcessing.LoadSchemaContext(SchemaUri);
            ctx.IgnoredByCodeEmitter("glTF Property");
            ctx.IgnoredByCodeEmitter("glTF Child of Root Property");
            ctx.IgnoredByCodeEmitter("Texture Info");

            ctx.FindClass("KHR_materials_pbrSpecularGlossiness glTF extension")
                .GetField("diffuseFactor")
                .SetDataType(typeof(System.Numerics.Vector4), true)
                .SetDefaultValue("Vector4.One")
                .SetItemsRange(0);

            ctx.FindClass("KHR_materials_pbrSpecularGlossiness glTF extension")
                .GetField("specularFactor")
                .SetDataType(typeof(System.Numerics.Vector3), true)
                .SetDefaultValue("Vector3.One")
                .SetItemsRange(0);

            yield return ("ext.pbrSpecularGlossiness.g", ctx);
        }

        public override void PrepareTypes(CodeGen.CSharpEmitter newEmitter, SchemaType.Context ctx)
        {
            newEmitter.SetRuntimeName("KHR_materials_pbrSpecularGlossiness glTF extension", "MaterialPBRSpecularGlossiness");
        }
    }
}
