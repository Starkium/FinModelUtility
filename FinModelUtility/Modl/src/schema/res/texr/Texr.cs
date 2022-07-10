﻿using System.Drawing;

using fin.model;
using fin.model.impl;
using fin.util.asserts;
using fin.util.color;
using fin.util.image;

using schema;


namespace modl.schema.res.texr {
  public class Texr : IBiSerializable {
    public string FileName { get; private set; }

    public void Read(EndianBinaryReader er) {
      er.AssertStringEndian("TEXR");

      var texrLength = er.ReadUInt32();
      var expectedTexrEnd = er.Position + texrLength;

      this.FileName = er.ReadString(er.ReadInt32());

      // TODO: Only true for BW1, BW2 uses GBTF
      er.AssertStringEndian("XBTF");
      var xbtfLength = er.ReadUInt32();
      var expectedXbtfEnd = er.Position + xbtfLength;

      Asserts.Equal(expectedTexrEnd, expectedXbtfEnd);

      var textureCount = er.ReadUInt32();
      for (var i = 0; i < textureCount; ++i) {
        er.AssertStringEndian("TEXT");
        var textureLength = er.ReadUInt32();
        var expectedTextureEnd = er.Position + textureLength;

        var textureName = er.ReadString(0x10);

        var width = er.ReadUInt32();
        var height = er.ReadUInt32();

        var unknowns0 = er.ReadUInt32s(2);

        var textureType = er.ReadString(8);
        var drawType = er.ReadString(8);

        var unknowns1 = er.ReadUInt32s(8);

        var unknowns2 = er.ReadUInt32s(1);

        var image = textureType switch {
            "DXT1" => this.ReadDxt1_(er, width, height),
            _      => null,
        };

        image?.Save(
            $@"R:\Documents\CSharpWorkspace\Pikmin2Utility\cli\roms\battalion_wars\Data\CompoundFiles\debug\{textureType}_{textureName}.png");

        ;

        er.Position = expectedTextureEnd;
        Asserts.Equal(expectedTextureEnd, er.Position);
      }

      Asserts.Equal(expectedTexrEnd, er.Position);

      ;
    }

    public void Write(EndianBinaryWriter ew) =>
        throw new NotImplementedException();

    private Image ReadDxt1_(EndianBinaryReader er, uint width, uint height) {
      er.AssertStringEndian("MIP ");
      var mipSize = er.ReadUInt32();

      var endianness = er.Endianness;
      er.Endianness = Endianness.BigEndian;

      var tileWidth = 4;
      var tileHeight = 4;

      var tileCountX = width / tileWidth;
      var tileCountY = height / tileHeight;

      IColor[] colors = new IColor[4];

      var image = new Bitmap((int) width, (int) height);
      BitmapUtil.InvokeAsLocked(image, bmpData => {
        var x = 0;
        var y = 0;

        unsafe {
          var ptr = (byte*) bmpData.Scan0;
          for (var ii = 0; ii < mipSize / 8; ++ii) {
            var color0Value = er.ReadUInt16();
            var color1Value = er.ReadUInt16();
            var pixelMask = er.ReadUInt32();

            var color0 = colors[0] = ColorUtil.ParseRgb565(color0Value);
            var color1 = colors[1] = ColorUtil.ParseRgb565(color1Value);

            if (color0Value > color1Value) {
              colors[2] = ColorImpl.FromRgbaBytes(
                  (byte) ((2 * color0.Rb + color1.Rb) / 3f),
                  (byte) ((2 * color0.Gb + color1.Gb) / 3f),
                  (byte) ((2 * color0.Bb + color1.Bb) / 3f),
                  255);
              colors[3] = ColorImpl.FromRgbaBytes(
                  (byte) ((2 * color1.Rb + color0.Rb) / 3f),
                  (byte) ((2 * color1.Gb + color0.Gb) / 3f),
                  (byte) ((2 * color1.Bb + color0.Bb) / 3f),
                  255);
            } else {
              colors[2] = ColorImpl.FromRgbaBytes(
                  (byte) ((color0.Rb + color1.Rb) / 2f),
                  (byte) ((color0.Gb + color1.Gb) / 2f),
                  (byte) ((color0.Bb + color1.Bb) / 2f),
                  255);
              colors[3] = ColorImpl.FromRgbaBytes(
                  0, 0, 0, 0);
            }

            var ii2 = ii % 4;
            var iiX = (ii2 % 2) * 4;
            var iiY = (ii2 / 2) * 4;

            for (var yInTile = 0; yInTile < tileHeight; ++yInTile) {
              var arrayY = y + iiY + yInTile;
              for (var xInTile = 0; xInTile < tileWidth; ++xInTile) {
                var arrayX = x + iiX + xInTile;

                var colorIndex =
                    (pixelMask >> ((15 - (yInTile * 4 + xInTile)) * 2)) &
                    0b11;
                var color = colors[colorIndex];

                var imageIndex = 4 * (arrayY * width + arrayX);
                ptr[imageIndex + 0] = color.Bb;
                ptr[imageIndex + 1] = color.Gb;
                ptr[imageIndex + 2] = color.Rb;
                ptr[imageIndex + 3] = color.Ab;
              }
            }

            if (ii2 == 3) {
              x += 8;
              if (x >= width) {
                x = 0;
                y += 8;
              }
            }
          }
        }
      });

      er.Endianness = endianness;

      return image;
    }
  }
}