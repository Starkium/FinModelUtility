﻿using NUnit.Framework;


namespace schema.binary.attributes.array {
  internal class ArrayUntilEndOfStreamAttribute {
    [Test] public void TestArrayUntilEndOfStream() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.array;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    [ArrayUntilEndOfStream]
    public byte[] Field { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadBytes(er.Length - er.Position);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteBytes(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestListUntilEndOfStream() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.Collections.Generic;

using schema.binary;
using schema.binary.attributes.array;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    [ArrayUntilEndOfStream]
    public List<byte> Field { get; } = new();
  }
}",
                                           @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      {
        this.Field.Clear();
        while (!er.Eof) {
          this.Field.Add(er.ReadByte());
        }
      }
    }
  }
}
",
                                           @"using System;
using System.IO;
namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteBytes(this.Field);
    }
  }
}
");
    }
  }
}