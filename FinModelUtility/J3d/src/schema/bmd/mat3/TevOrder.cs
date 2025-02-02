﻿using gx;

using schema.binary;


namespace j3d.schema.bmd.mat3 {
  [BinarySchema]
  public partial class TevOrder : IBinaryDeserializable {
    public byte TexCoordId { get; set; }
    public sbyte TexMap { get; set; }
    public GxColorChannel ColorChannelId { get; set; }
    private readonly byte padding_ = 0xff;
  }
}
