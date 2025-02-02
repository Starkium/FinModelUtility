﻿using fin.log;

using uni.platforms;
using uni.platforms.gcn;

namespace uni.games.paper_mario_the_thousand_year_door {
  public class PaperMarioTheThousandYearDoorExtractor : IExtractor {
    private readonly ILogger logger_ =
        Logging.Create<PaperMarioTheThousandYearDoorExtractor>();

    public void ExtractAll() {
      var paperMarioTheThousandYearDoorRom =
          DirectoryConstants.ROMS_DIRECTORY.GetExistingFile(
              "paper_mario_the_thousand_year_door.gcm");

      var options = GcnFileHierarchyExtractor.Options.Standard();
      var fileHierarchy =
          new GcnFileHierarchyExtractor()
              .ExtractFromRom(options, paperMarioTheThousandYearDoorRom);
    }
  }
}