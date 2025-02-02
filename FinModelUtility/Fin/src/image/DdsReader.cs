﻿using System;
using System.IO;
using System.Linq;

using fin.color;

using SixLabors.ImageSharp.PixelFormats;

using ImageFormat = Pfim.ImageFormat;


namespace fin.image {
  public class DdsReader {
    public unsafe IMipMap<IImage> Read(Stream stream) {
      using var pfimImage = Pfim.Pfim.FromStream(stream);

      return MipMapUtil.From(
          pfimImage.MipMaps.Select(pfimMipMap => {
                     var mmWidth = pfimMipMap.Width;
                     var mmHeight = pfimMipMap.Height;

                     fixed (byte* imagePtr = pfimImage.Data) {
                       var byteSrcPtr = imagePtr + pfimMipMap.DataOffset;

                       switch (pfimImage.Format) {
                         case ImageFormat.Rgba32: {
                           var intSrcPtr = (int*) byteSrcPtr;
                           
                           var image = new Rgba32Image(mmWidth, mmHeight);
                           using var imageLock = image.Lock();
                           var dstPtr = imageLock.pixelScan0;

                           for (var y = 0; y < mmHeight; ++y) {
                             for (var x = 0; x < mmWidth; ++x) {
                               var i = y * mmWidth + x;
                               FinColor.SplitBgra(intSrcPtr[i], out var r, out var g, out var b, out var a);
                               dstPtr[i] = new Rgba32(r, g, b, a);
                             }
                           }

                           return image as IImage;
                         }
                         case ImageFormat.Rgb24: {
                           var image = new Rgb24Image(mmWidth, mmHeight);
                           using var imageLock = image.Lock();
                           var ptr = imageLock.pixelScan0;
                           for (var y = 0; y < mmHeight; ++y) {
                             for (var x = 0; x < mmWidth; ++x) {
                               var i = y * mmWidth + x;

                               var inI = pfimMipMap.DataOffset + 3 * i;
                               var b = pfimImage.Data[inI + 0];
                               var g = pfimImage.Data[inI + 1];
                               var r = pfimImage.Data[inI + 2];

                               ptr[i] = new Rgb24(r, g, b);
                             }
                           }

                           return image;
                         }
                         default:
                           throw new NotImplementedException(
                               $"Unsupported Pfim format: {pfimImage.Format}");
                       }
                     }
                   })
                   .ToList());
    }
  }
}