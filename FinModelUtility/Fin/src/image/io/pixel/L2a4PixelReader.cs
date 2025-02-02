﻿using System.IO;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io {
  /// <summary>
  ///   Helper class for reading 4-bit luminance pixels to both luminance and
  ///   alpha channels.
  /// </summary>
  public class L2a4PixelReader : IPixelReader<La16> {
    public IImage<La16> CreateImage(int width, int height)
      => new Ia16Image(width, height);

    public unsafe void Decode(IEndianBinaryReader er,
                              La16* scan0,
                              int offset) {
      var value = er.ReadByte();

      var upper = (byte) ((value >> 4) * 17);
      var lower = (byte) ((value & 0xF) * 17);

      scan0[offset + 0] = new La16(upper, upper);
      scan0[offset + 1] = new La16(lower, lower);
    }

    public int PixelsPerRead => 2;
  }
}