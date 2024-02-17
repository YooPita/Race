using NUnit.Framework;
using UnityEngine;

namespace Retrover.Math
{
    [TestFixture]
    public class TriangleTests
    {
        [Test]
        public void TestContainsPointInside()
        {
            Triangle triangle = new(new(0, 0), new(10, 0), new(5, 10));
            Vector2 pointInside = new(5, 5);
            Assert.IsTrue(triangle.ContainsInCircumcircle(pointInside),
                "A point inside the circumcircle must be detected.");
        }

        [Test]
        public void TestContainsPointOutside()
        {
            Triangle triangle = new(new(0, 0), new(10, 0), new(5, 10));
            Vector2 pointOutside = new(20, 20);
            Assert.IsFalse(triangle.ContainsInCircumcircle(pointOutside),
                "A point outside the described environment shall not be detected.");
        }

        [Test]
        public void TestWithVerticesAlmostOnOneLine()
        {
            Triangle triangle = new(new(0, 0), new(1000, 0.001f), new(2000, -0.001f));
            Vector2 pointNear = new(1000, 0);
            Assert.IsFalse(triangle.ContainsInCircumcircle(pointNear),
                "A triangle with vertices almost on the same line should not incorrectly determine the identity of a point.");
        }
    }
}
