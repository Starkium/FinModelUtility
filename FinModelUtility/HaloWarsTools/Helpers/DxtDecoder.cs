﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using fin.math;
using fin.util.asserts;
using fin.util.image;


// From https://github.com/mafaca/Dxt


namespace Dxt {
  public static class DxtDecoder {
    public static void DecompressDXT1(
        byte[] input,
        int width,
        int height,
        byte[] output) {
      int offset = 0;
      int bcw = (width + 3) / 4;
      int bch = (height + 3) / 4;
      int clen_last = (width + 3) % 4 + 1;
      uint[] buffer = new uint[16];
      int[] colors = new int[4];
      for (int t = 0; t < bch; t++) {
        for (int s = 0; s < bcw; s++, offset += 8) {
          int r0, g0, b0, r1, g1, b1;
          int q0 = input[offset + 0] | input[offset + 1] << 8;
          int q1 = input[offset + 2] | input[offset + 3] << 8;
          Rgb565(q0, out r0, out g0, out b0);
          Rgb565(q1, out r1, out g1, out b1);
          colors[0] = Color(r0, g0, b0, 255);
          colors[1] = Color(r1, g1, b1, 255);
          if (q0 > q1) {
            colors[2] = Color((r0 * 2 + r1) / 3,
                              (g0 * 2 + g1) / 3,
                              (b0 * 2 + b1) / 3,
                              255);
            colors[3] = Color((r0 + r1 * 2) / 3,
                              (g0 + g1 * 2) / 3,
                              (b0 + b1 * 2) / 3,
                              255);
          } else {
            colors[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 255);
          }

          uint d = BitConverter.ToUInt32(input, offset + 4);
          for (int i = 0; i < 16; i++, d >>= 2) {
            buffer[i] = unchecked((uint) colors[d & 3]);
          }

          int clen = (s < bcw - 1 ? 4 : clen_last) * 4;
          for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++) {
            Buffer.BlockCopy(buffer,
                             i * 4 * 4,
                             output,
                             (y * width + s * 4) * 4,
                             clen);
          }
        }
      }
    }


    public static void DecompressDXT3(
        byte[] input,
        int width,
        int height,
        byte[] output) {
      int offset = 0;
      int bcw = (width + 3) / 4;
      int bch = (height + 3) / 4;
      int clen_last = (width + 3) % 4 + 1;
      uint[] buffer = new uint[16];
      int[] colors = new int[4];
      int[] alphas = new int[16];
      for (int t = 0; t < bch; t++) {
        for (int s = 0; s < bcw; s++, offset += 16) {
          for (int i = 0; i < 4; i++) {
            int alpha = input[offset + i * 2] | input[offset + i * 2 + 1] << 8;
            alphas[i * 4 + 0] = (((alpha >> 0) & 0xF) * 0x11) << 24;
            alphas[i * 4 + 1] = (((alpha >> 4) & 0xF) * 0x11) << 24;
            alphas[i * 4 + 2] = (((alpha >> 8) & 0xF) * 0x11) << 24;
            alphas[i * 4 + 3] = (((alpha >> 12) & 0xF) * 0x11) << 24;
          }

          int r0, g0, b0, r1, g1, b1;
          int q0 = input[offset + 8] | input[offset + 9] << 8;
          int q1 = input[offset + 10] | input[offset + 11] << 8;
          Rgb565(q0, out r0, out g0, out b0);
          Rgb565(q1, out r1, out g1, out b1);
          colors[0] = Color(r0, g0, b0, 0);
          colors[1] = Color(r1, g1, b1, 0);
          if (q0 > q1) {
            colors[2] = Color((r0 * 2 + r1) / 3,
                              (g0 * 2 + g1) / 3,
                              (b0 * 2 + b1) / 3,
                              0);
            colors[3] = Color((r0 + r1 * 2) / 3,
                              (g0 + g1 * 2) / 3,
                              (b0 + b1 * 2) / 3,
                              0);
          } else {
            colors[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 0);
          }

          uint d = BitConverter.ToUInt32(input, offset + 12);
          for (int i = 0; i < 16; i++, d >>= 2) {
            buffer[i] = unchecked((uint) (colors[d & 3] | alphas[i]));
          }

          int clen = (s < bcw - 1 ? 4 : clen_last) * 4;
          for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++) {
            Buffer.BlockCopy(buffer,
                             i * 4 * 4,
                             output,
                             (y * width + s * 4) * 4,
                             clen);
          }
        }
      }
    }

    public static unsafe Bitmap DecompressDxt5a(
        byte[] src,
        int srcOffset,
        int width,
        int height) {
      const int blockSize = 4;
      var blockCountX = width / blockSize;
      var blockCountY = height / blockSize;

      var imageSize = width * height / 2;

      var monoTable = new byte[8];
      var rIndices = new byte[16];

      // TODO: Support grayscale?
      var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      BitmapUtil.InvokeAsLocked(bitmap, bmpData => {
        var scan0 = (byte*) bmpData.Scan0.ToPointer();

        for (var i = 0; i < imageSize; i += 8) {
          var iOff = srcOffset + i;

          // Gathers up color palette.
          monoTable[0] = src[iOff + 0];
          monoTable[1] = src[iOff + 1];

          var first = monoTable[0];
          var second = monoTable[1];

          var useEightIndexMode = first < second;
          var min = useEightIndexMode ? first : second;
          var max = !useEightIndexMode ? first : second;

          if (useEightIndexMode) {
            monoTable[2] = (byte) ((6 * min + 1 * max) / 7f);
            monoTable[3] = (byte) ((5 * min + 2 * max) / 7f);
            monoTable[4] = (byte) ((4 * min + 3 * max) / 7f);
            monoTable[5] = (byte) ((3 * min + 4 * max) / 7f);
            monoTable[6] = (byte) ((2 * min + 5 * max) / 7f);
            monoTable[7] = (byte) ((1 * min + 6 * max) / 7f);
          } else {
            monoTable[2] = (byte) ((4 * min + 1 * max) / 5f);
            monoTable[3] = (byte) ((3 * min + 2 * max) / 5f);
            monoTable[4] = (byte) ((2 * min + 3 * max) / 5f);
            monoTable[5] = (byte) ((1 * min + 4 * max) / 5f);
            monoTable[6] = 0;
            monoTable[7] = 255;
          }

          // Gathers up color indices.
          for (var ii = 0; ii < 16; ++ii) {
            // Picks middle color for low-resolution image.
            rIndices[ii] = (byte) (useEightIndexMode ? 4 : 3);
          }

          var temp = ((src[iOff + 4] << 16) | (src[iOff + 3] << 8)) |
                     src[iOff + 2];
          for (var ii = 0; ii < 8; ++ii) {
            rIndices[ii] = (byte) (temp & 7);
            temp >>= 3;
          }

          temp = ((src[iOff + 7] << 16) | (src[iOff + 6] << 8)) | src[iOff + 5];
          for (var ii = 8; ii < 16; ++ii) {
            rIndices[ii] = (byte) (temp & 7);
            temp >>= 3;
          }

          // Writes pixels to output image.
          var tileIndex = i / 8;
          var tileY = tileIndex % blockCountY;
          var tileX = (tileIndex - tileY) / blockCountX;

          for (var j = 0; j < blockSize; j++) {
            for (var k = 0; k < blockSize; k++) {
              var value = monoTable[rIndices[(j * blockSize) + k]];

              var outIndex =
                  (((((tileY * blockSize) + j) * width) + (tileX * blockSize)) +
                   k) * 3;

              scan0[outIndex] =
                  scan0[outIndex + 1] = scan0[outIndex + 2] = value;
            }
          }
        }
      });

      return bitmap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rgb565(int c, out int r, out int g, out int b) {
      r = (c & 0xf800) >> 8;
      g = (c & 0x07e0) >> 3;
      b = (c & 0x001f) << 3;
      r |= r >> 5;
      g |= g >> 6;
      b |= b >> 5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Color(int r, int g, int b, int a) {
      return r << 16 | g << 8 | b | a << 24;
    }
  }
}