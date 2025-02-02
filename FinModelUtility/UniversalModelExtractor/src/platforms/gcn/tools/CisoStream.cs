﻿using schema.binary.util;

namespace uni.platforms.gcn.tools {
  public class CisoStream : Stream {
    private readonly Stream impl_;
    private readonly long offset_ = 0x8000;
    private readonly bool[] blockMap_;
    private const long ROM_SIZE = 1_459_978_240;

    private readonly uint blockSize_;

    public CisoStream(Stream impl) {
      this.impl_ = impl;

      var er = new EndianBinaryReader(impl, Endianness.LittleEndian);
      er.AssertString("CISO");

      this.blockSize_ = er.ReadUInt32();
      Asserts.True(this.blockSize_ is > 0 and <= 0x8000000);

      this.blockMap_ = er.ReadBytes(this.offset_ - 8)
                         .Select(value => value == 1)
                         .ToArray();
    }

    public override void Flush() => this.impl_.Flush();

    public override int ReadByte() {
      if (this.Position >= this.Length) {
        return -1;
      }

      Span<byte> value = stackalloc byte[1];
      this.Read(value);
      return value[0];
    }

    public override int Read(byte[] buffer, int offset, int count)
      => this.Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer) {
      int dstOffset = 0;
      while (dstOffset < buffer.Length) {
        var remainingInBlock = GetRemainingBytesInBlock_(this.Position);
        var copyAmount = Math.Min(buffer.Length - dstOffset, remainingInBlock);
        var currentSlice = buffer.Slice(dstOffset, copyAmount);

        if (this.GetCisoPosition_(this.Position,
                                  out var cisoPosition)) {
          this.impl_.Position = cisoPosition;
          this.impl_.Read(currentSlice);
        }
        // Otherwise, fills block with 0s.
        else {
          currentSlice.Fill(0);
        }

        dstOffset += copyAmount;
        this.Position += copyAmount;
      }

      return dstOffset;
    }

    private int GetRemainingBytesInBlock_(long originalPosition)
      => (int) (this.blockSize_ - (originalPosition % this.blockSize_));

    private bool GetCisoPosition_(long originalPosition,
                                  out long cisoPosition) {
      var blockIndex = originalPosition / this.blockSize_;
      if (!this.blockMap_[blockIndex]) {
        cisoPosition = -1;
        return false;
      }

      cisoPosition = this.offset_ + originalPosition % this.blockSize_;
      for (var i = 0; i < blockIndex; ++i) {
        if (this.blockMap_[i]) {
          cisoPosition += this.blockSize_;
        }
      }

      return true;
    }

    public override long Seek(long offset, SeekOrigin origin)
      => throw new NotImplementedException();

    public override void SetLength(long value)
      => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count)
      => throw new NotImplementedException();

    public override bool CanRead => this.impl_.CanRead;
    public override bool CanSeek => this.impl_.CanSeek;
    public override bool CanWrite => this.impl_.CanWrite;
    public override long Length => ROM_SIZE;

    public override long Position { get; set; }
  }
}