﻿using System.IO;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.image.io {
  /// <summary>
  ///   Helper class for reading 8-bit luminance pixels.
  /// </summary>
  public class L8PixelReader : IPixelReader<L8> {
    public IImage<L8> CreateImage(int width, int height)
      => new I8Image(width, height);

    public unsafe void Decode(IEndianBinaryReader er,
                                          L8* scan0,
                                          int offset)
      => scan0[offset] = new L8(er.ReadByte());
  }
}