﻿using fin.data;
using fin.image;
using fin.io;
using fin.model;
using fin.model.impl;
using OpenTK.Graphics.OpenGL;
using Quad64;
using Quad64.Scripts;
using Quad64.src.JSON;
using Quad64.src.LevelInfo;
using Quad64.src.Scripts;


namespace sm64.api {
  public class Sm64LevelModelFileBundle : IModelFileBundle {
    public Sm64LevelModelFileBundle(
        IFile sm64Rom,
        LevelId levelId) {
      this.Sm64Rom = sm64Rom;
      this.LevelId = levelId;
    }

    public IFileHierarchyFile MainFile => null!;

    public IFile Sm64Rom { get; }
    public LevelId LevelId { get; }
    string IUiFile.FileName => $"{LevelId}";
  }

  public class Sm64LevelModelLoader : IModelLoader<Sm64LevelModelFileBundle> {
    // TODO: Load this as a scene instead

    public IModel LoadModel(Sm64LevelModelFileBundle levelModelFileBundle) {
      var level = Sm64LevelModelLoader.LoadLevel_(levelModelFileBundle);

      var finModel = new ModelImpl();

      foreach (var area in level.Areas) {
        Sm64LevelModelLoader.AddAreaToModel_(finModel, area);
      }

      return finModel;
    }

    private static Level LoadLevel_(
        Sm64LevelModelFileBundle levelModelFileBundle) {
      ROM rom = ROM.Instance;

      rom.readFile(levelModelFileBundle.Sm64Rom.FullName);

      Globals.objectComboEntries.Clear();
      Globals.behaviorNameEntries.Clear();
      BehaviorNameFile.parseBehaviorNames(
          Globals.getDefaultBehaviorNamesPath());
      ModelComboFile.parseObjectCombos(Globals.getDefaultObjectComboPath());
      rom.setSegment(0x15, Globals.seg15_location[0], Globals.seg15_location[1],
                     false, null);
      rom.setSegment(0x02, Globals.seg02_location[0], Globals.seg02_location[1],
                     rom.isSegmentMIO0(0x02, null), rom.Seg02_isFakeMIO0,
                     rom.Seg02_uncompressedOffset, null);

      var level = new Level((ushort)levelModelFileBundle.LevelId, 1);
      LevelScripts.parse(ref level, 0x15, 0);
      level.sortAndAddNoModelEntries();
      level.CurrentAreaID = level.Areas[0].AreaID;

      return level;
    }

    private static void AddAreaToModel_(IModel finModel, Area area) {
      var scale = 1; //Constants.LEVEL_SCALE;

      GL.Color3(1f, 1f, 1f);

      var lazyTextureDictionary = new LazyDictionary<Texture2D, ITexture>(
          sm64Texture => {
            var finTexture = finModel.MaterialManager.CreateTexture(
                FinImage.FromBitmap(sm64Texture.Bmp));

            finTexture.WrapModeU =
                ConvertFromGlWrap_((TextureWrapMode)sm64Texture.TextureParamS);
            finTexture.WrapModeV =
                ConvertFromGlWrap_((TextureWrapMode)sm64Texture.TextureParamT);

            return finTexture;
          });
      var lazyMaterialDictionary = new LazyDictionary<Texture2D, IMaterial>(
          sm64Texture =>
              finModel.MaterialManager.AddTextureMaterial(
                  lazyTextureDictionary[sm64Texture]));

      var sm64Model = area.AreaModel.HighestLod;
      foreach (var sm64Mesh in sm64Model.meshes) {
        var geometryMode = sm64Mesh.Material.GeometryMode;

        var cullFront = geometryMode.HasFlag(RspGeometryMode.G_CULL_FRONT);
        var cullBack = geometryMode.HasFlag(RspGeometryMode.G_CULL_BACK);
        var finCullingMode = cullFront switch {
            false => cullBack switch {
                true  => CullingMode.SHOW_FRONT_ONLY,
                false => CullingMode.SHOW_BOTH,
            },
            true => cullBack switch {
                false => CullingMode.SHOW_BACK_ONLY,
                true  => CullingMode.SHOW_NEITHER,
            },
        };

        var finMaterial = lazyMaterialDictionary[sm64Mesh.texture];

        var indices = sm64Mesh.indices;
        var colors = sm64Mesh.colors;
        var vertices = sm64Mesh.vertices;
        var uvs = sm64Mesh.texCoord;

        var finVertices = new List<IVertex>();
        foreach (var vertexIndex in indices) {
          var uv = uvs[vertexIndex];
          var color = colors[vertexIndex];
          var vertex = vertices[vertexIndex];

          finVertices.Add(
              finModel.Skin.AddVertex(vertex.X, vertex.Y, vertex.Z)
                      .SetUv(uv.X, uv.Y)
                      .SetColorBytes(
                          (byte)(255 * color.X),
                          (byte)(255 * color.Y),
                          (byte)(255 * color.Z),
                          (byte)(255 * color.W)));
        }

        var finMesh = finModel.Skin.AddMesh();
        finMesh.AddTriangles(finVertices.ToArray())
               .SetMaterial(finMaterial)
               .SetVertexOrder(VertexOrder.NORMAL);
      }
    }

    private static WrapMode ConvertFromGlWrap_(
        TextureWrapMode wrapMode) =>
        wrapMode switch {
            TextureWrapMode.ClampToEdge    => WrapMode.CLAMP,
            TextureWrapMode.Repeat         => WrapMode.REPEAT,
            TextureWrapMode.MirroredRepeat => WrapMode.MIRROR_REPEAT,
            _ => throw new ArgumentOutOfRangeException(
                     nameof(wrapMode), wrapMode, null)
        };
  }
}