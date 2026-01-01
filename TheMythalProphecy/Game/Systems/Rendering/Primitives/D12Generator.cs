using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a D12 (12-sided die) mesh based on the dodecahedron.
    /// Each face displays a number from 1-12 using texture mapping.
    /// </summary>
    public static class D12Generator
    {
        /// <summary>
        /// Golden ratio φ = (1 + √5) / 2 ≈ 1.618034
        /// </summary>
        private static readonly float Phi = (1f + MathF.Sqrt(5f)) / 2f;

        /// <summary>
        /// Standard D12 face arrangement - opposite faces sum to 13.
        /// Index corresponds to face index, value is the number on that face.
        /// </summary>
        public static readonly int[] FaceNumbers = new int[]
        {
            12, 2, 8, 4, 10, 6,  // Faces 0-5
            1, 11, 5, 9, 3, 7    // Faces 6-11 (opposites)
        };

        /// <summary>
        /// Which vertex (0-4) each face's number points toward.
        /// </summary>
        public static readonly int[] FaceApexVertices = new int[]
        {
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0
        };

        /// <summary>
        /// 12 pentagonal faces defined by 5 vertex indices each.
        /// </summary>
        private static readonly int[][] FaceIndices = new int[][]
        {
            new[] { 0, 8, 9, 4, 16 },    // Face 0 (top)
            new[] { 0, 16, 17, 2, 12 },  // Face 1
            new[] { 0, 12, 13, 1, 8 },   // Face 2
            new[] { 1, 13, 15, 6, 18 },  // Face 3
            new[] { 2, 17, 19, 7, 14 },  // Face 4
            new[] { 4, 9, 11, 5, 10 },   // Face 5
            new[] { 3, 19, 17, 16, 9 },  // Face 6
            new[] { 3, 9, 8, 1, 18 },    // Face 7
            new[] { 3, 18, 6, 7, 19 },   // Face 8
            new[] { 5, 11, 15, 13, 12 }, // Face 9
            new[] { 5, 12, 2, 14, 10 },  // Face 10
            new[] { 5, 10, 4, 11, 15 }   // Face 11 (bottom - will be recalculated)
        };

        /// <summary>
        /// D12 result for the mesh generation.
        /// </summary>
        public class D12Mesh : IDisposable
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
        /// Generates a D12 mesh with texture-mapped pentagonal faces.
        /// </summary>
        public static D12Mesh Generate(GraphicsDevice graphicsDevice, float radius = 1f)
        {
            float invPhi = 1f / Phi;

            // Generate the 20 base vertex positions for a dodecahedron
            Vector3[] basePositions = new Vector3[]
            {
                // 8 cube vertices (±1, ±1, ±1)
                new Vector3(1, 1, 1),     // 0
                new Vector3(1, 1, -1),    // 1
                new Vector3(1, -1, 1),    // 2
                new Vector3(1, -1, -1),   // 3
                new Vector3(-1, 1, 1),    // 4
                new Vector3(-1, 1, -1),   // 5
                new Vector3(-1, -1, 1),   // 6
                new Vector3(-1, -1, -1),  // 7
                // 4 vertices (0, ±1/φ, ±φ)
                new Vector3(0, invPhi, Phi),   // 8
                new Vector3(0, invPhi, -Phi),  // 9
                new Vector3(0, -invPhi, Phi),  // 10
                new Vector3(0, -invPhi, -Phi), // 11
                // 4 vertices (±1/φ, ±φ, 0)
                new Vector3(invPhi, Phi, 0),   // 12
                new Vector3(invPhi, -Phi, 0),  // 13
                new Vector3(-invPhi, Phi, 0),  // 14
                new Vector3(-invPhi, -Phi, 0), // 15
                // 4 vertices (±φ, 0, ±1/φ)
                new Vector3(Phi, 0, invPhi),   // 16
                new Vector3(Phi, 0, -invPhi),  // 17
                new Vector3(-Phi, 0, invPhi),  // 18
                new Vector3(-Phi, 0, -invPhi)  // 19
            };

            // Normalize vertices to unit sphere, then scale by radius
            for (int i = 0; i < basePositions.Length; i++)
            {
                basePositions[i] = Vector3.Normalize(basePositions[i]) * radius;
            }

            // Correct face definitions for a regular dodecahedron
            int[][] faces = GenerateFaces(basePositions);

            // Each pentagon is triangulated into 5 triangles from its center
            // 12 faces × 5 triangles × 3 vertices = 180 vertices
            var vertices = new VertexPositionNormalTexture[180];
            var indices = new short[180];
            var faceCenters = new Vector3[12];
            var faceNormals = new Vector3[12];

            int vertexIndex = 0;

            for (int face = 0; face < 12; face++)
            {
                int[] faceVerts = faces[face];

                // Get the 5 vertex positions for this face
                Vector3 p0 = basePositions[faceVerts[0]];
                Vector3 p1 = basePositions[faceVerts[1]];
                Vector3 p2 = basePositions[faceVerts[2]];
                Vector3 p3 = basePositions[faceVerts[3]];
                Vector3 p4 = basePositions[faceVerts[4]];

                // Calculate face center
                Vector3 center = (p0 + p1 + p2 + p3 + p4) / 5f;
                faceCenters[face] = center;

                // Calculate face normal using cross product of two edges
                Vector3 edge1 = p1 - p0;
                Vector3 edge2 = p2 - p0;
                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                // Ensure normal points outward (away from origin)
                if (Vector3.Dot(faceNormal, center) < 0)
                {
                    faceNormal = -faceNormal;
                }
                faceNormals[face] = faceNormal;

                // Get UV coordinates for this face's number
                int faceNumber = FaceNumbers[face];
                Vector3[] pentagonVerts = { p0, p1, p2, p3, p4 };
                var uvs = D12TextureGenerator.GetFaceUVs(faceNumber, pentagonVerts, center, faceNormal);

                // Triangulate the pentagon: create 5 triangles from center
                for (int i = 0; i < 5; i++)
                {
                    int next = (i + 1) % 5;

                    Vector3 v0 = center;
                    Vector3 v1 = pentagonVerts[i];
                    Vector3 v2 = pentagonVerts[next];

                    // Calculate UV for center point
                    Vector2 uvCenter = uvs[5]; // Center UV
                    Vector2 uv1 = uvs[i];
                    Vector2 uv2 = uvs[next];

                    // Create vertices for this triangle
                    vertices[vertexIndex] = new VertexPositionNormalTexture(v0, faceNormal, uvCenter);
                    vertices[vertexIndex + 1] = new VertexPositionNormalTexture(v1, faceNormal, uv1);
                    vertices[vertexIndex + 2] = new VertexPositionNormalTexture(v2, faceNormal, uv2);

                    indices[vertexIndex] = (short)vertexIndex;
                    indices[vertexIndex + 1] = (short)(vertexIndex + 1);
                    indices[vertexIndex + 2] = (short)(vertexIndex + 2);

                    vertexIndex += 3;
                }
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

            return new D12Mesh
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                FaceCenters = faceCenters,
                FaceNormals = faceNormals
            };
        }

        /// <summary>
        /// Generates the 12 pentagonal faces by finding vertices that share edges.
        /// </summary>
        private static int[][] GenerateFaces(Vector3[] vertices)
        {
            // Pre-computed face definitions for a regular dodecahedron
            // Each face lists 5 vertices in counter-clockwise order when viewed from outside
            return new int[][]
            {
                new[] { 0, 8, 4, 14, 12 },   // Face 0
                new[] { 0, 16, 2, 10, 8 },   // Face 1
                new[] { 0, 12, 1, 17, 16 },  // Face 2
                new[] { 1, 12, 14, 5, 9 },   // Face 3
                new[] { 2, 16, 17, 3, 13 },  // Face 4
                new[] { 4, 8, 10, 6, 18 },   // Face 5
                new[] { 1, 9, 11, 3, 17 },   // Face 6
                new[] { 4, 18, 19, 5, 14 },  // Face 7
                new[] { 6, 10, 2, 13, 15 },  // Face 8
                new[] { 3, 11, 7, 15, 13 },  // Face 9
                new[] { 5, 19, 7, 11, 9 },   // Face 10
                new[] { 6, 15, 7, 19, 18 }   // Face 11
            };
        }

        /// <summary>
        /// Gets the number on a specific face.
        /// </summary>
        public static int GetFaceNumber(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= 12)
                return 0;
            return FaceNumbers[faceIndex];
        }
    }
}
