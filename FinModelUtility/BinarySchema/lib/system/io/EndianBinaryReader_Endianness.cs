﻿namespace System.IO {
  public sealed partial class EndianBinaryReader : IEndiannessStack {
    public Endianness Endianness {
      get => this.BufferedStream_.Endianness;
      set => this.BufferedStream_.Endianness = value;
    }

    public bool IsOppositeEndiannessOfSystem
      => this.BufferedStream_.IsOppositeEndiannessOfSystem;

    public void PushClassEndianness(Endianness endianness)
      => this.BufferedStream_.PushClassEndianness(endianness);

    public void PushFieldEndianness(Endianness endianness)
      => this.BufferedStream_.PushFieldEndianness(endianness);

    public void PopEndianness() => this.BufferedStream_.PopEndianness();
  }
}