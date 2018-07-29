using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LMPatcher
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class Class : System.Attribute
    {
        public string ClassName;

        public Class(string className)
        {
            ClassName = className;
        }
    }

    public class Patch
    {
        private AssemblyDefinition assembly;
        public AssemblyDefinition Assembly {
            get
            {
                return assembly;
            }
            set
            {
                assembly = value;
                Class patch = (Class)System.Attribute.GetCustomAttribute(this.GetType(), typeof(Class));
                Type = assembly.MainModule.GetType(patch.ClassName);
            }
        }

        public virtual void Apply()
        {
            throw new NotImplementedException();
        }

        public TypeDefinition Type { get; set; }

        private static void unlock(FieldDefinition f)
        {
            unlock(f, null);
        }
        private static void unlock(FieldDefinition f, string name)
        {
            f.IsPublic = true;
            f.IsPrivate = false;
            f.IsInitOnly = false;
            if (!String.IsNullOrEmpty(name))
            {
                f.Name = name;
            }
        }

        public MethodDefinition GetInterfaceMethod(string name, string game = "Unturned")
        {
            return assembly.MainModule.GetType("Rocket." + game + ".Interface").Methods.AsEnumerable().Where(m => m.Name == name).FirstOrDefault();
        }

        public MethodDefinition GetMethod(string name)
        {
            return Type.Methods.Where(p => p.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public bool RemoveMethod(string name)
        {
            return Type.Methods.Remove(Type.Methods.Where(p => p.Name.ToLower() == name.ToLower()).FirstOrDefault());
        }

        public FieldDefinition GetField(string name)
        {
            return Type.Fields.Where(p => p.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public FieldDefinition[] GetFieldsByType(Type typeToUnlock)
        {
            return Type.Fields.Where(p => p.FieldType.FullName.Replace('/', '+') == typeToUnlock.FullName).ToArray();
        }

        public FieldDefinition[] GetFieldsByType(string typeToUnlock)
        {
            List<FieldDefinition> fields = Type.Fields.ToList();
            List<FieldDefinition> outFields = new List<FieldDefinition>();

            foreach (FieldDefinition field in fields)
            {
                string fieldname = field.FieldType.FullName;
                fieldname = fieldname.Replace("Steam/", "").Replace("System.Collections.Generic.List", "List").Replace("UnityEngine.", "").Replace("Steamworks.", "").Replace("List`1", "List").Replace("SDG.Unturned.", "");
                fieldname = fieldname.Split('.')[fieldname.Split('.').Count() - 1];
                if (fieldname.ToLower() == typeToUnlock.ToLower())
                {
                    outFields.Add(field);
                }
            }

            return outFields.ToArray();
        }
        private static void unlock(MethodDefinition m)
        {
            unlock(m, null);
        }
        private static void unlock(MethodDefinition m, string name)
        {
            m.IsPublic = true;
            m.IsPrivate = false;
            if (!String.IsNullOrEmpty(name))
            {
                m.Name = name;
            }
        }

        public void UnlockFieldByType(Type typeToUnlock)
        {
            UnlockFieldByType(typeToUnlock, null, 0);
        }

        public void UnlockFieldByType(Type typeToUnlock, string name)
        {
            UnlockFieldByType(typeToUnlock, name, 0);
        }
        /// <summary>
        /// Unlocks field with a type that matches a specifiy typen
        /// </summary>
        /// <param name="t"></param>
        /// <param name="typeToUnlock"></param>
        /// <param name="name"></param>
        public void UnlockFieldByType(Type typeToUnlock, string name, int index)
        {
            FieldDefinition[] fields = GetFieldsByType(typeToUnlock);

            if (fields.Count() >= index + 1 && fields[index] != null)
            {
                unlock(fields[index], name);
            }
            else
            {
                notFound(name);
            }
        }

        public void UnlockFieldByType(string typeToUnlock)
        {
            UnlockFieldByType(typeToUnlock, null, 0);
        }

        public void UnlockFieldByType(string typeToUnlock, string name)
        {
            UnlockFieldByType(typeToUnlock, name, 0);
        }
        /// <summary>
        /// Unlocks field with a type that matches a specifiy typename
        /// </summary>
        /// <param name="t"></param>
        /// <param name="typeToUnlock"></param>
        /// <param name="name"></param>
        public void UnlockFieldByType(string typeToUnlock, string name, int index)
        {
            FieldDefinition[] fields = GetFieldsByType(typeToUnlock);

            if (fields.Count() >= index + 1 && fields[index] != null)
            {
                unlock(fields[index], name);
            }
            else
            {
                notFound(name);
            }
        }
        public void UnlockFieldByName(string nameToUnlock)
        {
            UnlockFieldByName(nameToUnlock);
        }
        /// <summary>
        /// Unlocks field that matches a specifiy name
        /// </summary>
        /// <param name="t"></param>
        /// <param name="typeToUnlock"></param>
        /// <param name="name"></param>
        public void UnlockFieldByName(string nameToUnlock, string name)
        {
            FieldDefinition field = GetField(nameToUnlock);

            if (field != null)
            {
                unlock(field, name);
            }
            else
            {
                notFound(name);
            }
        }
        public void UnlockMethodByName(string nameToUnlock)
        {
            UnlockMethodByName(nameToUnlock, nameToUnlock);
        }
        /// <summary>
        /// Unlocks method that matches a specifiy name
        /// </summary>
        /// <param name="t"></param>
        /// <param name="typeToUnlock"></param>
        /// <param name="name"></param>
        public void UnlockMethodByName(string nameToUnlock, string name)
        {
            MethodDefinition method = GetMethod(nameToUnlock);

            if (method != null)
            {
                unlock(method, name);
            }
            else
            {
                notFound(name);
            }
        }

        private void notFound(string name)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: could not find " + Type.Name + " > " + name);
            Console.ForegroundColor = ConsoleColor.White;
#if DEBUG
            Console.ReadLine();
#endif
        }
    }


}