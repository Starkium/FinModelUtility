﻿using f3dzex2.io;
using System.IO;

namespace uni.games.ocarina_of_time {
  public interface IZFile {
    string Name { get; }
  }

  public class OcarinaOfTimeBanks {
    private readonly IBankManager impl_;

    public OcarinaOfTimeBanks(IEndianBinaryReader er) {
      
    }


  }
}
