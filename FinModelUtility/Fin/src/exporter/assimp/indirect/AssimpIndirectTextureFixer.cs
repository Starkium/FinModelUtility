﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using Assimp;

using fin.model;

namespace fin.exporter.assimp.indirect {
  using WrapMode = fin.model.WrapMode;
  using FinBlendMode = fin.model.BlendMode;

  public class AssimpIndirectTextureFixer {
    public void Fix(IModel model, Scene sc) {
      // Imports the textures
      var finTextures = new HashSet<ITexture>();
      foreach (var finMaterial in model.MaterialManager.All) {
        foreach (var finTexture in finMaterial.Textures) {
          finTextures.Add(finTexture);
        }
      }

      var originalMaterialOrder =
          sc.Materials.Select(material => material.Name).ToArray();

      sc.Textures.Clear();

      foreach (var finTexture in finTextures) {
        var imageData = finTexture.ImageData;

        var imageBytes = new MemoryStream();
        imageData.Save(imageBytes, ImageFormat.Png);

        var assTexture =
            new EmbeddedTexture("png",
                                imageBytes.ToArray(),
                                finTexture.Name);
        assTexture.Filename = finTexture.Name + ".png";

        sc.Textures.Add(assTexture);
      }

      // Need to keep order the same because Assimp references them by index.
      for (var m = 0; m < originalMaterialOrder.Length; ++m) {
        var originalMaterialName = originalMaterialOrder[m];
        var finMaterial =
            model.MaterialManager.All
                 .FirstOrDefault(finMaterial => finMaterial.Name == originalMaterialName);

        if (finMaterial == null) {
          continue;
        }
        
        var assMaterial = new Material {Name = finMaterial.Name};

        if (finMaterial is ILayerMaterial layerMaterial) {
          var addLayers =
              layerMaterial
                  .Layers
                  .Where(layer => layer.BlendMode == FinBlendMode.ADD)
                  .ToArray();
          var multiplyLayers =
              layerMaterial
                  .Layers
                  .Where(layer => layer.BlendMode == FinBlendMode.MULTIPLY)
                  .ToArray();

          if (addLayers.Length == 0) {
            //throw new NotSupportedException("Expected to find an add layer!");
          }
          if (addLayers.Length > 1) {
            ;
          }
          if (addLayers.Length > 2) {
            //throw new NotSupportedException("Too many add layers for GLTF!");
          }

          for (var i = 0; i < addLayers.Length; ++i) {
            var layer = addLayers[i];

            // TODO: Simplify/cut down on redundant logic
            // TODO: Support flat color layers by generating a 1x1 clamped texture of that color.
            if (layer.ColorSource is ITexture finTexture) {
              var assTextureSlot = new TextureSlot {
                  FilePath = finTexture.Name + ".png",
                  WrapModeU = this.ConvertWrapMode_(finTexture.WrapModeU),
                  WrapModeV = this.ConvertWrapMode_(finTexture.WrapModeV)
              };

              // TODO: FBX doesn't support mirror. Blegh

              if (i == 0) {
                assTextureSlot.TextureType = TextureType.Diffuse;
              } else {
                assTextureSlot.TextureType = TextureType.Emissive;
              }

              // TODO: Set blend mode
              //assTextureSlot.Operation =

              assTextureSlot.UVIndex = layer.TexCoordIndex;

              // TODO: Set texture coord type

              assMaterial.AddMaterialTexture(assTextureSlot);
            }
          }

          // Meshes should already have material indices set.
          sc.Materials[m] = assMaterial;
        } else if (finMaterial is ITextureMaterial textureMaterial) {
          var finTexture = textureMaterial.Texture;
          var assTextureSlot = new TextureSlot {
              FilePath = finTexture.Name + ".png",
              WrapModeU = this.ConvertWrapMode_(finTexture.WrapModeU),
              WrapModeV = this.ConvertWrapMode_(finTexture.WrapModeV)
          };

          assTextureSlot.TextureType = TextureType.Diffuse;

          assTextureSlot.UVIndex = 0;

          assMaterial.AddMaterialTexture(assTextureSlot);

          // Meshes should already have material indices set.
          sc.Materials[m] = assMaterial;
        }
      }
    }

    private TextureWrapMode ConvertWrapMode_(WrapMode wrapMode)
      => wrapMode switch {
          WrapMode.CLAMP         => TextureWrapMode.Clamp,
          WrapMode.REPEAT        => TextureWrapMode.Wrap,
          WrapMode.MIRROR_REPEAT => TextureWrapMode.Mirror,
          _ => throw new ArgumentOutOfRangeException(
                   nameof(wrapMode),
                   wrapMode,
                   null)
      };
  }
}