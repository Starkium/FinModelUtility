﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;

using fin.color;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.schema.vector;
using fin.util.enumerables;
using fin.util.image;


namespace fin.model.impl {
  public partial class ModelImpl {
    public IMaterialManager MaterialManager { get; } =
      new MaterialManagerImpl();

    private class MaterialManagerImpl : IMaterialManager {
      private IList<IMaterial> materials_ = new List<IMaterial>();
      private IList<ITexture> textures_ = new List<ITexture>();

      public MaterialManagerImpl() {
        this.All = new ReadOnlyCollection<IMaterial>(this.materials_);
        this.Textures = new ReadOnlyCollection<ITexture>(this.textures_);
      }

      public IReadOnlyList<IMaterial> All { get; }

      public INullMaterial AddNullMaterial() {
        var material = new NullMaterialImpl();
        this.materials_.Add(material);
        return material;
      }

      public ITextureMaterial AddTextureMaterial(ITexture texture) {
        var material = new TextureMaterialImpl(texture);
        this.materials_.Add(material);
        return material;
      }

      public IStandardMaterial AddStandardMaterial() {
        var material = new StandardMaterialImpl();
        this.materials_.Add(material);
        return material;
      }

      public IFixedFunctionMaterial AddFixedFunctionMaterial() {
        var material = new FixedFunctionMaterialImpl();
        this.materials_.Add(material);
        return material;
      }

      public ITexture CreateTexture(IImage imageData) {
        var texture = new TextureImpl(imageData);
        this.textures_.Add(texture);
        return texture;
      }

      public IReadOnlyList<ITexture> Textures { get; }
    }

    private class TextureImpl : ITexture {
      private ImageTransparencyType? transparencyType_;
      private Bitmap? imageData_;

      public TextureImpl(IImage image) {
        this.Image = image;
      }

      public string Name { get; set; }
      public int UvIndex { get; set; }
      public UvType UvType { get; set; }

      public ColorType ColorType { get; set; }

      public IImage Image { get; }
      public Bitmap ImageData => this.imageData_ ??= Image.AsBitmap();

      public IFile SaveInDirectory(IDirectory directory) {
        IFile outFile =
            new FinFile(Path.Combine(directory.FullName, this.Name + ".png"));
        using var writer = outFile.OpenWrite();
        this.Image.ExportToStream(writer, LocalImageFormat.PNG);
        return outFile;
      }

      public ImageTransparencyType TransparencyType
        => this.transparencyType_ ??= ImageUtil.GetTransparencyType(this.Image);

      public WrapMode WrapModeU { get; set; }
      public WrapMode WrapModeV { get; set; }

      public IColor? BorderColor { get; set; }

      public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.LINEAR;

      public TextureMinFilter MinFilter { get; set; } =
        TextureMinFilter.LINEAR_MIPMAP_LINEAR;


      public IVector2 Offset { get; } = new Vector2f();

      public ITexture SetOffset(float x, float y) {
        this.Offset.X = x;
        this.Offset.Y = y;
        return this;
      }


      public IVector2 Scale { get; } = new Vector2f { X = 1, Y = 1 };

      public ITexture SetScale(float x, float y) {
        this.Scale.X = x;
        this.Scale.Y = y;
        return this;
      }


      public float RotationDegrees { get; private set; }

      public ITexture SetRotationDegrees(float rotationDegrees) {
        this.RotationDegrees = rotationDegrees;
        return this;
      }


      public override int GetHashCode() {
        int hash = 216613626;
        var sub = 16780669;
        hash = hash * sub ^ Image.GetHashCode();
        hash = hash * sub ^ WrapModeU.GetHashCode();
        hash = hash * sub ^ WrapModeU.GetHashCode();
        return hash;
      }

      public override bool Equals(object? other) {
        if (ReferenceEquals(null, other)) {
          return false;
        }

        if (ReferenceEquals(this, other)) {
          return true;
        }

        if (other is ITexture otherTexture) {
          return this.Image == otherTexture.Image &&
                 this.WrapModeU == otherTexture.WrapModeU &&
                 this.WrapModeV == otherTexture.WrapModeV;
        }

        return false;
      }
    }

    private abstract class BMaterialImpl : IMaterial {
      public abstract IEnumerable<ITexture> Textures { get; }

      public string? Name { get; set; }
      public CullingMode CullingMode { get; set; }

      public DepthMode DepthMode { get; set; }
      public DepthCompareType DepthCompareType { get; set; }

      public bool IgnoreLights { get; set; }
    }

    private class NullMaterialImpl : BMaterialImpl, INullMaterial {
      public override IEnumerable<ITexture> Textures { get; } =
        Array.Empty<ITexture>();
    }

    private class TextureMaterialImpl : BMaterialImpl, ITextureMaterial {
      public TextureMaterialImpl(ITexture texture) {
        this.Texture = texture;
        this.Textures = new ReadOnlyCollection<ITexture>(new[] { texture });
      }

      public ITexture Texture { get; }
      public override IEnumerable<ITexture> Textures { get; }
    }

    private class StandardMaterialImpl : BMaterialImpl, IStandardMaterial {
      public override IEnumerable<ITexture> Textures {
        get {
          if (this.DiffuseTexture != null) {
            yield return this.DiffuseTexture;
          }

          if (this.MaskTexture != null) {
            yield return this.MaskTexture;
          }

          if (this.AmbientOcclusionTexture != null) {
            yield return this.AmbientOcclusionTexture;
          }

          if (this.NormalTexture != null) {
            yield return this.NormalTexture;
          }

          if (this.EmissiveTexture != null) {
            yield return this.EmissiveTexture;
          }

          if (this.SpecularTexture != null) {
            yield return this.SpecularTexture;
          }
        }
      }

      public ITexture? DiffuseTexture { get; set; }
      public ITexture? MaskTexture { get; set; }
      public ITexture? AmbientOcclusionTexture { get; set; }
      public ITexture? NormalTexture { get; set; }
      public ITexture? EmissiveTexture { get; set; }
      public ITexture? SpecularTexture { get; set; }
    }

    private class FixedFunctionMaterialImpl 
        : BMaterialImpl, IFixedFunctionMaterial {
      private readonly List<ITexture> textures_ = new();

      private readonly ITexture?[] texturesSources_ = new ITexture[8];
      private readonly IColor?[] colors_ = new IColor[2];
      private readonly float?[] alphas_ = new float?[2];

      public FixedFunctionMaterialImpl() {
        this.Textures = new ReadOnlyCollection<ITexture>(this.textures_);

        this.TextureSources =
            new ReadOnlyCollection<ITexture?>(this.texturesSources_);
      }

      public override IEnumerable<ITexture> Textures { get; }

      public IFixedFunctionEquations<FixedFunctionSource> Equations { get; } =
        new FixedFunctionEquations<FixedFunctionSource>();

      public IReadOnlyList<ITexture?> TextureSources { get; }

      public IFixedFunctionMaterial SetTextureSource(
          int textureIndex,
          ITexture texture) {
        if (!this.texturesSources_.Contains(texture)) {
          this.textures_.Add(texture);
        }

        this.texturesSources_[textureIndex] = texture;

        return this;
      }

      public ITexture? CompiledTexture { get; set; }

      public IFixedFunctionMaterial SetBlending(
          BlendMode blendMode,
          BlendFactor srcFactor,
          BlendFactor dstFactor,
          LogicOp logicOp) {
        this.BlendMode = blendMode;
        this.SrcFactor = srcFactor;
        this.DstFactor = dstFactor;
        this.LogicOp = logicOp;
        return this;
      }

      public BlendMode BlendMode { get; private set; } = BlendMode.ADD;

      public BlendFactor SrcFactor { get; private set; } =
        BlendFactor.SRC_ALPHA;

      public BlendFactor DstFactor { get; private set; } =
        BlendFactor.ONE_MINUS_SRC_ALPHA;

      public LogicOp LogicOp { get; private set; } = LogicOp.COPY;

      public IFixedFunctionMaterial SetAlphaCompare(
          AlphaOp alphaOp,
          AlphaCompareType alphaCompareType0,
          float reference0,
          AlphaCompareType alphaCompareType1,
          float reference1) {
        this.AlphaOp = alphaOp;
        this.AlphaCompareType0 = alphaCompareType0;
        this.AlphaReference0 = reference0;
        this.AlphaCompareType1 = alphaCompareType1;
        this.AlphaReference1 = reference1;
        return this;
      }

      public AlphaOp AlphaOp { get; private set; }

      public AlphaCompareType AlphaCompareType0 { get; private set; } =
        AlphaCompareType.Always;

      public float AlphaReference0 { get; private set; }

      public AlphaCompareType AlphaCompareType1 { get; private set; } =
        AlphaCompareType.Always;

      public float AlphaReference1 { get; private set; }
    }
  }
}