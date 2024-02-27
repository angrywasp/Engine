// <auto-generated/>

//------------------------------------------------------------------------------------------------
//      This file has been programatically generated; DON´T EDIT!
//------------------------------------------------------------------------------------------------

#pragma warning disable SA1001
#pragma warning disable SA1027
#pragma warning disable SA1028
#pragma warning disable SA1121
#pragma warning disable SA1205
#pragma warning disable SA1309
#pragma warning disable SA1402
#pragma warning disable SA1505
#pragma warning disable SA1507
#pragma warning disable SA1508
#pragma warning disable SA1652

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Text.Json;

namespace SharpGLTF.Schema2
{
	using Collections;

	/// <summary>
	/// glTF extension that defines an iridescence effect.
	/// </summary>
	partial class MaterialIridescence : ExtraProperties
	{
	
		private const Double _iridescenceFactorDefault = 0;
		private const Double _iridescenceFactorMinimum = 0;
		private const Double _iridescenceFactorMaximum = 1;
		private Double? _iridescenceFactor = _iridescenceFactorDefault;
		
		private const Double _iridescenceIorDefault = 1.3;
		private const Double _iridescenceIorMinimum = 1;
		private Double? _iridescenceIor = _iridescenceIorDefault;
		
		private TextureInfo _iridescenceTexture;
		
		private const Double _iridescenceThicknessMaximumDefault = 400;
		private const Double _iridescenceThicknessMaximumMinimum = 0;
		private Double? _iridescenceThicknessMaximum = _iridescenceThicknessMaximumDefault;
		
		private const Double _iridescenceThicknessMinimumDefault = 100;
		private const Double _iridescenceThicknessMinimumMinimum = 0;
		private Double? _iridescenceThicknessMinimum = _iridescenceThicknessMinimumDefault;
		
		private TextureInfo _iridescenceThicknessTexture;
		
	
		protected override void SerializeProperties(Utf8JsonWriter writer)
		{
			base.SerializeProperties(writer);
			SerializeProperty(writer, "iridescenceFactor", _iridescenceFactor, _iridescenceFactorDefault);
			SerializeProperty(writer, "iridescenceIor", _iridescenceIor, _iridescenceIorDefault);
			SerializePropertyObject(writer, "iridescenceTexture", _iridescenceTexture);
			SerializeProperty(writer, "iridescenceThicknessMaximum", _iridescenceThicknessMaximum, _iridescenceThicknessMaximumDefault);
			SerializeProperty(writer, "iridescenceThicknessMinimum", _iridescenceThicknessMinimum, _iridescenceThicknessMinimumDefault);
			SerializePropertyObject(writer, "iridescenceThicknessTexture", _iridescenceThicknessTexture);
		}
	
		protected override void DeserializeProperty(string jsonPropertyName, ref Utf8JsonReader reader)
		{
			switch (jsonPropertyName)
			{
				case "iridescenceFactor": _iridescenceFactor = DeserializePropertyValue<Double?>(ref reader); break;
				case "iridescenceIor": _iridescenceIor = DeserializePropertyValue<Double?>(ref reader); break;
				case "iridescenceTexture": _iridescenceTexture = DeserializePropertyValue<TextureInfo>(ref reader); break;
				case "iridescenceThicknessMaximum": _iridescenceThicknessMaximum = DeserializePropertyValue<Double?>(ref reader); break;
				case "iridescenceThicknessMinimum": _iridescenceThicknessMinimum = DeserializePropertyValue<Double?>(ref reader); break;
				case "iridescenceThicknessTexture": _iridescenceThicknessTexture = DeserializePropertyValue<TextureInfo>(ref reader); break;
				default: base.DeserializeProperty(jsonPropertyName,ref reader); break;
			}
		}
	
	}

}
