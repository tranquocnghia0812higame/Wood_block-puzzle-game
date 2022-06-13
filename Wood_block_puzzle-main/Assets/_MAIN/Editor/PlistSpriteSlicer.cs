using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// This is only useful for spritesheets that use plist file
public class PlistSpriteSlicer
{
    [MenuItem("Tools/Slice PLIST Spritesheets")]
    public static void Slice()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        foreach (var texture in textures)
        {
            ProcessTexture(texture);
        }
    }

    static void ProcessTexture(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.spritePivot = Vector2.down;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var textureSettings = new TextureImporterSettings(); // need this stupid class because spriteExtrude and spriteMeshType aren't exposed on TextureImporter
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.Tight;
        textureSettings.spriteExtrude = 0;

        importer.SetTextureSettings(textureSettings);

        // Parse plist file
        string plistPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".plist");
        string folderPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(plistPath);
        XmlNode rootNode = xmlDoc.DocumentElement.ChildNodes[0];
        XmlNode textureInfo = rootNode.ChildNodes[1];
        var metas = new List<SpriteMetaData>();

        for (int i = 0; i < textureInfo.ChildNodes.Count; i += 2)
        {
            var textureMeta = new SpriteMetaData();

            string textureFileName = textureInfo.ChildNodes[i].InnerText;
            textureMeta.name = textureFileName;
            
            XmlNode detailNode = textureInfo.ChildNodes[i + 1];
            bool rotated = false;

            for (int keyIndex = 0; keyIndex < detailNode.ChildNodes.Count; keyIndex += 2)
            {
                if (string.Compare(detailNode.ChildNodes[keyIndex].InnerText, "frame") == 0)
                {
                    string texturePositionString = detailNode.ChildNodes[keyIndex + 1].InnerText;
                    List<string> pairStringList = new List<string>();
                    StringBuilder currentPair = new StringBuilder();
                    foreach (char c in texturePositionString)
                    {
                        if (c == '{')
                        {
                            currentPair.Clear();
                        }
                        else if (c == '}')
                        {
                            if (currentPair.Length == 0)
                                continue;

                            pairStringList.Add(currentPair.ToString());
                            currentPair.Clear();
                        }
                        else
                        {
                            currentPair.Append(c);
                        }
                    }

                    if (pairStringList.Count != 2)
                        break;

                    string[] positions = pairStringList[0].Split(',');
                    Vector2 position = new Vector2(int.Parse(positions[0]), texture.height - int.Parse(positions[1]));

                    string[] sizes = pairStringList[1].Split(',');
                    Vector2 size = new Vector2(int.Parse(sizes[0]), int.Parse(sizes[1]));

                    Rect textureRect = new Rect(position, size);

                    textureMeta.pivot = Vector2.zero;
                    textureMeta.alignment = (int) SpriteAlignment.Center;
                    textureMeta.rect = textureRect;
                }
                else if (string.Compare(detailNode.ChildNodes[keyIndex].InnerText, "rotated") == 0)
                {
                    Vector2 position = textureMeta.rect.position;
                    Vector2 size = textureMeta.rect.size;
                    if (string.Compare(detailNode.ChildNodes[keyIndex + 1].Name, "true") == 0)
                    {
                        size = new Vector2(size.y, size.x);
                        rotated = true;
                    }

                    position = new Vector2(position.x, position.y - size.y);
                    textureMeta.rect = new Rect(position, size);
                }
            }

            // metas.Add(textureMeta);

            CropAndSaveTexture(texture, textureMeta.rect, rotated, Path.Combine(folderPath, textureMeta.name + ".png"));
        }

        // importer.spritesheet = metas.ToArray();

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }

    static void CropAndSaveTexture(Texture2D texture, Rect r, bool rotated, string path)
    {
        if (r.height < 0 || r.width < 0)
        {
            return;
        }

        Texture2D result = new Texture2D((int) r.width, (int) r.height);

        result.SetPixels(texture.GetPixels(Mathf.FloorToInt(r.x), Mathf.FloorToInt(r.y), Mathf.FloorToInt(r.width), Mathf.FloorToInt(r.height)));
        result.Apply();

        byte[] bytes;
        if (rotated)
        {
            bytes = RotateTexture(result, false).EncodeToPNG();
        }
        else
        {
            bytes = result.EncodeToPNG();
        }

        File.WriteAllBytes(path, bytes);
    }

    static Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }
}