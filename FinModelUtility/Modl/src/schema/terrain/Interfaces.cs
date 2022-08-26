﻿using fin.data;
using fin.schema.color;

using schema;


namespace modl.schema.terrain {
  public record BwSection(string Name, int Size, long Offset);
  
  [BinarySchema]
  public partial class BwHeightmapMaterial : IBiSerializable {
    [StringLengthSource(16)]
    public string Texture1 { get; private set; }

    [StringLengthSource(16)]
    public string Texture2 { get; private set; }

    public byte[] Unknown { get; } = new byte[16];
  }

  public interface IBwHeightmap {
    Grid<IBwHeightmapChunk?> Chunks { get; } 
  }

  public interface IBwHeightmapChunk {
    Grid<IBwHeightmapTile> Tiles { get; }
  }

  public interface IBwHeightmapTile {
    Grid<IBwHeightmapPoint> Points { get; }
  }

  public interface IBwHeightmapPoint {
    int X { get; }
    int Y { get; }
    ushort Height { get; }

    Rgba32 LightColor { get; }
  }
}