﻿using System;
using System.IO;

using fin.util.strings;

using schema.binary;

namespace cmb.schema.cmb {
  public class Primitive : IBinaryConvertible {
    public uint chunkSize;
    public bool isVisible;
    public PrimitiveMode primitiveMode;
    public DataType dataType;
    public ushort indicesCount;
    public uint[] indices;
    public ushort offset;

    public void Read(IEndianBinaryReader r) {
      r.AssertMagicText("prm" + AsciiUtil.GetChar(0x20));

      this.chunkSize = r.ReadUInt32();
      this.isVisible = r.ReadUInt32() != 0;

      // Other modes don't exist in OoT3D's shader so we'd never know
      this.primitiveMode = (PrimitiveMode) r.ReadUInt32();
      this.dataType = (DataType) r.ReadUInt32();

      this.indicesCount = r.ReadUInt16();

      this.offset = r.ReadUInt16();
    }

    public void Write(ISubEndianBinaryWriter w)
      => throw new NotImplementedException();
  }
}