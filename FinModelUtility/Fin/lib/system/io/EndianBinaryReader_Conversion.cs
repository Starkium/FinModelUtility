﻿// Decompiled with JetBrains decompiler
// Type: System.IO.EndianBinaryReader
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System.Text;

using schema.binary;


namespace System.IO {
  public sealed partial class EndianBinaryReader {
    private static byte ConvertByte_(byte[] buffer, int i) => buffer[i];

    private static sbyte ConvertSByte_(byte[] buffer, int i)
      => (sbyte) buffer[i];

    private static short ConvertInt16_(byte[] buffer, int i)
      => BitConverter.ToInt16(buffer, i);

    private static ushort ConvertUInt16_(byte[] buffer, int i)
      => BitConverter.ToUInt16(buffer, sizeof(ushort) * i);

    private static int ConvertInt24_(byte[] buffer, int i) {
      var value = (buffer[3 * i + 2] << 16) |
                  (buffer[3 * i + 1] << 8) |
                  buffer[3 * i];

      const int bitMask = -16777216;

      // Stolen from https://github.com/GridProtectionAlliance/gsf/blob/master/Source/Libraries/GSF.Core.Shared/Int24.cs
      // Check bit 23, the sign bit in a signed 24-bit integer
      if ((value & 0x00800000) > 0) {
        // If the sign-bit is set, this number will be negative - set all high-byte bits (keeps 32-bit number in 24-bit range)
        value |= bitMask;
      } else {
        // If the sign-bit is not set, this number will be positive - clear all high-byte bits (keeps 32-bit number in 24-bit range)
        value &= ~bitMask;
      }

      return value;
    }

    private static uint ConvertUInt24_(byte[] buffer, int i)
      => (uint) ((buffer[3 * i + 2] << 16) |
                 (buffer[3 * i + 1] << 8) |
                 buffer[3 * i]);

    private static float ConvertHalf_(byte[] buffer, int i)
      => (float) BitConverter.ToHalf(buffer, i);

    private static float ConvertSn8_(byte[] buffer, int i)
      => EndianBinaryReader.ConvertSByte_(buffer, i) / (255f / 2);

    private static float ConvertUn8_(byte[] buffer, int i)
      => EndianBinaryReader.ConvertByte_(buffer, i) / 255f;

    private static float ConvertSn16_(byte[] buffer, int i)
      => EndianBinaryReader.ConvertInt16_(buffer, i) / (65535f / 2);

    private static float ConvertUn16_(byte[] buffer, int i)
      => EndianBinaryReader.ConvertUInt16_(buffer, i) / 65535f;
  }
}