﻿using System;

using schema.binary;
using schema.binary.attributes.ignore;


namespace mod.schema.animation {
  public enum AnimationFormat : uint {
    DCA = 2,
    DCK = 3,
  }

  [BinarySchema]
  public partial class DcxWrapper : IBinaryConvertible {
    public AnimationFormat AnimationFormat { get; set; }

    [Ignore]
    private bool IsDca => this.AnimationFormat == AnimationFormat.DCA;

    [IfBoolean(nameof(IsDca))]
    public Dca? Dca { get; set; }

    [Ignore]
    private bool IsDck => this.AnimationFormat == AnimationFormat.DCK;

    [IfBoolean(nameof(IsDck))]
    public Dck? Dck { get; set; }

    [Ignore]
    public IDcx Dcx
      => IsDca ? Dca! :
         IsDck ? Dck! :
         throw new NotSupportedException();

    public override string ToString() => this.Dcx.ToString()!;
  }
}
