using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Asteroids.Content {

    public static class Extensions {

        public static void Drop<T>(this IList<T> list ) {
            list.RemoveAt(list.Count - 1);
        }

    }
}