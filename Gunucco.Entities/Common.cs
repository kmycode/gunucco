using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gunucco.Entities
{
    internal class DBIgnoreAttribute
#if GUNUCCO
        : System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute
#else
        : Attribute
#endif
    {
    }

    public abstract class GunuccoEntityBase
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class GunuccoEntityComparer<T> : IComparer<T>, IEqualityComparer<T>
        where T : GunuccoEntityBase
    {
        public int Compare(T x, T y)
        {
            if (x.Id > y.Id)
            {
                return 1;
            }
            if (x.Id < y.Id)
            {
                return -1;
            }
            return 0;
        }

        public bool Equals(T x, T y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(T obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public class GunuccoEntityComparer : GunuccoEntityComparer<GunuccoEntityBase>
    {
    }
}
