﻿using fin.math;
using fin.math.matrix;
using System;
using System.Numerics;
using OpenTK.Graphics.OpenGL;

using GlMatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;

namespace fin.gl {
  public static class GlTransform {
    private static readonly Matrix4x4Stack modelViewMatrix_ = new();
    private static readonly Matrix4x4Stack projectionMatrix_ = new();
    private static IMatrix4x4Stack currentMatrix_;


    public static Matrix4x4 ModelViewMatrix => modelViewMatrix_.Top;
    public static Matrix4x4 ProjectionMatrix => projectionMatrix_.Top;

    public unsafe static void UniformMatrix4(int location, Matrix4x4 matrix) {
      var ptr = &(matrix.M11);
      GL.UniformMatrix4(location, 1, false, ptr);
    }

    public static void PushMatrix() => currentMatrix_.Push();
    public static void PopMatrix() => currentMatrix_.Pop();

    public static unsafe void PassMatricesIntoGl() {
      var projection = projectionMatrix_.Top;
      GL.MatrixMode(GlMatrixMode.Projection);
      GL.LoadMatrix(&(projection.M11));

      var modelView = modelViewMatrix_.Top;
      GL.MatrixMode(GlMatrixMode.Modelview);
      GL.LoadMatrix(&(modelView.M11));
    }

    public static void MatrixMode(GlMatrixMode mode)
      => currentMatrix_ = mode switch {
        GlMatrixMode.Projection => projectionMatrix_,
        GlMatrixMode.Modelview => modelViewMatrix_,
      };

    public static void LoadIdentity() {
      currentMatrix_.SetIdentity();
    }

    public static void MultMatrix(Matrix4x4 matrix)
      => currentMatrix_.MultiplyInPlace(matrix);


    public static void Translate(double x, double y, double z)
      => Translate((float)x, (float)y, (float)z);
    public static void Translate(float x, float y, float z)
      => MultMatrix(Matrix4x4.CreateTranslation(x, y, z));


    public static void Scale(double x, double y, double z)
      => Scale((float)x, (float)y, (float)z);
    public static void Scale(float x, float y, float z)
      => MultMatrix(Matrix4x4.CreateScale(x, y, z));


    public static void Rotate(double angle, double x, double y, double z)
      => Rotate((float)angle, (float)x, (float)y, (float)z);

    public static void Rotate(float angle, float x, float y, float z)
      => MultMatrix(Matrix4x4.CreateFromAxisAngle(new Vector3(x, y, z), angle / 180 * MathF.PI));


    public static void Perspective(double fovYDegrees,
                                 double aspectRatio,
                                 double zNear,
                                 double zFar) {
      var matrix = new Matrix4x4();

      var f = 1.0 / Math.Tan(fovYDegrees / 180 * Math.PI / 2);

      SetInMatrix(ref matrix, 0, 0, f / aspectRatio);
      SetInMatrix(ref matrix, 1, 1, f);
      SetInMatrix(ref matrix, 2, 2, (zNear + zFar) / (zNear - zFar));
      SetInMatrix(ref matrix, 3, 2, 2 * zNear * zFar / (zNear - zFar));
      SetInMatrix(ref matrix, 2, 3, -1);

      MultMatrix(matrix);
    }

    public static void Ortho2d(int left, int right, int bottom, int top) {
      var near = -1;
      var far = 1;

      var matrix = new Matrix4x4();

      SetInMatrix(ref matrix, 0, 0, 2f / (right - left));
      SetInMatrix(ref matrix, 1, 1, 2f / (top - bottom));
      SetInMatrix(ref matrix, 2, 2, -2f / (far - near));
      SetInMatrix(ref matrix, 3, 0, -(1f * right + left) / (right - left));
      SetInMatrix(ref matrix, 3, 1, -(1f * top + bottom) / (top - bottom));
      SetInMatrix(ref matrix, 3, 2, -(1f * far + near) / (far - near));
      SetInMatrix(ref matrix, 3, 3, 1);

      MultMatrix(matrix);
    }

    public static void LookAt(
        double eyeX,
        double eyeY,
        double eyeZ,
        double centerX,
        double centerY,
        double centerZ,
        double upX,
        double upY,
        double upZ) {
      var lookX = centerX - eyeX;
      var lookY = centerY - eyeY;
      var lookZ = centerZ - eyeZ;
      Normalize3(ref lookX, ref lookY, ref lookZ);

      CrossProduct3(
          lookX, lookY, lookZ,
          upX, upY, upZ,
          out var sideX, out var sideY, out var sideZ);
      Normalize3(ref sideX, ref sideY, ref sideZ);

      CrossProduct3(
          sideX, sideY, sideZ,
          lookX, lookY, lookZ,
          out upX, out upY, out upZ);

      var matrix = new Matrix4x4();

      SetInMatrix(ref matrix, 0, 0, sideX);
      SetInMatrix(ref matrix, 1, 0, sideY);
      SetInMatrix(ref matrix, 2, 0, sideZ);

      SetInMatrix(ref matrix, 0, 1, upX);
      SetInMatrix(ref matrix, 1, 1, upY);
      SetInMatrix(ref matrix, 2, 1, upZ);

      SetInMatrix(ref matrix, 0, 2, -lookX);
      SetInMatrix(ref matrix, 1, 2, -lookY);
      SetInMatrix(ref matrix, 2, 2, -lookZ);

      SetInMatrix(ref matrix, 3, 3, 1);

      MultMatrix(matrix);
      Translate(-eyeX, -eyeY, -eyeZ);
    }

    public static void SetInMatrix(ref Matrix4x4 matrix, int r, int c, double value)
      => matrix[r, c] = (float) value;

    public static void CrossProduct3(
        double x1,
        double y1,
        double z1,
        double x2,
        double y2,
        double z2,
        out double outX,
        out double outY,
        out double outZ) {
      outX = y1 * z2 - z1 * y2;
      outY = z1 * x2 - x1 * z2;
      outZ = x1 * y2 - y1 * x2;
    }

    public static void Normalize3(ref double x, ref double y, ref double z) {
      var length = Math.Sqrt(x * x + y * y + z * z);
      x /= length;
      y /= length;
      z /= length;
    }

    public static unsafe void MultMatrix(IReadOnlyFinMatrix4x4 matrix) {
      if (matrix is FinMatrix4x4 matrixImpl) {
        MultMatrix(matrixImpl.impl_);
        return;
      }

      var buffer = new Matrix4x4();
      for (var y = 0; y < 4; ++y) {
        for (var x = 0; x < 4; ++x) {
          buffer[y, x] = matrix[x, y];
        }
      }
      MultMatrix(buffer);
    }
  }
}