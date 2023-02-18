﻿using schema.binary;
using schema.binary.attributes.ignore;

namespace fin.schema.data {
  /// <summary>
  ///   Schema class that implements a uint32-sized section without needing to
  ///   worry about passing in an instance of the contained data. This should
  ///   be adequate for most cases, except when the data class needs to access
  ///   parent data.
  /// </summary>
  [BinarySchema]
  public partial class AutoUInt32SizedSection<T> : ISizedSection<T>
      where T : IBinaryConvertible, new() {
    private readonly PassThruUint32SizedSection<T> impl_;

    [Ignore]
    public T Data => this.impl_.Data;

    public AutoUInt32SizedSection() {
      this.impl_ = new(new T());
    }
  }
}
