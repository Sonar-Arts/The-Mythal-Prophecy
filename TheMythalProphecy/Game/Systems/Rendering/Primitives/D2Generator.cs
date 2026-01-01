using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a D2 (coin) mesh - a cylinder with two circular faces.
    /// Face 0 (top) displays "1", Face 1 (bottom) displays "2".
    /// </summary>
    public static class D2Generator
    {
        private const int EdgeSegments = 32; // Number of segments around the coin edge
        private const float CoinThickness = 0.15f; // Relative thickness of the coin

        /// <summary>
        /// Standard D2 face arrangement.
        /// Index corresponds to face index, value is the number on that face.
        /// </summary>
        public static readonly int[] FaceNumbers = new int[]
        {
            1, 2
        };

        /// <summary>
        /// D2 mesh result for the mesh generation.
        /// </summary>
        public class D2Mesh : IDisposable
        {
            public VertexBuffer VertexBuffer { get; init; }
            public IndexBuffer IndexBuffer { get; init; }
            public Vector3[] FaceCenters { get; init; }
            public Vector3[] FaceNormals { get; init; }

            public void Dispose()
            {
                VertexBuffer?.Dispose();
                IndexBuffer?.Dispose();
            }
        }

        /// <summary>
        /// Generates a D2 mesh with textured circular faces.
        /// </summary>
        public static D2Mesh Generate(GraphicsDevice graphicsDevice, float radius = 1f)
        {
            float halfHeight = radius * CoinThickness / 2f;

            // Calculate vertex and index counts
            // Top face: EdgeSegments triangles (fan from center)
            // Bottom face: EdgeSegments triangles (fan from center)
            // Edge: EdgeSegments * 2 triangles (quad strip)
            int topFaceVertices = EdgeSegments + 1; // center + ring
            int bottomFaceVertices = EdgeSegments + 1;
            int edgeVertices = EdgeSegments * 4; // 2 rings * 2 (for separate normals)
            int totalVertices = topFaceVertices + bottomFaceVertices + edgeVertices;

            int topFaceIndices = EdgeSegments * 3;
            int bottomFaceIndices = EdgeSegments * 3;
            int edgeIndices = EdgeSegments * 6; // 2 triangles per segment
            int totalIndices = topFaceIndices + bottomFaceIndices + edgeIndices;

            var vertices = new VertexPositionNormalTexture[totalVertices];
            var indices = new short[totalIndices];
            var faceCenters = new Vector3[2];
            var faceNormals = new Vector3[2];

            int vertexIndex = 0;
            int indexIndex = 0;

            // Face centers and normals
            faceCenters[0] = new Vector3(0, halfHeight, 0);
            faceCenters[1] = new Vector3(0, -halfHeight, 0);
            faceNormals[0] = Vector3.Up;
            faceNormals[1] = Vector3.Down;

            // === TOP FACE (number 1) ===
            int topCenterIndex = vertexIndex;

            // Get UVs for top face
            var topUVs = D2TextureGenerator.GetFaceUVs(1, radius);

            // Center vertex
            vertices[vertexIndex++] = new VertexPositionNormalTexture(
                new Vector3(0, halfHeight, 0),
                Vector3.Up,
                topUVs.Center);

            // Ring vertices
            for (int i = 0; i < EdgeSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / EdgeSegments;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;

                vertices[vertexIndex++] = new VertexPositionNormalTexture(
                    new Vector3(x, halfHeight, z),
                    Vector3.Up,
                    topUVs.GetRingUV(angle));
            }

            // Top face indices (triangle fan)
            for (int i = 0; i < EdgeSegments; i++)
            {
                indices[indexIndex++] = (short)topCenterIndex;
                indices[indexIndex++] = (short)(topCenterIndex + 1 + i);
                indices[indexIndex++] = (short)(topCenterIndex + 1 + (i + 1) % EdgeSegments);
            }

            // === BOTTOM FACE (number 2) ===
            int bottomCenterIndex = vertexIndex;

            // Get UVs for bottom face
            var bottomUVs = D2TextureGenerator.GetFaceUVs(2, radius);

            // Center vertex
            vertices[vertexIndex++] = new VertexPositionNormalTexture(
                new Vector3(0, -halfHeight, 0),
                Vector3.Down,
                bottomUVs.Center);

            // Ring vertices (reversed winding for correct facing)
            for (int i = 0; i < EdgeSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / EdgeSegments;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;

                vertices[vertexIndex++] = new VertexPositionNormalTexture(
                    new Vector3(x, -halfHeight, z),
                    Vector3.Down,
                    bottomUVs.GetRingUV(angle));
            }

            // Bottom face indices (triangle fan, reversed winding)
            for (int i = 0; i < EdgeSegments; i++)
            {
                indices[indexIndex++] = (short)bottomCenterIndex;
                indices[indexIndex++] = (short)(bottomCenterIndex + 1 + (i + 1) % EdgeSegments);
                indices[indexIndex++] = (short)(bottomCenterIndex + 1 + i);
            }

            // === EDGE (cylinder wall) ===
            int edgeStartIndex = vertexIndex;

            // Edge uses a simple gray UV (center of a non-numbered area or we'll use edge color)
            Vector2 edgeUV = new Vector2(0.75f, 0.5f); // Middle-right of texture (edge area)

            for (int i = 0; i < EdgeSegments; i++)
            {
                float angle = i * MathHelper.TwoPi / EdgeSegments;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;
                Vector3 edgeNormal = Vector3.Normalize(new Vector3(x, 0, z));

                // Top ring vertex
                vertices[vertexIndex++] = new VertexPositionNormalTexture(
                    new Vector3(x, halfHeight, z),
                    edgeNormal,
                    edgeUV);

                // Bottom ring vertex
                vertices[vertexIndex++] = new VertexPositionNormalTexture(
                    new Vector3(x, -halfHeight, z),
                    edgeNormal,
                    edgeUV);
            }

            // Edge indices (quad strip as triangles)
            for (int i = 0; i < EdgeSegments; i++)
            {
                int current = edgeStartIndex + i * 2;
                int next = edgeStartIndex + ((i + 1) % EdgeSegments) * 2;

                // Triangle 1
                indices[indexIndex++] = (short)current;
                indices[indexIndex++] = (short)(current + 1);
                indices[indexIndex++] = (short)next;

                // Triangle 2
                indices[indexIndex++] = (short)next;
                indices[indexIndex++] = (short)(current + 1);
                indices[indexIndex++] = (short)(next + 1);
            }

            // Create vertex buffer
            var vertexBuffer = new VertexBuffer(
                graphicsDevice,
                VertexPositionNormalTexture.VertexDeclaration,
                vertices.Length,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            // Create index buffer
            var indexBuffer = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits,
                indices.Length,
                BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            return new D2Mesh
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                FaceCenters = faceCenters,
                FaceNormals = faceNormals
            };
        }

        /// <summary>
        /// Gets the number on a specific face.
        /// </summary>
        public static int GetFaceNumber(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= 2)
                return 0;
            return FaceNumbers[faceIndex];
        }
    }
}
