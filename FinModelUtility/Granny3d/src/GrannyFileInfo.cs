﻿using fin.util.asserts;

using schema.binary;


namespace granny3d {
  /// <summary>
  ///   Based on HaloWarsDocs file template:
  ///   https://github.com/HaloMods/HaloWarsDocs/blob/master/010Editor/Granny.bt
  /// </summary>
  public class GrannyFileInfo : IGrannyFileInfo, IBinaryDeserializable {
    public string FromFileName { get; private set; }

    public IList<IGrannySkeleton> SkeletonHeaderList { get; } =
      new List<IGrannySkeleton>();

    public IList<IGrannyMesh> VertexDataList { get; } =
      new List<IGrannyMesh>();

    public IList<IGrannyModel> ModelHeaderList { get; private set; }

    public IList<IGrannyTrackGroup> TrackGroupHeaderList { get; } =
      new List<IGrannyTrackGroup>();

    public IList<IGrannyAnimation> AnimationHeaderList { get; } =
      new List<IGrannyAnimation>();

    public void Read(IEndianBinaryReader er) {
      // TODO: Make this offset-agnostic.
      // The reader passed into this method should have an offset of 0 at the
      // start of the granny_file_info object.
      Asserts.Equal(0, er.Position, "Expected to start reading at offset 0.");

      er.ReadUInt64(); // ArtToolInfo
      er.ReadUInt64(); // ExporterInfo

      GrannyUtils.SubreadRef(
          er, ser => { this.FromFileName = ser.ReadStringNT(); });

      GrannyUtils.SubreadRefToArray(er, (ser, textureCount) => {
        for (var i = 0; i < textureCount; ++i) { }
      });

      GrannyUtils.SubreadRefToArray(er, (ser, materialCount) => {
        for (var i = 0; i < materialCount; ++i) { }
      });

      GrannyUtils.SubreadRefToArray(
          er,
          (ser, skeletonCount) => {
            for (var i = 0; i < skeletonCount; ++i) {
              GrannyUtils.SubreadRef(ser, sser => {
                var skeleton = new GrannySkeleton();
                skeleton.Read(sser);
                this.SkeletonHeaderList.Add(skeleton);
              });
            }
          });

      GrannyUtils.SubreadRefToArray(er, (ser, vertexDataCount) => {
        for (var i = 0; i < vertexDataCount; ++i) { }
      });

      GrannyUtils.SubreadRefToArray(er, (ser, modelHeaderCount) => {
        for (var i = 0; i < modelHeaderCount; ++i) { }
      });

      GrannyUtils.SubreadRefToArray(
          er,
          (ser, trackGroupHeaderCount) => {
            /*for (var i = 0; i < trackGroupHeaderCount; ++i) {
              GrannyUtils.SubreadRef(ser, sser => {
                this.TrackGroupHeaderList.Add(sser.ReadNew<GrannyTrackGroup>());
              });
            }*/
          });

      GrannyUtils.SubreadRefToArray(
          er,
          (ser, animationHeaderCount) => {
            for (var i = 0; i < animationHeaderCount; ++i) {
              GrannyUtils.SubreadRef(ser, sser => {
                var animation = new GrannyAnimation();
                animation.Read(sser);
                this.AnimationHeaderList.Add(animation);
              });
            }
          });
    }
  }
}