﻿using fin.model;
using uni.games.battalion_wars_1;
using uni.games.battalion_wars_2;
using uni.games.glover;
using uni.games.great_ace_attorney;
using uni.games.halo_wars;
using uni.games.luigis_mansion_3d;
using uni.games.majoras_mask_3d;
using uni.games.mario_kart_double_dash;
using uni.games.ocarina_of_time_3d;
using uni.games.pikmin_1;
using uni.games.pikmin_2;
using uni.games.professor_layton_vs_phoenix_wright;
using uni.games.super_mario_sunshine;
using uni.games.super_smash_bros_melee;


namespace uni.games {
  public class RootModelFileGatherer {
    public RootModelDirectory GatherAllModelFiles() {
      var rootModelDirectory = new RootModelDirectory();

      var gatherers = new IModelFileGatherer[] {
          new BattalionWars1FileGatherer(), new BattalionWars2FileGatherer(),
          new GloverModelFileGatherer(),
          new GreatAceAttorneyModelFileGatherer(),
          new HaloWarsModelFileGatherer(),
          new LuigisMansion3dModelFileGatherer(),
          new MajorasMask3dFileGatherer(),
          new MarioKartDoubleDashFileGatherer(),
          new OcarinaOfTime3dFileGatherer(),
          new Pikmin1ModelFileGatherer(),
          new Pikmin2FileGatherer(),
          new ProfessorLaytonVsPhoenixWrightModelFileGatherer(),
          new SuperMarioSunshineModelFileGatherer(),
          new SuperSmashBrosMeleeModelFileGatherer(),
      };

      var useMultiThreading = true;
      if (useMultiThreading) {
        var gatherTasks =
            gatherers.Select(
                         gatherer =>
                             Task.Run(() => {
                               try {
                                 return gatherer
                                     .GatherModelFileBundles(false);
                               } catch (Exception e) {
                                 ;
                                 return null;
                               }
                             }))
                     .ToArray();

        Task.WhenAll(gatherTasks)
            .ContinueWith(async resultsTask => {
              var results = await resultsTask;
              foreach (var result in results) {
                rootModelDirectory.AddSubdirIfNotNull(result);
              }
            })
            .Wait();
      } else {
        var gatherTasks =
            gatherers.Select(
                         gatherer =>
                             new Task<IModelDirectory?>(() => gatherer
                                 .GatherModelFileBundles(false)))
                     .ToArray();

        foreach (var gatherTask in gatherTasks) {
          gatherTask.Start();
          gatherTask.Wait();
          rootModelDirectory.AddSubdirIfNotNull(gatherTask.Result);
        }
      }
      rootModelDirectory.RemoveEmptyChildren();

      return rootModelDirectory;
    }
  }
}