﻿using schema.binary;
using schema.binary.attributes.array;

namespace visceral.schema.str.content {
  [BinarySchema]
  public partial class UncompressedData : IContent {
    [ArrayUntilEndOfStream]
    public byte[] Bytes { get; set; }
  }
}