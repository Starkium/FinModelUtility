﻿using fin.schema.vector;

using schema.binary;

namespace mod.schema {
  [BinarySchema]
  public partial class JointMatPoly : IBinaryConvertible {
    public ushort matIdx = 0;
    public ushort meshIdx = 0;
  }

  [BinarySchema]
  public partial class Joint : IBinaryConvertible {
    public uint parentIdx = 0;
    public uint flags = 0;
    public readonly Vector3f boundsMax = new();    
    public readonly Vector3f boundsMin = new();
    public float volumeRadius = 0;
    public readonly Vector3f scale = new();
    public readonly Vector3f rotation = new();
    public readonly Vector3f position = new();

    [ArrayLengthSource(SchemaIntegerType.UINT32)]
    public JointMatPoly[] matpolys;
  }
}