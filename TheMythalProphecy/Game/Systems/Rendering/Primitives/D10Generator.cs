using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a D10 (10-sided die) mesh based on the pentagonal trapezohedron.
    /// Each face displays a number from 0-9 using texture mapping.
    /// </summary>
    public static class D10Generator
    {
        /// <summary>
        /// Standard D10 face arrangement - opposite faces sum to 9.
        /// Index corresponds to face index, value is the number on that face.
        /// </summary>
        public static readonly int[] FaceNumbers = new int[]
        {
            0, 2, 4, 6, 8,  // Top faces (around top vertex)
            9, 7, 5, 3, 1   // Bottom faces (opposites)
        };

        /// <summary>
        /// D10 mesh result for the mesh generation.
        /// </summary>
        public class D10Mesh : IDisposable
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
        /// Generates a D10 mesh with texture-mapped kite-shaped faces.
        /// </summary>
        public static D10Mesh Generate(GraphicsDevice graphicsDevice, float radius = 1f)
        {
            // Pentagonal trapezohedron geometry:
            // - 2 pole vertices (top and bottom)
            // - 2 rings of 5 vertices each, offset by 36 degrees
            // - Each face is a kite (4 vertices)

            // For planar kite faces, the ratio ringHeight/poleHeight must equal:
            // (2*sin(36°) - sin(72°)) / (sin(72°) + 2*sin(36°)) ≈ 0.1056
            // This ensures all 4 vertices of each kite lie on the same plane.
            float poleHeight = 1.0f * radius;
            float ringHeight = 0.1056f * radius;
            float ringRadius = 0.95f * radius;

            Vector3[] basePositions = new Vector3[12];

            // Top pole
            basePositions[0] = new Vector3(0, poleHeight, 0);

            // Top ring (5 vertices) - at angles 0, 72, 144, 216, 288 degrees
            for (int i = 0; i < 5; i++)
            {
                float angle = i * MathHelper.TwoPi / 5f;
                basePositions[1 + i] = new Vector3(
                    ringRadius * MathF.Cos(angle),
                    ringHeight,
                    ringRadius * MathF.Sin(angle)
                );
            }

            // Bottom ring (5 vertices) - offset by 36 degrees (half of 72)
            for (int i = 0; i < 5; i++)
            {
                float angle = (i + 0.5f) * MathHelper.TwoPi / 5f;
                basePositions[6 + i] = new Vector3(
                    ringRadius * MathF.Cos(angle),
                    -ringHeight,
                    ringRadius * MathF.Sin(angle)
                );
            }

            // Bottom pole
            basePositions[11] = new Vector3(0, -poleHeight, 0);

            // Each kite face is made of 2 triangles = 20 triangles total
            // 10 faces × 2 triangles × 3 vertices = 60 vertices
            var vertices = new VertexPositionNormalTexture[60];
            var indices = new short[60];
            var faceCenters = new Vector3[10];
            var faceNormals = new Vector3[10];

            int vertexIndex = 0;

            // Top 5 faces (connected to top pole)
            for (int i = 0; i < 5; i++)
            {
                int nextI = (i + 1) % 5;

                // Kite vertices: top pole, top ring[i], bottom ring[i], top ring[next]
                Vector3 p0 = basePositions[0];           // Top pole
                Vector3 p1 = basePositions[1 + i];       // Top ring vertex
                Vector3 p2 = basePositions[6 + i];       // Bottom ring vertex (between top ring vertices)
                Vector3 p3 = basePositions[1 + nextI];   // Next top ring vertex

                CreateKiteFace(vertices, indices, ref vertexIndex, i,
                    p0, p1, p2, p3, faceCenters, faceNormals);
            }

            // Bottom 5 faces (connected to bottom pole)
            for (int i = 0; i < 5; i++)
            {
                int nextI = (i + 1) % 5;

                // Kite vertices: bottom pole, bottom ring[next], top ring[next], bottom ring[i]
                Vector3 p0 = basePositions[11];          // Bottom pole
                Vector3 p1 = basePositions[6 + nextI];   // Next bottom ring vertex
                Vector3 p2 = basePositions[1 + nextI];   // Top ring vertex (between bottom ring vertices)
                Vector3 p3 = basePositions[6 + i];       // Bottom ring vertex

                CreateKiteFace(vertices, indices, ref vertexIndex, 5 + i,
                    p0, p1, p2, p3, faceCenters, faceNormals);
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

            return new D10Mesh
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                FaceCenters = faceCenters,
                FaceNormals = faceNormals
            };
        }

        private static void CreateKiteFace(
            VertexPositionNormalTexture[] vertices,
            short[] indices,
            ref int vertexIndex,
            int faceIndex,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            Vector3[] faceCenters, Vector3[] faceNormals)
        {
            // Calculate face center
            Vector3 center = (p0 + p1 + p2 + p3) / 4f;
            faceCenters[faceIndex] = center;

            // Calculate face normal using cross product
            Vector3 edge1 = p1 - p0;
            Vector3 edge2 = p3 - p0;
            Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

            // Ensure normal points outward (away from origin)
            if (Vector3.Dot(faceNormal, center) < 0)
            {
                faceNormal = -faceNormal;
            }
            faceNormals[faceIndex] = faceNormal;

            // Get UV coordinates for this face's number
            int faceNumber = FaceNumbers[faceIndex];
            Vector3[] kiteVerts = { p0, p1, p2, p3 };
            var uvs = D10TextureGenerator.GetFaceUVs(faceNumber, kiteVerts, center, faceNormal);

            // Triangulate the kite: 2 triangles
            // Triangle 1: p0, p1, p2
            vertices[vertexIndex] = new VertexPositionNormalTexture(p0, faceNormal, uvs[0]);
            vertices[vertexIndex + 1] = new VertexPositionNormalTexture(p1, faceNormal, uvs[1]);
            vertices[vertexIndex + 2] = new VertexPositionNormalTexture(p2, faceNormal, uvs[2]);

            indices[vertexIndex] = (short)vertexIndex;
            indices[vertexIndex + 1] = (short)(vertexIndex + 1);
            indices[vertexIndex + 2] = (short)(vertexIndex + 2);

            vertexIndex += 3;

            // Triangle 2: p0, p2, p3
            vertices[vertexIndex] = new VertexPositionNormalTexture(p0, faceNormal, uvs[0]);
            vertices[vertexIndex + 1] = new VertexPositionNormalTexture(p2, faceNormal, uvs[2]);
            vertices[vertexIndex + 2] = new VertexPositionNormalTexture(p3, faceNormal, uvs[3]);

            indices[vertexIndex] = (short)vertexIndex;
            indices[vertexIndex + 1] = (short)(vertexIndex + 1);
            indices[vertexIndex + 2] = (short)(vertexIndex + 2);

            vertexIndex += 3;
        }

        /// <summary>
        /// Gets the number on a specific face.
        /// </summary>
        public static int GetFaceNumber(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= 10)
                return 0;
            return FaceNumbers[faceIndex];
        }
    }
}
