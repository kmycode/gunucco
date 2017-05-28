using Gunucco.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Entities.Helpers
{
    public class TreeEntity<T> where T : GunuccoEntityBase
    {
        public T Item { get; set; }

        // public IEnumerable<T> Children { get; set; } = new List<T>();

        public int Depth { get; set; }

        private bool IsDepthChecked { get; set; }

        /// <summary>
        /// Convert tree entity array to tree-depth/reordered array
        /// </summary>
        /// <param name="items"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static IEnumerable<TreeEntity<T>> FromEntities(IEnumerable<T> items, Func<T, int?> parentId)
        {
            var entities = items.Select(i => new TreeEntity<T> { Item = i, Depth = 0, }).ToArray();
            var copiedEntities = entities.ToArray();

            // get tree item depthes
            foreach (var entity in entities)
            {
                entity.Depth = CheckItemDepth(entity, copiedEntities, parentId);
                entity.IsDepthChecked = true;
            }

            // reorder tree for clarifying parent-child relations
            var shaped = new List<TreeEntity<T>>();
            int j = 0;
            var depthedEntities = copiedEntities.Where(item => item.Depth == j);
            while (depthedEntities.Any())
            {
                if (j == 0)
                {
                    // root items
                    foreach (var child in depthedEntities)
                    {
                        shaped.Add(child);
                    }
                }
                else
                {
                    // child items
                    foreach (var child in depthedEntities)
                    {
                        var parentLastChild = shaped.LastOrDefault(e => parentId(child.Item) == e.Item.Id ||
                                                                        parentId(child.Item) == parentId(e.Item));
                        if (parentLastChild != null)
                        {
                            var index = shaped.IndexOf(parentLastChild) + 1;
                            shaped.Insert(index, child);
                        }
                    }
                }

                j++;
                depthedEntities = copiedEntities.Where(item => item.Depth == j);
            }

            return shaped;
        }

        private static int CheckItemDepth(TreeEntity<T> item, IEnumerable<TreeEntity<T>> items, Func<T, int?> parentId)
        {
            if (item.IsDepthChecked)
            {
                return item.Depth;
            }
            
            var parent = items.SingleOrDefault(i => i.Item.Id == (parentId(item.Item) ?? -1));
            if (parent != null)
            {
                item.Depth = CheckItemDepth(parent, items, parentId) + 1;
                item.IsDepthChecked = true;
                return item.Depth;
            }
            else
            {
                item.Depth = 0;
                item.IsDepthChecked = true;
                return item.Depth;
            }
        }
    }
}
