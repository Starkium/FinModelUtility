﻿using schema.binary;


namespace modl.schema.modl.common {

  [BinarySchema]
  public partial class VertexPosition : IBinaryConvertible {
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }
  }
}
