using Jurassic;
using Jurassic.Library;
using System;
using System.IO;
using Zorbo.Core;

namespace Javascript.Objects
{
    public class JSFile : ScriptObject
    {
        readonly Script script;

        public JSFile(Script script)
            : base(script.Engine) {

            this.script = script;
            this.PopulateFunctions();
        }
        
        [JSFunction(Name = "length", IsEnumerable = true, IsWritable = false)]
        public NumberInstance Length(object a) {
            
            if ((a is string || a is ConcatenatedString)) {

                FileInfo file = new FileInfo(Path.Combine(script.Directory, a.ToString()));
                return script.Engine.Number.Construct((double)file.Length);
            }

            return script.Engine.Number.Construct(0);
        }
        
        [JSFunction(Name = "create", IsEnumerable = true, IsWritable = false)]
        public bool Create(object a) {

            if ((a is string || a is ConcatenatedString)) {

                string path = Path.Combine(script.Directory, a.ToString());

                File.Create(path).Dispose();
                return true;
            }

            return false;
        }

        [JSFunction(Name = "delete", IsEnumerable = true, IsWritable = false)]
        public bool Delete(object a) {

            if ((a is string || a is ConcatenatedString)) {

                string path = Path.Combine(script.Directory, a.ToString());

                if (File.Exists(path)) {
                    File.Delete(path);
                    return true;
                }
            }

            return false;
        }

        [JSFunction(Name = "rename", IsEnumerable = true, IsWritable = false)]
        public bool Rename(object a, object b) {

            if ((a is string || a is ConcatenatedString) &&
                (b is string || b is ConcatenatedString)) {

                string path = Path.Combine(script.Directory, a.ToString());

                if (File.Exists(path)) {

                    File.Move(path, Path.Combine(script.Directory, b.ToString()));
                    File.Delete(path);

                    return true;
                }
            }
            return false;
        }


        [JSFunction(Name = "read", IsEnumerable = true, IsWritable = false, Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
        public string Read(object a) {

            if (a is string || a is ConcatenatedString)
                return File.ReadAllText(Path.Combine(script.Directory, a.ToString()));

            return null;
        }

        [JSFunction(Name = "readLines", IsEnumerable = true, IsWritable = false, Flags = JSFunctionFlags.ConvertNullReturnValueToUndefined)]
        public ArrayInstance ReadLines(object a) {

            if (a is string || a is ConcatenatedString) {

                string path = Path.Combine(script.Directory, a.ToString());

                if (File.Exists(path)) {

                    ArrayInstance ret = Engine.Array.Construct();

                    string[] lines = File.ReadAllLines(path);
                    lines.ForEach((s) => ret.Push(s));

                    return ret;
                }
            }

            return null;
        }


        [JSFunction(Name = "write", IsEnumerable = true, IsWritable = false)]
        public bool Write(object a, object b) {

            if ((a is string || a is ConcatenatedString) &&
                (b is string || b is ConcatenatedString)) {

                string path = Path.Combine(script.Directory, a.ToString());
                File.WriteAllText(path, b.ToString());

                return true;
            }

            return false;
        }

        [JSFunction(Name = "writeLines", IsEnumerable = true, IsWritable = false)]
        public bool WriteLines(object a, ArrayInstance b) {

            if (b != null && (a is String || a is ConcatenatedString)) {

                string path = Path.Combine(script.Directory, a.ToString());
                File.WriteAllLines(path, b.ToArray<string>());

                return true;
            }

            return false;
        }


        [JSFunction(Name = "append", IsEnumerable = true, IsWritable = false)]
        public bool Append(object a, object b) {

            if ((a is string || a is ConcatenatedString) &&
                (b is string || b is ConcatenatedString)) {

                string path = Path.Combine(script.Directory, a.ToString());
                File.AppendAllText(path, b.ToString());

                return true;
            }

            return false;
        }

        [JSFunction(Name = "appendLines", IsEnumerable = true, IsWritable = false)]
        public bool AppendLines(object a, ArrayInstance b) {

            if (b != null && (a is String || a is ConcatenatedString)) {

                string path = Path.Combine(script.Directory, a.ToString());
                File.AppendAllLines(path, b.ToArray<string>());

                return true;
            }

            return false;
        }
    }
}
