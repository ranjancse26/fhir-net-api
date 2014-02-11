﻿using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hl7.Fhir.Rest
{
    public class RestUrl
    {
        private UriBuilder _builder;
        private List<Tuple<string, string>> _parameters = new List<Tuple<string, string>>();


        public RestUrl(RestUrl url) : this(url.Uri)
        {
        }

        public RestUrl(Uri url)
        {
            if (!url.IsAbsoluteUri) throw Error.Argument("url", "Must be an absolute url");

            if (url.Scheme != "http")
                Error.Argument("uri", "RestUrl must be a http url");

            _builder = new UriBuilder(url);

            if (!String.IsNullOrEmpty(_builder.Query))
                _parameters = new List<Tuple<string,string>>( HttpUtil.SplitParams(_builder.Query) ); 
        }


        public RestUrl(string endpoint) : this(new Uri(endpoint,UriKind.RelativeOrAbsolute))
        {
        }


        public Uri Uri 
        { 
            get
            {
                _builder.Query = HttpUtil.JoinParams(_parameters);
                return _builder.Uri;
            } 
        }

        public string AsString
        {
            get
            {
                return Uri.ToString();
            }
        }


        private static string delimit(string path)
        {
            return path.EndsWith(@"/") ? path : path + @"/";
        }
        private static string prefix(string path)
        {
            return path.StartsWith(@"/") ? path : @"/"+path;
        }
        
        
        /// <summary>
        /// Add additional components to the end of the RestUrl
        /// </summary>
        /// <param name="components">one or more path components to add</param>
        /// <returns>The current RestUrl, so multiple AddPath statements can be combined in a fluent way.</returns>
        /// <example>If the current path is "http://hl7.org/svc", then adding ("fhir", "Patient") would
        /// return in a new RestUrl "http://hl7.org/svc/fhir/Patient"</example>
        public RestUrl AddPath(params string[] components)
        {
            string _components = string.Join("/", components).Trim('/');
            _builder.Path = delimit(_builder.Path)+ _components;
            return this;
        }

        /// <summary>
        /// Add a query parameter to the RestUrl
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RestUrl AddParam(string name, string value)
        {
            if (name == null) throw Error.ArgumentNull("name");
            if (value == null) throw Error.ArgumentNull("value");

            _parameters.Add(Tuple.Create(name, value));
            return this;
        }

        public RestUrl AddParam(Tuple<string, string> keyValue)
        {
            if (keyValue == null) throw Error.ArgumentNull("keyValue");

            return AddParam(keyValue.Item1, keyValue.Item2);
        }

        public bool IsEndpointFor(Uri other)
        {
            return IsEndpointFor(other.ToString());
        }

        public bool IsEndpointFor(string other)
        {
            var baseAddress = this.Uri.ToString();

            // HACK! To support Fiddler2 on Win8, localhost needs to be spelled out as localhost.fiddler, but still functions as localhost
            baseAddress = baseAddress.Replace("localhost.fiddler", "localhost");
            var baseUri = new Uri(delimit(baseAddress));

            other = delimit(other.Replace("localhost.fiddler", "localhost"));

            return baseUri.IsBaseOf(new Uri(other,UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Returs a new ResourceLocation that represents a location after navigating to the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <example>If the current path is "http://hl7.org/svc/patient", NavigatingTo("../observation") will 
        /// result in a ResourceLocation of "http://hl7.org/svc/observation" whereas if the current path is
        /// "http://hl7.org/svc/ (note the slash), NavigatingTo("../observation") will 
        /// result in a ResourceLocation of "http://hl7.org/svc/observation" 
        /// </example>
        public RestUrl NavigateTo(string path)
        {
            if (path == null) throw Error.ArgumentNull("path");

            return NavigateTo(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        public RestUrl NavigateTo(Uri path)
        {
            if (path == null) throw Error.ArgumentNull("path");

            if (path.IsAbsoluteUri)
                throw new ArgumentException("Can only navigate to relative paths", "path");

            return new RestUrl(new Uri(this.Uri, path));
        }

        public override string ToString()
        {
            return AsString;
        }    
    }    
}