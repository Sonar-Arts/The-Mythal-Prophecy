using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.Systems.Rendering.Primitives
{
    /// <summary>
    /// Generates a D4 (4-sided die) mesh based on the tetrahedron.
    /// Each face displays a number from 1-4 using texture mapping.
    /// </summary>
    public static class D4Generator
    {
        /// <summary>
        /// Standard D4 face arrangement.
        /// Index corresponds to face index, value is the number on that face.
        /// </summary>
        public static readonly int[] FaceNumbers = new int[]
        {
            1, 2, 3, 4
        };

        /// <summary>
        /// 4 triangular faces defined by 3 vertex indices each.
        /// First vertex of each face determines the "up" direction for UV mapping.
        /// For side faces: apex vertex first (number points up toward apex)
        /// For bottom face: front vertex first
        /// </summary>
        private static readonly int[][] FaceIndices = new int[][]
        {
            new[] { 0, 2, 1 },  // Face 0: front-right (apex first)
            new[] { 0, 3, 2 },  // Face 1: back-right (apex first)
            new[] { 0, 1, 3 },  // Face 2: back-left (apex first)
            new[] { 1, 2, 3 }   // Face 3: bottom (front vertex first)
        };

        /// <summary>
        /// D4 mesh result for the mesh generation.
        /// </summary>
        public class D4Mesh : IDisposable
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
        /// Generates a D4 mesh with texture-mapped triangular faces.
        /// </summary>
        public static D4Mesh Generate(GraphicsDevice graphicsDevice, float radius = 1f)
        {
            // Regular tetrahedron vertices
            // Using coordinates that create a regular tetrahedron centered at origin
            float a = radius * 0.8f;

            // Tetrahedron with one vertex at top and base triangle below
            float topY = a;
            float baseY = -a * 0.333f;
            float baseR = a * 0.943f; // sqrt(8/9) * a

            // Base triangle vertices at 120 degree intervals
            float angle1 = 0f;                          // Front
            float angle2 = MathHelper.TwoPi / 3f;       // Back-right
            float angle3 = 2f * MathHelper.TwoPi / 3f;  // Back-left

            Vector3[] basePositions = new Vector3[]
            {
                new Vector3(0, topY, 0),                                              // 0: Apex (top)
                new Vector3(MathF.Sin(angle1) * baseR, baseY, MathF.Cos(angle1) * baseR),  // 1: Front base
                new Vector3(MathF.Sin(angle2) * baseR, baseY, MathF.Cos(angle2) * baseR),  // 2: Back-right base
                new Vector3(MathF.Sin(angle3) * baseR, baseY, MathF.Cos(angle3) * baseR)   // 3: Back-left base
            };

            // 4 faces × 3 vertices = 12 vertices
            var vertices = new VertexPositionNormalTexture[12];
            var indices = new short[12];
            var faceCenters = new Vector3[4];
            var faceNormals = new Vector3[4];

            int vertexIndex = 0;

            for (int face = 0; face < 4; face++)
            {
                int[] faceVerts = FaceIndices[face];

                // Get the 3 vertex positions for this face
                Vector3 p0 = basePositions[faceVerts[0]];
                Vector3 p1 = basePositions[faceVerts[1]];
                Vector3 p2 = basePositions[faceVerts[2]];

                // Calculate face center
                Vector3 center = (p0 + p1 + p2) / 3f;
                faceCenters[face] = center;

                // Calculate face normal using cross product
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
                Vector3[] triangleVerts = { p0, p1, p2 };
                var uvs = D4TextureGenerator.GetFaceUVs(faceNumber, triangleVerts, center, faceNormal);

                // Create vertices for this triangle
                vertices[vertexIndex] = new VertexPositionNormalTexture(p0, faceNormal, uvs[0]);
                vertices[vertexIndex + 1] = new VertexPositionNormalTexture(p1, faceNormal, uvs[1]);
                vertices[vertexIndex + 2] = new VertexPositionNormalTexture(p2, faceNormal, uvs[2]);

                indices[vertexIndex] = (short)vertexIndex;
                indices[vertexIndex + 1] = (short)(vertexIndex + 1);
                indices[vertexIndex + 2] = (short)(vertexIndex + 2);

                vertexIndex += 3;
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

            return new D4Mesh
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
            if (faceIndex < 0 || faceIndex >= 4)
                return 0;
            return FaceNumbers[faceIndex];
        }
    }
}
