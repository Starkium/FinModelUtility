﻿using System.Security.Cryptography;
using System.Text;
using System.Xml;

using fin.data;
using fin.util.asserts;

using Force.Crc32;

using NullFX.CRC;


namespace uni.debug {
  public class DebugProgram {
    public void Run() {
      using var xmlStream =
          File.OpenRead(
              @"R:\Documents\CSharpWorkspace\Pikmin2Utility\cli\roms\halo_wars\data\soundtable.xml");

      var xmlReader = XmlReader.Create(xmlStream);

      xmlReader.MoveToContent();

      xmlReader.ReadStartElement("Table");

      var sounds = new List<(string cueName, long cueIndex)>();
      while (true) {
        try {
          xmlReader.ReadStartElement("Sound");
        } catch {
          break;
        }

        xmlReader.ReadStartElement("CueName");
        var cueName = xmlReader.ReadContentAsString();
        xmlReader.ReadEndElement();

        xmlReader.ReadStartElement("CueIndex");

        var longCueIndex = xmlReader.ReadContentAsLong();
        var cueIndex = (uint)longCueIndex;
        Asserts.Equal(longCueIndex, cueIndex);

        xmlReader.ReadEndElement();

        sounds.Add((cueName, cueIndex));

        xmlReader.ReadEndElement();
      }

      sounds.Sort((lhs, rhs) => {
        var (lhsCueName, lhsCueIndex) = lhs;
        var (rhsCueName, rhsCueIndex) = rhs;

        return lhsCueIndex.CompareTo(rhsCueIndex);
      });

      const int bits32 = 32;
      const uint prime32 = 16777619;
      const uint offsetBasis32 = 2166136261U;

      var calcHash32A = (byte[] bytes) => {
        var hval = offsetBasis32;
        foreach (var b in bytes) {
          hval *= prime32;
          hval ^= b;
        }

        return hval;
      };

      var calcHash32B = (byte[] bytes) => {
        ulong hval = offsetBasis32;
        foreach (var b in bytes) {
          hval *= prime32;
          hval ^= b;
        }

        var mask = (1UL << bits32) - 1;
        return (uint)((hval >> bits32) ^ (hval & mask));
      };

      var calcHashLowercase32A = (string str) => {
        var hval = offsetBasis32;
        foreach (var c in str) {
          hval *= prime32;
          var cc = c is >= 'A' and <= 'Z' ? c - 'A' + 'a' : c;
          hval ^= (uint)cc;
        }
        return hval;
      };

      var calcHashLowercase32B = (string str) => {
        ulong hval = offsetBasis32;
        foreach (var c in str) {
          hval *= prime32;
          var cc = c is >= 'A' and <= 'Z' ? c - 'A' + 'a' : c;
          hval ^= (uint)cc;
        }

        var mask = (1UL << bits32) - 1;
        return (uint)((hval >> bits32) ^ (hval & mask));
      };


      var offsetBasis30 = offsetBasis32;
      var prime30 = prime32;
      var bits30 = 30;

      var calcHash30A = (byte[] bytes) => {
        var hval = offsetBasis30;
        foreach (var b in bytes) {
          hval *= prime30;
          hval ^= b;
        }

        return hval;
      };

      var calcHash30B = (byte[] bytes) => {
        ulong hval = offsetBasis30;
        foreach (var b in bytes) {
          hval *= prime30;
          hval ^= b;
        }

        var mask = (1UL << bits30) - 1;
        return (uint)((hval >> bits30) ^ (hval & mask));
      };

      var calcHashLowercase30A = (string str) => {
        var hval = offsetBasis30;
        foreach (var c in str) {
          hval *= prime30;
          var cc = c is >= 'A' and <= 'Z' ? c - 'A' + 'a' : c;
          hval ^= (uint)cc;
        }
        return hval;
      };

      var calcHashLowercase30B = (string str) => {
        ulong hval = offsetBasis30;
        foreach (var c in str) {
          hval *= prime30;
          var cc = c is >= 'A' and <= 'Z' ? c - 'A' + 'a' : c;
          hval ^= (uint)cc;
        }

        var mask = (1UL << bits30) - 1;
        return (uint)((hval >> bits30) ^ (hval & mask));
      };



      var soundNamesByCrc32 =
          sounds
              .Select(
                  cueNameAndIndex => cueNameAndIndex.cueName)
              .SelectMany(cueName => {
                var allBytes = new List<byte[]>();

                var allStrings = new[] {
                    cueName,
                    cueName + '\0'
                };

                var strChecksums = allStrings.SelectMany(str => {
                  var allChecksums = new List<uint>();

                  allChecksums.Add(calcHashLowercase32A(str));
                  allChecksums.Add(calcHashLowercase32B(str));

                  allChecksums.Add(calcHashLowercase30A(str));
                  allChecksums.Add(calcHashLowercase30B(str));

                  return allChecksums;
                });

                allBytes.AddRange(
                    allStrings.Select(str => Encoding.ASCII.GetBytes(str)));
                allBytes.AddRange(
                    allStrings.Select(str => Encoding.UTF8.GetBytes(str)));
                allBytes.AddRange(
                    allStrings.Select(str => Encoding.Unicode.GetBytes(str)));
                allBytes.AddRange(
                    allStrings.Select(
                        str => Encoding.BigEndianUnicode.GetBytes(str)));

                var n = allBytes.Count;

                for (var i = 0; i < 4; ++i) {
                  var totalNameBytes = new byte[0x40];
                  var currentBytes = allBytes[i];

                  for (var b = 0; b < currentBytes.Length; ++b) {
                    totalNameBytes[b] = currentBytes[b];
                  }

                  allBytes.Add(totalNameBytes);
                }

                for (var i = 0; i < n; ++i) {
                  var totalNameBytes = new byte[0x80];
                  var currentBytes = allBytes[i];

                  for (var b = 0; b < currentBytes.Length; ++b) {
                    totalNameBytes[b] = currentBytes[b];
                  }

                  allBytes.Add(totalNameBytes);
                }

                n = allBytes.Count;
                for (var i = 0; i < n; ++i) {
                  allBytes.Add(allBytes[i].Reverse().ToArray());
                }

                var byteChecksums = allBytes.SelectMany(bytes => {
                  var allChecksums = new List<uint>();

                  allChecksums.Add(calcHash32A(bytes));
                  allChecksums.Add(calcHash32B(bytes));

                  allChecksums.Add(calcHash30A(bytes));
                  allChecksums.Add(calcHash30B(bytes));

                  allChecksums.Add(Crc32.ComputeChecksum(bytes));
                  allChecksums.Add(CalculateCrc(bytes));

                  allChecksums.Add(Crc32Algorithm.Compute(bytes));
                  allChecksums.Add(Crc32CAlgorithm.Compute(bytes));

                  var md5 = MD5.HashData(bytes);
                  using var er =
                      new EndianBinaryReader(new MemoryStream(md5),
                                             Endianness.LittleEndian);
                  foreach (var val in er.ReadUInt32s(4)) {
                    allChecksums.Add(val);
                  }

                  allChecksums.Add(
                      Crc16.ComputeChecksum(Crc16Algorithm.Standard, bytes));
                  allChecksums.Add(
                      Crc16.ComputeChecksum(Crc16Algorithm.Ccitt, bytes));
                  allChecksums.Add(
                      Crc16.ComputeChecksum(
                          Crc16Algorithm.CcittInitialValue0x1D0F, bytes));
                  allChecksums.Add(
                      Crc16.ComputeChecksum(
                          Crc16Algorithm.CcittInitialValue0xFFFF, bytes));
                  allChecksums.Add(
                      Crc16.ComputeChecksum(Crc16Algorithm.CcittKermit, bytes));
                  allChecksums.Add(
                      Crc16.ComputeChecksum(Crc16Algorithm.Dnp, bytes));
                  allChecksums.Add(
                      Crc16.ComputeChecksum(Crc16Algorithm.Modbus, bytes));

                  return allChecksums;
                });

                var allChecksums = strChecksums.Concat(byteChecksums).ToList();
                var allChecksumCount = allChecksums.Count;
                for (var i = 0; i < allChecksumCount; ++i) {
                  var checksum = allChecksums[i];
                  allChecksums.Add(checksum - 1);
                  allChecksums.Add(checksum + 1);
                }

                return allChecksums.Select(checksum => (cueName, checksum));
              })
              .Select(cueNameAndChecksum => cueNameAndChecksum.checksum)
              .ToHashSet();

      var cueIndices = sounds.Select(sound => sound.cueIndex).ToHashSet();
      var cueIndexMatchCount = 0;

      var filesByLang = new ListDictionary<uint, SoundFile>();
      {
        using var fs =
            File.OpenRead(
                @"R:\Leaks\Wwise-Unpacker-1.0.3\Game Files\Sounds.pck");
        var er = new EndianBinaryReader(fs, Endianness.LittleEndian);

        er.AssertString("AKPK");

        var headerSize = er.ReadUInt32();
        var headerStart = er.Position;
        er.AssertUInt32(1);

        var languageListSize = er.ReadUInt32();
        var bankTableSize = er.ReadUInt32();
        var soundTableSize = er.ReadUInt32();
        var externalsSize = er.ReadUInt32();

        // Read langs
        var langDict = new Dictionary<uint, Lang>();
        {
          var stringsOffset = er.Position;

          var langCount = er.ReadUInt32();
          for (var i = 0; i < langCount; ++i) {
            var langOffset = er.ReadUInt32();
            var langId = er.ReadUInt32();

            var langName = "";
            er.Subread(stringsOffset + langOffset,
                       ser => {
                         langName = ser.ReadStringNT(Encoding.Unicode);
                       });

            var lang = new Lang {
              Id = langId,
              Name = langName
            };

            langDict[langId] = lang;
          }

          er.Position = stringsOffset + languageListSize;
        }

        var maxSoundCrc = 0UL;
        var maxBankCrc = 0UL;

        var readTable = (uint sectionSize, bool isBank) => {
          var sectionOffset = er.Position;
          var fileCount = er.ReadUInt32();

          for (var f = 0; f < fileCount; ++f) {
            var nameCrc32 = er.ReadUInt32();

            if (isBank) {
              maxBankCrc = Math.Max(nameCrc32, maxBankCrc);
            } else {
              maxSoundCrc = Math.Max(nameCrc32, maxSoundCrc);
            }

            var blockSize = er.ReadUInt32();
            var size = er.ReadUInt32();
            var offset = er.ReadUInt32();
            var langId = er.ReadUInt32();

            if (soundNamesByCrc32.Contains(nameCrc32)) {
              ++cueIndexMatchCount;
            }

            er.Subread(offset, ser => {
              var unkId = ser.ReadUInt32();

              if (soundNamesByCrc32.Contains(unkId)) {
                ++cueIndexMatchCount;
              }

              var unk1 = ser.ReadUInt32();
              var unk2 = ser.ReadUInt32();

              uint unk3;
              if (isBank) {
                ser.AssertUInt32(nameCrc32);
              } else {
                unk3 = ser.ReadUInt32();
              }
              var unk4 = ser.ReadUInt32();

              var codecUint = ser.ReadUInt16();

              ;
            });

            var soundFile = new SoundFile {
              Id = nameCrc32,
              BlockSize = blockSize,
              Size = size,
              Offset = offset,
              LangId = langId,
            };

            filesByLang.Add(langId, soundFile);
          }

          er.Position = sectionOffset + sectionSize;
        };

        // Read banks
        readTable(bankTableSize, true);

        ;

        readTable(soundTableSize, false);

        var bankF = 1f * maxBankCrc / uint.MaxValue;
        var soundF = 1f * maxSoundCrc / Math.Pow(2, 30);

        ;
      }

      ;
    }

    public uint CalculateCrc(byte[] bytes) {
      var crc32Table = new uint[256]
    {
        0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3,
        0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91,
        0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7,
        0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5,
        0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B,
        0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
        0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F,
        0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D,
        0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
        0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
        0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457,
        0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
        0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB,
        0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9,
        0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
        0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD,
        0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683,
        0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
        0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7,
        0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
        0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B,
        0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79,
        0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236, 0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F,
        0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
        0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713,
        0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21,
        0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
        0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45,
        0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB,
        0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
        0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF,
        0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
    };

      uint crc = 0xffffffff;
      for (int i = 0; i < bytes.Length; i++) {
        crc = (crc >> 8) ^ crc32Table[(crc & 0xff) ^ bytes[i]];
      }

      return ~crc;
    }
  }

  public class Lang {
    public uint Id { get; set; }
    public string Name { get; set; }
  }

  public class SoundFile {
    public uint Id { get; set; }
    public uint BlockSize { get; set; }
    public uint Size { get; set; }
    public uint Offset { get; set; }
    public uint LangId { get; set; }
  }

  public enum Codec {
    UNDEFINED,
    XMA,
    OGG,
    WAV
  }
}