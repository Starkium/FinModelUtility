﻿using fin.data;

using schema;


namespace modl.schema.terrain.bw1 {
  public class Bw1Heightmap : IBwHeightmap, IDeserializable {
    public Grid<IBwHeightmapChunk?> Chunks { get; private set; }

    public void Read(EndianBinaryReader er) {
      var sections = new Dictionary<string, BwSection>();
      while (!er.Eof) {
        var name = er.ReadStringEndian(4);
        var size = er.ReadInt32();
        var offset = er.Position;

        sections[name] = new BwSection(name, size, offset);

        er.Position += size;
      }

      var chnkSection = sections["CHNK"];
      er.Position = chnkSection.Offset;
      var tilesBytes = er.ReadBytes(chnkSection.Size);

      var cmapSection = sections["CMAP"];
      er.Position = cmapSection.Offset;
      var tilemapBytes = er.ReadBytes(cmapSection.Size);

      var matlSection = sections["MATL"];
      er.Position = matlSection.Offset;
      var materialCount = matlSection.Size / 48;
      er.ReadNewArray<BwHeightmapMaterial>(out var materials, materialCount);

      var heightmapParser =
          new HeightmapParser(er, chnkSection.Offset, materialCount,
                              tilemapBytes, tilesBytes);
      this.Chunks = heightmapParser.Chunks;
    }
  }
}