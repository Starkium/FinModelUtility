﻿using fin.model;

using schema.binary.util;
using schema.text;

namespace xmod.schema.xmod {
  public class Vector4 : ITextDeserializable, IVector4 {
    public Vector4() { }

    public Vector4(float x, float y, float z, float w) {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }

    public void Read(ITextReader tr) {
      var values = tr.ReadSingles(TextReaderConstants.WHITESPACE_STRINGS,
                                  TextReaderConstants.NEWLINE_STRINGS);
      Asserts.Equal(4, values.Length);
      X = values[0];
      Y = values[1];
      Z = values[2];
      W = values[2];
    }
  }
}