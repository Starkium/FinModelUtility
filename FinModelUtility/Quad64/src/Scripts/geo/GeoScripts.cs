﻿using fin.math;
using fin.math.matrix;
using fin.model;
using fin.model.impl;
using Quad64.src.LevelInfo;
using sm64.scripts;
using sm64.scripts.geo;


namespace Quad64.src.Scripts {
  public class GeoScriptNode {
    public GeoScriptNode(GeoScriptNode? parent) {
      this.parent = parent;
    }

    public int ID = 0;
    public GeoScriptNode? parent = null;
    public IFinMatrix4x4 matrix { get; } = new FinMatrix4x4().SetIdentity();
    public bool callSwitch = false, isSwitch = false;
    public uint switchFunc = 0, switchCount = 0, switchPos = 0;

    public IFinMatrix4x4 GetTotalMatrix() {
      var matrices = new LinkedList<IFinMatrix4x4>();

      var current = this;
      while (current != null) {
        matrices.AddFirst(current.matrix);
        current = current.parent;
      }

      var matrix = new FinMatrix4x4().SetIdentity();
      foreach (var mat in matrices) {
        matrix.MultiplyInPlace(mat);
      }
      return matrix;
    }
  }

  public class GeoScripts : IGeoScripts {
    private GeoScriptNode rootNode { get; set; }
    private GeoScriptNode nodeCurrent { get; set; }

    private static uint bytesToInt(byte[] b, int offset, int length) {
      switch (length) {
        case 1: return b[0 + offset];
        case 2: return (uint)(b[0 + offset] << 8 | b[1 + offset]);
        case 3:
          return (uint)(b[0 + offset] << 16 | b[1 + offset] << 8 |
                        b[2 + offset]);
        default:
          return (uint)(b[0 + offset] << 24 | b[1 + offset] << 16 |
                        b[2 + offset] << 8 | b[3 + offset]);
      }
    }

    public GeoScripts() {
      rootNode = new GeoScriptNode(null);
      nodeCurrent = rootNode;
    }

    public void parse(Model3DLods mdlLods,
                      ref Level lvl,
                      byte seg,
                      uint off,
                      byte? areaID) {
      if (seg == 0) return;

      mdlLods.Current.Node = nodeCurrent;

      ROM rom = ROM.Instance;
      byte[] data = rom.getSegment(seg, areaID);
      bool end = false;
      while (!end) {
        byte cmdLen = getCmdLength(data[off]);
        byte[] cmd = rom.getSubArray_safe(data, off, cmdLen);
        string desc = "Unknown command";
        bool alreadyAdded = false;
        /*
        if (cmd[0] != 0x05 && nodeCurrent.isSwitch && nodeCurrent.switchPos != 1)
        {
            if (nodeCurrent.switchFunc == 0x8029DB48)
            {
                //rom.printArray(cmd, cmdLen);
                //Console.WriteLine(nodeCurrent.switchPos);
            }
            nodeCurrent.switchPos++;
            off += cmdLen;
            continue;
        }*/

        switch (cmd[0]) {
          case 0x00:
            desc = "Branch geometry layout to address 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8");
            addGLSCommandToDump(mdlLods.Current, cmd, seg, off, desc, areaID);
            alreadyAdded = true;
            CMD_00(mdlLods, ref lvl, cmd, areaID);
            break;
          case 0x01:
            desc = "End geometry layout";
            end = true;
            break;
          case 0x02:
            desc = "Branch geometry layout to address 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8");
            addGLSCommandToDump(mdlLods.Current, cmd, seg, off, desc, areaID);
            alreadyAdded = true;
            CMD_02(mdlLods, ref lvl, cmd, areaID);

            if (cmd[1] == 0x01) {
              if (nodeCurrent.parent == null ||
                  (nodeCurrent.parent != null &&
                   nodeCurrent.parent.callSwitch == false)) {
                // If the next command is not another 0x02 command, or a 0x05 command...
                if (data[off + cmdLen] != 0x02 && data[off + cmdLen] != 0x05) {
                  end = true;
                }
              }
            }
            break;
          case 0x03:
            desc = "Return from branch";
            end = true;
            break;
          case 0x04:
            desc = "Open New Node";
            CMD_04(mdlLods);
            break;
          case 0x05:
            desc = "Close Node";
            if (nodeCurrent != rootNode) {
              nodeCurrent = nodeCurrent.parent;
              mdlLods.Current.Node = nodeCurrent;
            }
            break;
          case 0x08:
            desc = "Set screen rendering area (" +
                   "center X = " + (short)bytesToInt(cmd, 4, 2) +
                   ", center Y = " + (short)bytesToInt(cmd, 6, 2) +
                   ", Width = " + (short)(bytesToInt(cmd, 8, 2) * 2) +
                   ", Height = " + (short)(bytesToInt(cmd, 10, 2) * 2) + ")";
            break;
          case 0x0A:
            desc = "Set camera frustum (" +
                   "FOV = " + (short)bytesToInt(cmd, 2, 2) +
                   ", Near = " + (short)bytesToInt(cmd, 4, 2) +
                   ", Far = " + (short)bytesToInt(cmd, 6, 2) + ")";
            break;
          case 0x0B:
            desc = "Start geometry layout";
            break;
          case 0x0C:
            if (cmd[1] == 0x00)
              desc = "Disable Z-Buffer";
            else
              desc = "Enable Z-Buffer";
            break;
          case 0x0D:
            var minRenderRange = (short)bytesToInt(cmd, 4, 2);
            var maxRenderRange = (short)bytesToInt(cmd, 6, 2);
            desc = "Set render range from camera (min = " +
                   minRenderRange + ", max = " +
                   maxRenderRange + ")";
            mdlLods.Add(nodeCurrent!);
            break;
          case 0x0E:
            desc =
                "Switch case with following display lists using ASM function 0x" +
                bytesToInt(cmd, 4, 4).ToString("X8");
            //rom.printArray(cmd, cmdLen);
            CMD_0E(mdlLods.Current, ref lvl, cmd);
            break;
          case 0x10:
            // TODO: Not implemented
            desc = "Translate and rotate";
            CMD_10(mdlLods.Current, ref lvl, cmd);
            break;
          case 0x11:
            // TODO: Not implemented
            //rom.printArray(cmd, cmdLen);
            desc = "Translate Node";
            CMD_11(mdlLods.Current, ref lvl, cmd);
            break;
          case 0x12:
            // TODO: Not implemented
            desc = "Rotate node";
            CMD_12(mdlLods.Current, ref lvl, cmd);
            break;
          case 0x13:
            desc = "Load display list 0x" +
                   bytesToInt(cmd, 8, 4).ToString("X8") +
                   " into layer " + cmd[1] + " and offset position by (" +
                   (short)bytesToInt(cmd, 2, 2) +
                   "," + (short)bytesToInt(cmd, 2, 2) +
                   "," + (short)bytesToInt(cmd, 2, 2) +
                   ")";
            //rom.printArray(cmd, cmdLen);
            CMD_13(mdlLods.Current, ref lvl, cmd, areaID);
            break;
          case 0x14:
            // TODO: Not implemented
            desc = "Billboard Model";
            //CMD_10(ref mdl, ref lvl, cmd);
            break;
          case 0x15:
            desc = "Load display list 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8") +
                   " into layer " + cmd[1];
            CMD_15(mdlLods.Current, ref lvl, cmd, areaID);
            // rom.printArray(cmd, cmdLen);
            break;
          case 0x16:
            desc = "Start geometry layout with a shadow. (type = " + cmd[3] +
                   ", solidity = " + cmd[5] + ", scale = " +
                   bytesToInt(cmd, 6, 2) + ")";
            //CMD_10(ref mdl, ref lvl, cmd);
            break;
          case 0x17:
            desc = "Setup display lists for level objects";
            break;
          case 0x18:
            desc = "Create display list(s) from the ASM function 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8")
                   + " (a0 = " + bytesToInt(cmd, 2, 2) + ")";
            CMD_18(mdlLods.Current, ref lvl, cmd);
            // rom.printArray(cmd, cmdLen);
            break;
          case 0x19:
            if (bytesToInt(cmd, 4, 4) == 0x00000000) {
              desc = "Draw solid color background. Color = (";
              ushort color = (ushort)bytesToInt(cmd, 2, 2);
              desc += (((color >> 11) & 0x1F) * 8) + ","
                                                   + (((color >> 6) & 0x1F) *
                                                         8) + ","
                                                   + (((color >> 1) & 0x1F) *
                                                         8) + ")";
            } else {
              desc = "Draw background image. bgID = " + bytesToInt(cmd, 2, 2) +
                     ", calls ASM function 0x" +
                     bytesToInt(cmd, 4, 4).ToString("X8");
            }
            CMD_19(mdlLods.Current, ref lvl, cmd,
                   rom.decodeSegmentAddress(seg, off, areaID));
            // rom.printArray(cmd, cmdLen);
            break;
          case 0x1D:
            desc = "Scale following node by " +
                   ((bytesToInt(cmd, 4, 4) / 65536.0f) * 100.0f) + "%";
            CMD_1D(mdlLods.Current, cmd);
            break;
          case 0x1C:
            // TODO: Not implemented
            desc = "Held object scene graph node";
            break;
          case 0x1A:
          case 0x1E:
          case 0x1F:
            desc = "Do nothing";
            break;
          case 0x20:
            desc = "Start geometry layout with render area of " +
                   bytesToInt(cmd, 2, 2);
            break;
          default:
            break;
        }
        if (!alreadyAdded)
          addGLSCommandToDump(mdlLods.Current, cmd, seg, off, desc, areaID);
        off += cmdLen;
        /*
        if (nodeCurrent.isSwitch)
            nodeCurrent.switchPos++;
            */
      }
    }

    private void addGLSCommandToDump(Model3D? mdl,
                                     byte[] cmd,
                                     byte seg,
                                     uint offset,
                                     string description,
                                     byte? areaID) {
      ScriptDumpCommandInfo info = new ScriptDumpCommandInfo();
      info.data = cmd;
      info.description = description;
      info.segAddress = (uint)(seg << 24) | offset;
      info.romAddress =
          ROM.Instance.decodeSegmentAddress_safe(seg, offset, areaID);
      mdl?.GeoLayoutCommands_ForDump.Add(info);
    }

    private void CMD_00(Model3DLods mdlLods,
                        ref Level lvl,
                        byte[] cmd,
                        byte? areaID) {
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);
      parse(mdlLods, ref lvl, seg, off, areaID);
    }

    private void CMD_02(Model3DLods mdlLods,
                        ref Level lvl,
                        byte[] cmd,
                        byte? areaID) {
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);
      parse(mdlLods, ref lvl, seg, off, areaID);
    }


    private void CMD_04(Model3DLods mdlLods) {
      GeoScriptNode newNode = new GeoScriptNode(nodeCurrent);
      newNode.ID = nodeCurrent.ID + 1;
      newNode.parent = nodeCurrent;
      /*
      if (nodeCurrent.callSwitch)
      {
          newNode.switchPos = 0;
          newNode.switchCount = nodeCurrent.switchCount;
          newNode.switchFunc = nodeCurrent.switchFunc;
          //newNode.isSwitch = true;
      }
      */
      nodeCurrent = newNode;
      mdlLods.Current.Node = nodeCurrent;
    }

    private void CMD_0E(Model3D mdl, ref Level lvl, byte[] cmd) {
      //nodeCurrent.switchFunc = bytesToInt(cmd, 4, 4);
      // Special Ignore cases
      //if (nodeCurrent.switchFunc == 0x8029DBD4) return;
      //nodeCurrent.switchCount = cmd[3];
      // TODO: Seems to be broken
      nodeCurrent.callSwitch = true;
    }

    private void CMD_10(Model3D mdl, ref Level lvl, byte[] cmd) {
      var param = cmd[1];
      var format = GeoUtils.GetTranslateAndRotateFormat(param);

      var posX = 0f;
      var posY = 0f;
      var posZ = 0f;
      var rotX = 0f;
      var rotY = 0f;
      var rotZ = 0f;

      switch (format) {
        case GeoTranslateAndRotateFormat.TRANSLATION_AND_ROTATION: {
          posX = (short)bytesToInt(cmd, 4, 2);
          posY = (short)bytesToInt(cmd, 6, 2);
          posZ = (short)bytesToInt(cmd, 8, 2);
          rotX = (short)bytesToInt(cmd, 10, 2);
          rotY = (short)bytesToInt(cmd, 12, 2);
          rotZ = (short)bytesToInt(cmd, 14, 2);
          break;
        }
        case GeoTranslateAndRotateFormat.TRANSLATION: {
          posX = (short)bytesToInt(cmd, 2, 2);
          posY = (short)bytesToInt(cmd, 4, 2);
          posZ = (short)bytesToInt(cmd, 6, 2);
          break;
        }
        case GeoTranslateAndRotateFormat.ROTATION: {
          rotX = (short)bytesToInt(cmd, 2, 2);
          rotY = (short)bytesToInt(cmd, 4, 2);
          rotZ = (short)bytesToInt(cmd, 6, 2);
          break;
        }
        case GeoTranslateAndRotateFormat.YAW: {
          rotY = (short)bytesToInt(cmd, 2, 2);
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
      this.nodeCurrent.matrix.MultiplyInPlace(
          MatrixTransformUtil.FromTrs(
              new Position(
                  posX,
                  posY,
                  posZ
              ),
              new ModelImpl.RotationImpl().SetDegrees(
                  rotX, rotY, rotZ),
              null));
    }

    private void CMD_11(Model3D mdl, ref Level lvl, byte[] cmd) {
      // TODO: Definitely broken
      short x = (short)bytesToInt(cmd, 2, 2);
      short y = (short)bytesToInt(cmd, 4, 2);
      short z = (short)bytesToInt(cmd, 6, 2);
      nodeCurrent.matrix.MultiplyInPlace(
          MatrixTransformUtil.FromTranslation(x, y, z));
    }

    private void CMD_12(Model3D mdl, ref Level lvl, byte[] cmd) {
      // TODO: Definitely broken
      short x = (short)bytesToInt(cmd, 2, 2);
      short y = (short)bytesToInt(cmd, 4, 2);
      short z = (short)bytesToInt(cmd, 6, 2);
      nodeCurrent.matrix.MultiplyInPlace(
          MatrixTransformUtil.FromRotation(
              new ModelImpl.RotationImpl().SetDegrees(x, y, z)));
    }

    private void CMD_13(Model3D mdl,
                        ref Level lvl,
                        byte[] cmd,
                        byte? areaID) {
      byte drawLayer = cmd[1];
      short x = (short)bytesToInt(cmd, 2, 2);
      short y = (short)bytesToInt(cmd, 4, 2);
      short z = (short)bytesToInt(cmd, 6, 2);
      uint displayListAddress = bytesToInt(cmd, 8, 4);
      byte seg = (byte)(displayListAddress >> 24);
      if (seg > 0x20)
        return;
      uint off = displayListAddress & 0xFFFFFF;
      nodeCurrent.matrix.MultiplyInPlace(
          MatrixTransformUtil.FromTranslation(x, y, z));
      // Don't bother processing duplicate display lists.
      if (displayListAddress != 0) {
        if (!mdl.hasGeoDisplayList(off)) {
          Fast3DScripts.parse(ref mdl, ref lvl, seg, off, areaID, 0);
        }
        lvl.temp_bgInfo.usesFog = mdl.builder.UsesFog;
        lvl.temp_bgInfo.fogColor = mdl.builder.FogColor;
        lvl.temp_bgInfo.fogColor_romLocation = mdl.builder.FogColor_romLocation;
      } else { }
    }

    private void CMD_15(Model3D mdl,
                        ref Level lvl,
                        byte[] cmd,
                        byte? areaID) {
      // if (bytesToInt(cmd, 4, 4) != 0x07006D70) return;
      byte drawLayer = cmd[1];
      byte seg = cmd[4];
      if (seg > 0x20)
        return;
      uint off = bytesToInt(cmd, 5, 3);
      // Don't bother processing duplicate display lists.
      if (!mdl.hasGeoDisplayList(off)) {
        Fast3DScripts.parse(ref mdl, ref lvl, seg, off, areaID, 0);
      }

      lvl.temp_bgInfo.usesFog = mdl.builder.UsesFog;
      lvl.temp_bgInfo.fogColor = mdl.builder.FogColor;
      lvl.temp_bgInfo.fogColor_romLocation = mdl.builder.FogColor_romLocation;
    }

    private void CMD_18(Model3D mdl, ref Level lvl, byte[] cmd) {
      ROM rom = ROM.Instance;
      uint asmAddress = bytesToInt(cmd, 4, 4);
      switch (rom.Region) {
        case ROM_Region.NORTH_AMERICA:
          if (asmAddress == Globals.endCake_drawFunc_NA) {
            lvl.temp_bgInfo.isEndCakeImage = true;
          }
          break;
      }
    }

    private void CMD_19(Model3D mdl,
                        ref Level lvl,
                        byte[] cmd,
                        uint romOffset) {
      lvl.temp_bgInfo.id_or_color = (ushort)bytesToInt(cmd, 2, 2);
      lvl.temp_bgInfo.address = bytesToInt(cmd, 4, 4);
      lvl.temp_bgInfo.isEndCakeImage = false;
      lvl.temp_bgInfo.romLocation = romOffset;
    }

    private void CMD_1D(Model3D mdl, byte[] cmd) {
      var scale = (bytesToInt(cmd, 4, 4) / 65536.0f);
      this.nodeCurrent.matrix.MultiplyInPlace(
          MatrixTransformUtil.FromScale(new Scale (scale)));
    }

    private byte getCmdLength(byte cmd) {
      switch (cmd) {
        case 0x00:
        case 0x02:
        case 0x0D:
        case 0x0E:
        case 0x11:
        case 0x12:
        case 0x14:
        case 0x15:
        case 0x16:
        case 0x18:
        case 0x19:
        case 0x1A:
        case 0x1D:
        case 0x1E:
          return 0x08;
        case 0x08:
        case 0x0A:
        case 0x13:
        case 0x1C:
          return 0x0C;
        case 0x1F:
          return 0x10;
        case 0x0F:
        case 0x10:
          return 0x14;
        default:
          return 0x04;
      }
    }
  }
}