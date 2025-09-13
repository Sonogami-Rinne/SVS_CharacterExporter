using System.Collections.Generic;
using SVSExporter.Utils;
using UnityEngine;

[Serializable]
internal class MaterialInfo
{
	public string MaterialName;
	public string ShaderName;
    public bool isHair = false;

    public List<string> ShaderPropNames = new List<string>();
	public List<string> ShaderPropTextureValues = new List<string>();
	public List<List<float>> ShaderPropColorValues = new List<List<float>>();
	public List<float> ShaderPropFloatValues = new List<float>();
	//public List<Color> ShaderPropColorValues = new List<Color>();

    public MaterialInfo(Material material, string _materialName)
	{
		MaterialName = _materialName;
		ShaderName = material.shader.name;
		var properties = material.GetTexturePropertyNames();
		foreach (string property in properties)
		{
			if (material.HasFloat(property))
			{
                ShaderPropNames.Add(property + " " + "Float" + "" + ShaderPropFloatValues.Count);
				ShaderPropFloatValues.Add(material.GetFloat(property));
            }
			else if (material.HasInt(property))
			{
                ShaderPropNames.Add(property + " " + "Float" + "" + ShaderPropFloatValues.Count);
                ShaderPropFloatValues.Add(material.GetInt(property));
            }
			else if (material.HasTexture(property))
			{
                ShaderPropNames.Add(property + " " + "Texture" + " " + ShaderPropTextureValues.Count);
                Texture texture = material.GetTexture(property);
                if (texture == null)
                {
                    ShaderPropTextureValues.Add(null);
                }
                else
                {
                    ShaderPropTextureValues.Add(property);
					TextureSaver.SaveTexture(texture, PmxBuilder.savePath + MaterialName + "_" + property + ".png");
                }
            }
            else if (material.HasColor(property))
            {
                ShaderPropNames.Add(property + " " + "Color" + " " + ShaderPropColorValues.Count);
                ShaderPropColorValues.Add(ConvertColor(material.GetColor(property)));
            }
        }
		//MaterialShader materialShader2 = MaterialShaders.materialShaders.Find((MaterialShader materialShader) => string.CompareOrdinal(materialShader.shaderName, ShaderName) == 0);
		//if (materialShader2 == null)
		//{
		//	return;
		//}
		//foreach (MaterialShaderItem property in materialShader2.properties)
		//{
		//	// Add the color data
  //          string text = "_" + property.name;
		//	if (property.type == "Color")
		//	{
		//		ShaderPropNames.Add(text + " " + property.type + " " + ShaderPropColorValues.Count());
		//		ShaderPropColorValues.Add(ConvertColor(material.GetColor(text)));
  //          }

		//	// check the names of each texture. if there is a texture with "_HGLS" in the name, mark it as a hair shader.
  //          if (property.type == "Texture")
  //          {
  //              Dictionary<string, string> dictionary = PmxBuilder.typeMap.ToDictionary((KeyValuePair<string, string> x) => x.Value, (KeyValuePair<string, string> x) => x.Key);
  //              if (dictionary.ContainsKey(text))
  //              {
  //                  string text2 = dictionary[text];
		//			if (text2 == "_HGLS") {
		//				isHair = true;
  //                  }
  //              }
  //          }

  //      }
    }
	private static List<float> ConvertColor(Color color)
	{
		return new List<float>() { color.r, color.g, color.b, color.a };
	}
}
