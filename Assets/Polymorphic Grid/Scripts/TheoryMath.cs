using System.Linq;
using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Static class for handling vectors, mesh and math problems.
    /// </summary>
    public static class TheoryMath
    {
        /// <summary>
        /// Flip all normals in the mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="flipTriangles">Flip triangles face direction</param>
        public static void FlipNormals(this Mesh mesh, bool flipTriangles = false)
        {
            Vector3[] norms = mesh.normals;
            for (int i = 0; i < norms.Length; i++)
                norms[i] = -norms[i];
            mesh.normals = norms;

            if(flipTriangles)
            {
                int[] tris = mesh.triangles;
                for (int i = 0; i < tris.Length; i += 3)
                {
                    tris[i] += tris[i + 2];
                    tris[i + 2] = tris[i] - tris[i + 2];
                    tris[i] -= tris[i + 2];
                }

                mesh.triangles = tris;
            }
        }

        /// <summary>
        /// Scale array of points to a given center (make all points closer of further of the center depending on scale value).
        /// </summary>
        /// <param name="center">Center to scale points to it</param>
        /// <param name="scale">Scale multiplyer value</param>
        /// <param name="points">points to scale</param>
        /// <returns></returns>
        public static Vector3[] ScaleAtCenter(Vector3 center, float scale, Vector3[] points)
        {
            return (from point in points select center + (point - center) * scale).ToArray();
        }

        /// <summary>
        /// Scale array of points to a given center (make all points closer of further of the center depending on scale value).
        /// </summary>
        /// <param name="scale">Scale multiplyer value</param>
        /// <param name="points">points to scale</param>
        /// <returns></returns>
        public static Vector3[] ScaleAtCenter(float scale, Vector3[] points)
        {
            return ScaleAtCenter(FindCenter(points), scale, points);
        }

        /// <summary>
        /// Fill a loop of vertices using a mesh by creating a center point and connect every point with it.
        /// </summary>
        /// <param name="center">Center point to connect all vertices with</param>
        /// <param name="up">Plane normal</param>
        /// <param name="points">Points loop to fill</param>
        /// <returns></returns>
        public static Mesh FilllAtCenter(Vector3 center, Vector3 up, params Vector3[] points)
        {
            Mesh mesh = new Mesh();
            Vector3 forward = (points[0] - center).normalized;
            Vector3 right = Vector3.Cross(forward, up);
            Vector3[] vertices = new Vector3[points.Length + 1];
            Vector2[] uvs = new Vector2[points.Length + 1];
            int[] triangles = new int[points.Length * 3];
            Vector3 min = points[0], max = points[0];

            for (int i = 0, t = 0; i < points.Length; i++)
            {
                if (points[i].x > max.x)
                    max.x = points[i].x;

                if (points[i].y > max.y)
                    max.y = points[i].y;

                if (points[i].z > max.z)
                    max.z = points[i].z;

                if (points[i].x < min.x)
                    min.x = points[i].x;

                if (points[i].y < min.y)
                    min.y = points[i].y;

                if (points[i].z < min.z)
                    min.z = points[i].z;

                vertices[i] = points[i];
                triangles[t++] = i;
                triangles[t++] = (i + 1) % points.Length;
                triangles[t++] = points.Length;
            }

            float maxX = Vector3.Project(max - min, right).magnitude;
            float maxY = Vector3.Project(max - min, forward).magnitude;
            Vector3 project;

            for (int i = 0; i < vertices.Length; i++)
            {
                project = vertices[i] - min;
                uvs[i] = new Vector2(Vector3.Project(project, right).magnitude / maxX, Vector3.Project(project, forward).magnitude / maxY);
            }

            vertices[points.Length] = center;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            if (mesh.normals[0] != up)
                mesh.FlipNormals(true);

            return mesh;
        }

        /// <summary>
        /// Fill a loop of vertices using a mesh by creating a center point and connect every point with it.
        /// </summary>
        /// <param name="points">Points loop to fill</param>
        /// <returns></returns>
        public static Mesh FilllAtCenter(params Vector3[] points)
        {
            Vector3 c = FindCenter(points);
            return FilllAtCenter(c, GetNormal(c, points[0], points[1]), points);
        }

        /// <summary>
        /// Join every vertix in the first array with other vertix at the same index in the second array and make an array of planes.
        /// </summary>
        /// <param name="a">First points array</param>
        /// <param name="b">Second points array</param>
        /// <returns></returns>
        public static Mesh BridgeEdgeLoops(Vector3[] a, Vector3[] b, bool inverseNormals = false)
        {
            int length = a.Length;
            Vector3[] vertices = new Vector3[length * 4];
            int[] tris = new int[length * 6];
            Vector2[] uvs = new Vector2[length * 4];
            int next, i = 0, vi = 0, ti = 0;

            for (; i < length; i++, vi += 4, ti += 6)
            {
                next = (i + 1) % length;
                vertices[vi + 0] = a[i];
                vertices[vi + 1] = a[next];
                vertices[vi + 2] = b[next];
                vertices[vi + 3] = b[i];
                uvs[vi + 0] = new Vector2(0, 1);
                uvs[vi + 1] = new Vector2(1, 1);
                uvs[vi + 2] = new Vector2(1, 0);
                uvs[vi + 3] = new Vector2(0, 0);
                tris[ti + 0] = vi + 0;
                tris[ti + 1] = vi + 1;
                tris[ti + 2] = vi + 2;
                tris[ti + 3] = vi + 2;
                tris[ti + 4] = vi + 3;
                tris[ti + 5] = vi + 0;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            if ((Vector3.Dot(FindCenter(a) - a[0], mesh.normals[0]) > 0) ^ inverseNormals)
                mesh.FlipNormals(true);

            return mesh;
        }

        /// <summary>
        /// Combine number of meshes in one mesh and return that mesh.
        /// </summary>
        /// <param name="meshes">Meshes to combine</param>
        /// <returns></returns>
        public static Mesh CombineMeshes(params Mesh[] meshes)
        {
            int vertsCount = 0;
            int trisCount = 0;
            int normsCount = 0;
            int uvsCount = 0;

            foreach (Mesh piece in meshes)
            {
                vertsCount += piece.vertices.Length;
                trisCount += piece.triangles.Length;
                normsCount += piece.normals.Length;
                uvsCount += piece.uv.Length;
            }

            Vector3[] vertices = new Vector3[vertsCount];
            int[] tris = new int[trisCount];
            Vector2[] uvs = new Vector2[uvsCount];
            Vector3[] normals = new Vector3[normsCount];

            int l = 0, m = 0, j = 0, k = 0, n = 0, o = 0;
            for (int i = 0; i < meshes.Length; i++)
            {
                while (j - l < meshes[i].vertices.Length)
                {
                    vertices[j] = meshes[i].vertices[j - l];
                    normals[j] = meshes[i].normals[j - l];
                    j++;
                }
                while (n - o < meshes[i].uv.Length)
                {
                    uvs[n] = meshes[i].uv[n - o];
                    n++;
                }
                while (k - m < meshes[i].triangles.Length)
                {
                    tris[k] = meshes[i].triangles[k - m] + l;
                    k++;
                }
                l = j;
                m = k;
                o = n;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = tris;
            mesh.uv = uvs;

            return mesh;
        }

        /// <summary>
        /// Create a plane and return its normal.
        /// </summary>
        /// <param name="A">Triangle Point A</param>
        /// <param name="B">Triangle Point B</param>
        /// <param name="C">Triangle Point C</param>
        /// <returns></returns>
        public static Vector3 GetNormal(Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 ab = B - A;
            Vector3 ac = C - A;
            return Vector3.Cross(ab, ac);
        }

        /// <summary>
        /// Check if a point is inside a convex Polymorphicgon.
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <param name="center">Center of the Polymorphicgon</param>
        /// <param name="PolymorphicgonPoints">Points which create the Polymorphicgon</param>
        /// <returns></returns>
        public static bool PointInsideConvexPolymorphicgon(Vector3 p, Vector3 center, params Vector3[] PolymorphicgonPoints)
        {
            Vector3 mainDir, centerDir, projected, origin;
            for (int i = 0; i < PolymorphicgonPoints.Length; i++)
            {
                mainDir = PolymorphicgonPoints[(i + 1) % PolymorphicgonPoints.Length] - PolymorphicgonPoints[i];
                centerDir = center - PolymorphicgonPoints[i];
                projected = Vector3.Project(centerDir, mainDir);
                origin = PolymorphicgonPoints[i] + projected;
                if (Vector3.Dot(center - origin, p - origin) < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if a point is inside a convex Polymorphicgon.
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <param name="PolymorphicgonPoints">Points which create the Polymorphicgon</param>
        /// <returns></returns>
        public static bool PointInsideConvexPolygon(Vector3 p, params Vector3[] PolymorphicgonPoints)
        {
            return PointInsideConvexPolymorphicgon(p, FindCenter(PolymorphicgonPoints), PolymorphicgonPoints);
        }

        /// <summary>
        /// Getting the center of points by calculating avarege for each x, y and z coordinates.
        /// </summary>
        /// <param name="points">Points to find center for</param>
        /// <returns></returns>
        public static Vector3 FindCenter(params Vector3[] points)
        {
            Vector3 result = Vector3.zero;

            foreach (Vector3 vector in points)
                result += vector;

            return result / points.Length;
        }

        /// <summary>
        /// Round floating point number to a number with less amount of digits to avoid calculation mistakes.
        /// </summary>
        /// <param name="num">Number to round</param>
        /// <param name="digitsCount">Number of digits to remove</param>
        /// <returns></returns>
        public static float GetApprox(this float num, int digitsCount)
        {
            int beforePoint = 0;
            for (int current = (int)num; current != 0; current /= 10)
                beforePoint++;

            int start = 7 - beforePoint - digitsCount;
            float result = num;
            for (int i = start; i > 0; i--)
                result *= 10;
            result = Mathf.Round(result);
            for (int i = start; i > 0; i--)
                result *= .1f;
            return result;
        }

        public static Vector3 GetApprox(this Vector3 vector, int digitsCount) =>
            new Vector3(vector.x.GetApprox(digitsCount), vector.y.GetApprox(digitsCount), vector.z.GetApprox(digitsCount));
    }
}
