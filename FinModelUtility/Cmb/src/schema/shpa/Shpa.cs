﻿using System.IO;

using fin.model;
using fin.util.asserts;

using schema.binary;

namespace cmb.schema.shpa {
  public class Shpa : IBinaryDeserializable {
    public Posi Posi { get; } = new();
    public Norm Norm { get; } = new();
    public Idxs Idxs { get; } = new();

    public void Read(IEndianBinaryReader r) {
      r.AssertMagicText("shpa");

      var headerLength = r.ReadUInt32();
      var unk0 = r.ReadUInt32();
      r.AssertUInt32(1); // Animation count?
      var name = r.ReadString(16);
      var unk1 = r.ReadUInt32();
      var posiOffset = r.ReadUInt32();
      var normOffset = r.ReadUInt32();
      var idxsOffset = r.ReadUInt32();

      Asserts.Equal(posiOffset, r.Position);
      this.Posi.Read(r);

      Asserts.Equal(normOffset, r.Position);
      this.Norm.Read(r);

      Asserts.Equal(idxsOffset, r.Position);
      this.Idxs.Read(r);
    }
  }

  public class Posi : IBinaryDeserializable {
    public Position[] Values { get; private set; }

    public void Read(IEndianBinaryReader r) {
      r.AssertMagicText("posi");

      var count = (r.ReadInt32() - 8) / 4 / 3;
      this.Values = new Position[count];
      for (var i = 0; i < count; ++i) {
        this.Values[i] = new Position {
            X = r.ReadSingle(),
            Y = r.ReadSingle(),
            Z = r.ReadSingle()
        };
      }
    }
  }

  public class Norm : IBinaryDeserializable {
    public Normal[] Values { get; private set; }

    public void Read(IEndianBinaryReader r) {
      r.AssertMagicText("norm");

      var count = (r.ReadInt32() - 8) / 2 / 3;
      this.Values = new Normal[count];
      for (var i = 0; i < count; ++i) {
        this.Values[i] = new Normal {
            X = r.ReadSn16(),
            Y = r.ReadSn16(),
            Z = r.ReadSn16()
        };
      }
    }
  }

  [BinarySchema]
  public partial class Idxs : IBinaryConvertible {
    private readonly string magic_ = "idxs";

    /// <summary>
    ///   The corresponding indices in the original model to update?
    /// </summary>
    [ArrayLengthSource(SchemaIntegerType.INT32)]
    public ushort[] Indices { get; private set; }
  }
}