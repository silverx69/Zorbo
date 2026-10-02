using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jurassic;
using Jurassic.Library;


namespace Javascript.Objects
{
    public class HttpRequest : ScriptObject
    {
        Uri uri;
        String method = "GET";

        [JSProperty(Name = "uri", IsEnumerable = true)]
        public string Uri {
            get { return (uri == null) ? String.Empty : uri.ToString(); }
            set {
                if (String.IsNullOrEmpty(value)) {
                    uri = null;
                    return;
                }

                if (!value.ToLower().StartsWith("http://"))
                    value = "http://" + value;

                uri = new Uri(value, UriKind.Absolute);
            }
        }

        [JSProperty(Name = "method", IsEnumerable = true)]
        public string Method {
            get { return method; }
            set { method = value; }
        }

        [JSProperty(Name = "headers", IsEnumerable = true)]
        public List Headers {
            get;
            private set;
        }

        [JSProperty(Name = "content", IsEnumerable = true)]
        public string PostContent {
            get;
            set;
        }

        #region " Constructor "

        public class Constructor : ClrFunction
        {
            readonly Script script;

            public Constructor(Script script)
                : base(script.Engine.Function.InstancePrototype, "HttpRequest", new HttpRequest(script)) {

                this.script = script;
            }

            [JSCallFunction]
            public HttpRequest Call(string a) {
                return new HttpRequest(script, a);
            }

            [JSConstructorFunction]
            public HttpRequest Construct(string a) {
                return new HttpRequest(script, a);
            }
        }

        #endregion

        internal HttpRequest(Script script)
            : base(script.Engine) {
            this.Headers = new List(script);
            this.PopulateFunctions();
        }

        public HttpRequest(Script script, string url)
            : base(script.Engine, ((ClrFunction)script.Engine.Global["HttpRequest"]).InstancePrototype) {
            this.Uri = url;
            this.Headers = new List(script);

            this.PopulateFunctions();
        }

        [JSFunction(Name = "download", IsEnumerable = true, IsWritable = false)]
        public bool Download(FunctionInstance callback) {
            if (callback == null) return false;

            try {
                var request = (HttpWebRequest)HttpWebRequest.Create(uri);

                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
                request.Method = method.ToUpper();

                foreach (object obj in Headers.Items) {
                    string str = obj.ToString();

                    if (str.StartsWith("host"))
                        request.Host = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("content-type"))
                        request.ContentType = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("content-length")) {

                        if (Int32.TryParse(str.Substring(str.IndexOf(':') + 1), out int length))
                            request.ContentLength = length;
                    }
                    else if (str.StartsWith("connection"))
                        request.Connection = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("date")) {
                        DateTime date = DateTime.MinValue;

                        if (DateTime.TryParse(str.Substring(str.IndexOf(':') + 1), out date))
                            request.Date = date;
                    }
                    else if (str.StartsWith("expect"))
                        request.Expect = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("accept"))
                        request.Accept = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("user-agent"))
                        request.UserAgent = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("referer"))
                        request.Referer = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("TE"))
                        request.TransferEncoding = str.Substring(str.IndexOf(':') + 1).Trim();

                    else if (str.StartsWith("if-modified-since")) {
                        DateTime date = DateTime.MinValue;

                        if (DateTime.TryParse(str.Substring(str.IndexOf(':') + 1), out date))
                            request.IfModifiedSince = date;
                    }
                    else request.Headers.Add(str);
                }

                if (request.Method == "POST") {

                    if (!String.IsNullOrEmpty(PostContent)) {
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(PostContent);
                        request.ContentLength = buffer.Length;

                        var stream = request.GetRequestStream();
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    else request.ContentLength = 0;
                }


                request.BeginGetResponse((IAsyncResult ar) => {
                    try {
                        var response = request.EndGetResponse(ar);
                        var stream = response.GetResponseStream();

                        var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                        string page = reader.ReadToEnd();

                        callback.Call(this, page);
                    }
                    catch (JavaScriptException jex) {
                        JurassicPlugin.Self.OnError(jex);
                    }
                    catch (Exception ex) {
                        JurassicPlugin.Self.OnError(new JavaScriptException(Engine, ErrorType.Error, ex.Message, ex));
                    }
                }, null);

                return true;
            }
            catch (Exception e) {
                JurassicPlugin.Self.OnError(new JavaScriptException(Engine, ErrorType.Error, e.Message, e));
            }

            return false;
        }
    }
}
