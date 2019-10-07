using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.BindingFlags;
using System.Reflection;

namespace StolenAvatarChecker.Utils {
    static class ReflectionUtils {
        public static IDictionary<string, string> DeepDump(this object instance, bool useGetNames = false) {
            Type _in = instance.GetType();
            Dictionary<string, string> vals = new Dictionary<string, string>();
            PropertyInfo[] p_infos = instance == null ? _in.GetProperties(Public | NonPublic | Static | FlattenHierarchy)
                : _in.GetProperties(Public | NonPublic | Instance | Static | FlattenHierarchy);
            FieldInfo[] f_infos = instance == null ? _in.GetFields(Public | NonPublic | Static | FlattenHierarchy)
                : _in.GetFields(Public | NonPublic | Instance | Static | FlattenHierarchy);

            for (int i = 0; i < p_infos.Length; i++)
                vals[useGetNames ? p_infos[i].GetGetMethod() == null ? p_infos[i].Name :
                    p_infos[i].GetGetMethod().Name : p_infos[i].Name] = $"{p_infos[i].GetValue(instance, null)}";
            for (int i = 0; i < f_infos.Length; i++)
                vals[f_infos[i].Name] = $"{f_infos[i].GetValue(instance)}";

            return vals;
        }
        public static string DeepDumpAsString(this object instance, bool useGetNames = false) {
            IDictionary<string, string> outp = DeepDump(instance, useGetNames);
            return string.Join(Environment.NewLine, outp.Where(x => !x.Key.Contains("<") && !x.Value.Contains("<"))
                .Select(x => $"{{ {x.Key} : {x.Value} }}").ToArray());
        }
    }
}
